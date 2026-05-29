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
using System.Threading;
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

        public string RemotePath { get; set; }

        // 如果要重写构造函数，就要使用这个进行重写。
        public WebHostStartup(IConfiguration configuration, IHostEnvironment hostEnvironment, IWebHostEnvironment env)
        {
            Configuration = configuration;
            _env = env;
            HostEnvironment = hostEnvironment;
            RemotePath = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent + @"\MyStaticFiles");
        }

        //先调用这个方法配置服务，  services 对象提供了各种 Add{Services}  方法,如方法内注释所示
        //而这些方法都是再不同的命名空间内写的扩展方法
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDirectoryBrowser();
        }

        private FolderInfo GetAllFiles(string path, string[] fileExtensions)
        {
            FolderInfo folderInfo = new FolderInfo(Path.GetFileNameWithoutExtension(path));
            string[] directories = Directory.GetDirectories(path);
            foreach (string path3 in directories)
            {
                FolderInfo allFiles = GetAllFiles(path3, fileExtensions);
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
                    try
                    {
                        var filePath = context.Request.Form["FilePath"];
                        if (filePath.Contains("../") || filePath.Contains("..\\"))
                        {
                            await context.Response.WriteAsync(null);
                        }
                        else
                        {
                            var extensions = context.Request.Form["Extensions"].ToString().Split(',');
                            var includeSubDirs = context.Request.Form["IncludeSubDirs"].ToString();
                            var fd = new FolderHelper();
                            bool includeSubDirsBoolean = string.IsNullOrEmpty(includeSubDirs) || includeSubDirs?.ToLower() == "true" || includeSubDirs?.ToLower() == "1";
                            var result = fd.GetAllFiles($"{staticFilePath}/{filePath}", extensions, includeSubDirsBoolean);
                            await context.Response.WriteAsync(Newtonsoft.Json.JsonConvert.SerializeObject(result));
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Get All Files Exception:\n" + ex.Message);
                    }
                    finally
                    {
                        
                    }
                });
            });

            app.Map("/DownloadFile", app0 =>
            {
                app0.Run(async context =>
                {
                    try
                    {
                        var path = context.Request.Form["FilePath"];
                        if (path.Contains("../") || path.Contains("..\\"))
                        {
                            await context.Response.WriteAsync("Return to upper layer operation is not allowed");
                        }
                        else
                        {
                            path = Path.Combine(RemotePath, path);
                            if (File.Exists(path))
                            {
                                var result = await File.ReadAllBytesAsync(path);
                                if (result != null)
                                {
                                    await context.Response.WriteAsync(Convert.ToBase64String(result));
                                }
                                else
                                {
                                    await context.Response.WriteAsync("Read file byte array error");
                                    Console.WriteLine($"Download file '{path}' error file byte array null");
                                }
                            }
                            else
                            {
                                await context.Response.WriteAsync($"File '{path}' not exist");
                                Console.WriteLine($"Download file '{path}' error file not exist");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Download file exception:\n" + ex.StackTrace);
                    }
                    finally
                    {

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
                            var realFp = $"{RemotePath}/{filePath}";
                            var path = Path.GetDirectoryName(realFp);
                            if (File.Exists(realFp))
                            {
                                File.Delete(realFp);
                            }
                            if (File.Exists(realFp))
                            {
                                await context.Response.WriteAsync($"Delete file '{realFp}' error");
                                Console.WriteLine($"Delete file '{realFp}' error");
                            }
                            else
                            {
                                await context.Response.WriteAsync($"Delete file '{realFp}' success");
                                Console.WriteLine($"Delete file '{realFp}' success");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Delete file exception:\n" + ex.Message);
                    }
                    finally
                    {

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
                            var realFp = $"{RemotePath}/{foldlerPath}";
                            var path = Path.GetDirectoryName(realFp);
                            if (!Directory.Exists(path))
                            {
                                await context.Response.WriteAsync("Directory not exist");
                            }
                            else
                            {
                                if (Directory.Exists(realFp))
                                {
                                    Directory.Delete(realFp, true);
                                }
                                if (Directory.Exists(realFp))
                                {
                                    await context.Response.WriteAsync($"Delete foler {realFp} error");
                                    Console.WriteLine($"Delete folder '{realFp}' error");
                                }
                                else
                                {
                                    await context.Response.WriteAsync($"Delete foler {realFp} success");
                                    Console.WriteLine($"Delete folder '{realFp}' success");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Delete folder exception:\n" + ex.Message);
                    }
                    finally
                    {

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
                            var realFp = $"{RemotePath}/{filePath}";
                            var path = Path.GetDirectoryName(realFp);
                            if (!Directory.Exists(path))
                                Directory.CreateDirectory(path);
                            using var fs = new FileStream(realFp, FileMode.Create, FileAccess.Write);
                            await file.CopyToAsync(fs);
                            fs?.Dispose();
                            var fi = new FileInfo(realFp);
                            fi.LastWriteTime = DateTime.Now;
                            Console.WriteLine(DateTime.Now.ToString() + "\tUpload File\t" + "Success");
                            await context.Response.WriteAsync("Upload success");
                        }
                    }
                    catch (Exception ex)
                    {
                        await context.Response.WriteAsync("Upload file exception:\n" + ex.Message);
                        Console.WriteLine("Upload file exception:\n" + ex.StackTrace);
                    }
                    finally
                    {
                    
                    }
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

    public class TimedHostedService : BackgroundService
    {
        private Timer _timer;

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var delay = TimeSpan.FromSeconds(15);
            _timer = new Timer(DoWork, null, TimeSpan.Zero, delay);
            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            await Task.CompletedTask;
        }

        public override void Dispose()
        {
            _timer?.Dispose();
        }
    }

}
