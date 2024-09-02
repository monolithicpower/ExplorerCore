using FluentFTP;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MPS.HZ.Core.Folders;
using MPS.Infrastructure.Folder;
using MPS.Infrastructure.Helper;
using MPS.Infrastructure.Pattern.IOC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using IApplicationLifetime = Microsoft.AspNetCore.Hosting.IApplicationLifetime;

namespace MPS.WebApi
{
    //这个类名不重要，重要的是里面的方法一定要正确，
    public class WebHostStartup
    {
        private readonly IWebHostEnvironment _env;

        public IConfiguration Configuration { get; }

        public IHostEnvironment HostEnvironment { get; set; }

        private FtpClient FtpClient { get; set; }

        public string RemotePath { get; set; }

        // 如果要重写构造函数，就要使用这个进行重写。
        public WebHostStartup(IConfiguration configuration, IHostEnvironment hostEnvironment, IWebHostEnvironment env)
        {
            Configuration = configuration;
            _env = env;
            HostEnvironment = hostEnvironment;
            var cfg = new FtpConfig();
            cfg.ConnectTimeout = 60000;
            cfg.DataConnectionConnectTimeout = 60000;
            cfg.DataConnectionReadTimeout = 60000;
            cfg.ReadTimeout = 60000;
            cfg.SocketKeepAlive = true;
            FtpClient = new FtpClient("10.10.84.204", 21, cfg);
            RemotePath = "MyStaticFiles";
        }

        //先调用这个方法配置服务，  services 对象提供了各种 Add{Services}  方法,如方法内注释所示
        //而这些方法都是再不同的命名空间内写的扩展方法
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDirectoryBrowser();
        }

        private FolderInfo GetAllFiles(string path, string[] fileExtensions, FtpClient ftpClient)
        {
            FolderInfo folderInfo = new FolderInfo(Path.GetFileNameWithoutExtension(path));
            string[] directories = Directory.GetDirectories(path);
            foreach (string path3 in directories)
            {
                FolderInfo allFiles = GetAllFiles(path3, fileExtensions, ftpClient);
                folderInfo.ChildFolders.Add(allFiles);
            }
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            FileInfo[] files = directoryInfo.GetFiles();
            IEnumerable<FileInfo> enumerable = from p in directoryInfo.GetFiles()
                                               where fileExtensions == null || fileExtensions.Length == 0 || fileExtensions.Contains(p.Extension.ToLower())
                                               select p;
            foreach (FileInfo item in enumerable)
            {
                MpsFileInfo mpsFileInfo = new MpsFileInfo();
                mpsFileInfo.SetInfo(item);
                folderInfo.FileInfos.Add(mpsFileInfo);
            }
            return folderInfo;
        }


        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApplicationLifetime applicationLifetime)
        {
            applicationLifetime.ApplicationStopping.Register(() =>
            {
                FtpClient?.Dispose();
            });
            //·开发人员异常页//常用的中间件，如果方法里面错了没有catch 会展示错误信息
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            //else
            //{
            //    app.UseExceptionHandler("/Error");
            //    app.UseHsts();
            //}
            var staticFilePath = Path.Combine($"{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}", "..\\MyStaticFiles");

            //app.UseHttpsRedirection();

            //FileExtensionContentTypeProvider 类包含 Mappings 属性，用作文件扩展名到 MIME 内容类型的映射。
            //在  StaticFileOptions  才能设置  没能理解干嘛的
            var provider = new FileExtensionContentTypeProvider();
            // Add new mappings
            provider.Mappings[".myapp"] = "application/x-msdownload";
            provider.Mappings[".png"] = "MP2960/";
            provider.Mappings.Remove(".jpg");

            //配置默认文件，不设置路径就在wwwroot里面，必须在调用 UseStaticFiles 之前
            //这玩意儿只能配合 wwwroot 使用
            var dOps = new DefaultFilesOptions();
            dOps.DefaultFileNames.Clear();
            dOps.DefaultFileNames.Add("htmlpage.html");
            app.UseDefaultFiles(dOps);

            //配置中间件提供静态文件。  这玩意儿只能返回.html文件  
            //静态文件存储在项目的 Web 根目录中。 默认目录为 {content root}/wwwroot，但可通过 传入 StaticFileOptions 更改目录
            // using Microsoft.Extensions.FileProviders; PhysicalFileProvider
            Console.WriteLine(staticFilePath);
            app.UseStaticFiles(
                new StaticFileOptions
                {
                    //Path.Combine支持这么使用：Path.Combine(env.ContentRootPath, "../outImgs")
                    FileProvider = new PhysicalFileProvider(staticFilePath),//文件夹绝对路径
                    RequestPath = "/StaticFiles",//在链接中的访问字符，实际使用就会被替换成上面的路径
                                                 //"~/StaticFiles/images/red-rose.jpg"  即   "~/MyStaticFiles/Tmp/images/red-rose.jpg" 
                                                 //ContentTypeProvider = provider,
                }
                );

            //上面那个一个套路
            //上面那个的升级版本，毕竟StaticFile只能返回.html  这个可以返回所有目录以及文件
            //但是具体的文件是浏览不了的。  名字设置成一样的话，可以浏览其中的html页面。
            app.UseDirectoryBrowser(new DirectoryBrowserOptions
            {
                FileProvider = new PhysicalFileProvider(staticFilePath),
                RequestPath = "/StaticFiles"
            });

            //上面三个的集合体,可以直接不配上面两个的更多参数，UseStaticFiles使用无参以使能wwwroot文件夹的东西
            //浏览器能打开的文件都可以浏览了。包括图片啥的
            app.UseFileServer(new FileServerOptions
            {
                FileProvider = new PhysicalFileProvider(staticFilePath),
                RequestPath = "/StaticFiles",
                EnableDirectoryBrowsing = true,//开启文件浏览，可以直接替代上面的配置
            });

            //app.UseRouting();

            //app.UseAuthorization();

            //app.UseEndpoints(endpoints =>
            //{
            //    endpoints.MapRazorPages();
            //});

            //使用自定义的中间件
            //app.UseMiddleware<object>(new object());

            app.Map("/GetAllFiles", app0 =>
            {
                app0.Run(async context =>
                {
                    var filePath = context.Request.Form["FilePath"];
                    if (filePath.Contains("../") || filePath.Contains("..\\"))
                    {
                        await context.Response.WriteAsync(null);
                    }
                    else
                    {
                        //var extensions = context.Request.Form["Extensions"].ToString().Split(',');
                        //var fd = new FolderHelper();
                        //var result = fd.GetAllFiles($"{staticFilePath}/{filePath}", extensions);
                        //await context.Response.WriteAsync(Newtonsoft.Json.JsonConvert.SerializeObject(result));

                        var folderPath = Path.Combine(RemotePath, filePath);
                        if (!FtpClient.IsConnected)
                        {
                            FtpClient.AutoConnect();
                        }
                        if (!FtpClient.DirectoryExists(folderPath))
                        {
                            FtpClient.CreateDirectory(folderPath);
                        }
                        else
                        {
                            string tempPath = Path.Combine(_env.WebRootPath, "Temp");
                            if (!Directory.Exists(tempPath))
                            {
                                Directory.CreateDirectory(tempPath);
                            }
                            if (!FtpClient.IsConnected)
                            {
                                FtpClient.AutoConnect();
                            }
                            FtpClient.DownloadDirectory(tempPath, folderPath);
                            var extensions = context.Request.Form["Extensions"].ToString().Split(',');
                            var fd = new FolderHelper();
                            var result = fd.GetAllFiles(Path.Combine(tempPath, folderPath), extensions);
                            await context.Response.WriteAsync(Newtonsoft.Json.JsonConvert.SerializeObject(result));
                        }
                    }
                });
            });

            app.Map("/DownloadFile", app0 =>
            {
                app0.Run(async context =>
                {
                    var path = context.Request.Form["FilePath"];
                    if (path.Contains("../") || path.Contains("..\\"))
                    {
                        await context.Response.WriteAsync(null);
                    }
                    else
                    {
                        //if (File.Exists(path))
                        //{
                        //    using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                        //    var sr = new StreamReader(fs);
                        //    await context.Response.WriteAsync(sr.ReadToEnd());
                        //    return;
                        //}
                        //await context.Response.WriteAsync("");

                        var filePathRemote = Path.Combine(RemotePath, path);
                        if (!FtpClient.IsConnected)
                        {
                            FtpClient.AutoConnect();
                        }
                        if (FtpClient.FileExists(filePathRemote))
                        {
                            byte[] fileBytes = null;
                            if (!FtpClient.IsConnected)
                            {
                                FtpClient.AutoConnect();
                            }
                            var result = FtpClient.DownloadBytes(out fileBytes, filePathRemote);
                            if (result)
                            {
                                await context.Response.WriteAsync(Convert.ToBase64String(fileBytes));
                            }
                            else
                            {
                                await context.Response.WriteAsync("Download file error");
                            }
                            fileBytes = null;
                        }
                        else
                        {
                            await context.Response.WriteAsync("File not exist");
                        }
                    }
                });
            });

            app.Map("/DeleteFile", app0 =>
            {
                app0.Run(async context =>
                {
                    try
                    {
                        var filePath = context.Request.Form["FilePath"];
                        if (filePath.Contains("../") || filePath.Contains("..\\"))
                        {
                            await context.Response.WriteAsync("Return to upper layer operation is not allowed");
                        }
                        else
                        {
                            var realFp = $"{staticFilePath}/{filePath}";
                            var path = Path.GetDirectoryName(realFp);
                            //if (!Directory.Exists(path))
                            //    await context.Response.WriteAsync("File not exist");
                            if (File.Exists(realFp))
                            {
                                File.Delete(realFp);
                                //await context.Response.WriteAsync("Delete success");
                            }

                            var filePathRemote = Path.Combine(RemotePath, filePath);
                            var folderPath = Path.GetDirectoryName(filePathRemote);
                            if (!FtpClient.IsConnected)
                            {
                                FtpClient.AutoConnect();
                            }
                            if (!FtpClient.DirectoryExists(folderPath))
                            {
                                await context.Response.WriteAsync("Directory not exist");
                            }
                            else if (FtpClient.FileExists(filePathRemote))
                            {
                                if (!FtpClient.IsConnected)
                                {
                                    FtpClient.AutoConnect();
                                }
                                FtpClient.DeleteFile(filePathRemote);
                                if (File.Exists(Path.Combine(_env.WebRootPath, "Temp", filePathRemote)))
                                {
                                    File.Delete(Path.Combine(_env.WebRootPath, "Temp", filePathRemote));
                                }
                                await context.Response.WriteAsync("Delete success");
                            }
                            else
                            {
                                await context.Response.WriteAsync("File not exist");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                });
            });

            app.Map("/DeleteFolder", app0 =>
            {
                app0.Run(async context =>
                {
                    try
                    {
                        var foldlerPath = context.Request.Form["FilePath"];
                        if (foldlerPath.Contains("../") || foldlerPath.Contains("..\\"))
                        {
                            await context.Response.WriteAsync("Return to upper layer operation is not allowed");
                        }
                        else
                        {
                            var dirs = foldlerPath.ToString().Split('\\', '/');
                            if (dirs.Length < 4)
                                await context.Response.WriteAsync("The directory hierarchy should not be less than 4.");
                            var realFp = $"{staticFilePath}/{foldlerPath}";
                            var path = Path.GetDirectoryName(realFp);
                            var foldlerPathRemote = Path.Combine(RemotePath, foldlerPath);
                            if (!Directory.Exists(path))
                            {
                                await context.Response.WriteAsync("Directory not exist");
                                if (!FtpClient.IsConnected)
                                {
                                    FtpClient.AutoConnect();
                                }
                                if (FtpClient.DirectoryExists(foldlerPathRemote))
                                {
                                    FtpClient.DeleteDirectory(foldlerPathRemote);
                                }
                                if (Directory.Exists(Path.Combine(_env.WebRootPath, "Temp", foldlerPathRemote)))
                                {
                                    Directory.Delete(Path.Combine(_env.WebRootPath, "Temp", foldlerPathRemote), true);
                                }
                            }
                            else
                            {
                                if (Directory.Exists(realFp))
                                {
                                    Directory.Delete(realFp, true);
                                }
                                //await context.Response.WriteAsync("Delete success");
                                if (!FtpClient.IsConnected)
                                {
                                    FtpClient.AutoConnect();
                                }
                                if (FtpClient.DirectoryExists(foldlerPathRemote))
                                {
                                    FtpClient.DeleteDirectory(foldlerPathRemote);
                                }
                                if (Directory.Exists(Path.Combine(_env.WebRootPath, "Temp", foldlerPathRemote)))
                                {
                                    Directory.Delete(Path.Combine(_env.WebRootPath, "Temp", foldlerPathRemote), true);
                                }
                                await context.Response.WriteAsync("Delete success");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                });
            });

            app.Map("/UploadFile", app0 =>
            {
                app0.Run(async context =>
                {
                    try
                    {
                        var files = context.Request.Form.Files;
                        var filePath = context.Request.Form["FilePath"];
                        if (filePath.Contains("../") || filePath.Contains("..\\"))
                        {
                            await context.Response.WriteAsync(null);
                            return;
                        }
                        else
                        {
                            var lastWriteTime = DateTime.Parse(context.Request.Form["LastWriteTime"]);
                            var file = files[0];
                            var realFp = $"{staticFilePath}/{filePath}";
                            var path = Path.GetDirectoryName(realFp);
                            if (!Directory.Exists(path))
                                Directory.CreateDirectory(path);
                            using var fs = new FileStream(realFp, FileMode.Create, FileAccess.Write);
                            await file.CopyToAsync(fs);
                            fs.Dispose();
                            var fi = new FileInfo(realFp);
                            fi.LastWriteTime = DateTime.Now;

                            var tempfs2Path = Path.Combine(_env.WebRootPath, "Temp", RemotePath, filePath);
                            if (!Directory.Exists(Path.GetDirectoryName(tempfs2Path)))
                            {
                                Directory.CreateDirectory(Path.GetDirectoryName(tempfs2Path));
                            }
                            var fs2 = new FileStream(tempfs2Path, FileMode.Create, FileAccess.Write);
                            await file.CopyToAsync(fs2);
                            fs2.Dispose();
                            var fi2 = new FileInfo(tempfs2Path);
                            fi2.LastWriteTime = DateTime.Now;

                            var filePathRemote = Path.Combine(RemotePath, filePath);
                            var folderPath = Path.GetDirectoryName(filePathRemote);
                            if (!FtpClient.IsConnected)
                            {
                                FtpClient.AutoConnect();
                            }
                            if (!FtpClient.DirectoryExists(folderPath))
                            {
                                FtpClient.CreateDirectory(folderPath);
                            }
                            if (!FtpClient.IsConnected)
                            {
                                FtpClient.AutoConnect();
                            }
                            var state = FtpClient.UploadFile(realFp, filePathRemote, FtpRemoteExists.Overwrite);
                            Console.WriteLine(DateTime.Now.ToString() + "\tUpload File\t" + state);
                        }
                    }
                    catch (Exception ex)
                    {
                        await context.Response.WriteAsync(ex.Message);
                        Console.WriteLine(ex);
                        return;
                    }
                    await context.Response.WriteAsync("Upload success");
                });
            });

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello World");
            });
        }
    }

    public class RequestSetOptionsMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestSetOptionsMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        // Test with https://localhost:5001/Privacy/?option=Hello
        public async Task Invoke(HttpContext httpContext)
        {
            var option = httpContext.Request.Query["option"];

            if (!string.IsNullOrWhiteSpace(option))
            {
                httpContext.Items["option"] = WebUtility.HtmlEncode(option);
            }

            await _next(httpContext);
        }
    }

    public class RequestSetOptionsStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return builder =>
            {
                builder.UseMiddleware<RequestSetOptionsMiddleware>();
                next(builder);
            };
        }
    }
}
