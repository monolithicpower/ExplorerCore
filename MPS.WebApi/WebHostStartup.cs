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
using MPS.Infrastructure.Helper;
using MPS.Infrastructure.Pattern.IOC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace MPS.WebApi
{
    //这个类名不重要，重要的是里面的方法一定要正确，
    public class WebHostStartup
    {
        private readonly IWebHostEnvironment _env;

        public IConfiguration Configuration { get; }

        public IHostEnvironment HostEnvironment { get; set; }

        // 如果要重写构造函数，就要使用这个进行重写。
        public WebHostStartup(IConfiguration configuration, IHostEnvironment hostEnvironment, IWebHostEnvironment env)
        {
            Configuration = configuration;
            _env = env;
            HostEnvironment = hostEnvironment;
        }

        //先调用这个方法配置服务，  services 对象提供了各种 Add{Services}  方法,如方法内注释所示
        //而这些方法都是再不同的命名空间内写的扩展方法
        public void ConfigureServices(IServiceCollection services)
        {
            //Microsoft.Extensions.DependencyInjection.MvcServiceCollectionExtensions
            //services.AddMvc();
            //services.AddRazorPages();
            //Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions
            //services.AddSingleton(typeof(object));
            //既然  Add{Services} 是各路扩展，那么扩展内容里面具体添加了什么呢？
            //为什么把这部分放到扩展里？怀疑windows不想提供这部分源码。
            // *常用的服务有（部分服务框架已默认注册）：
            //  ·AddControllers：注册Controller相关服务，内部调用了AddMvcCore、AddApiExplorer、AddAuthorization、AddCors、AddDataAnnotations、AddFormatterMappings等多个扩展方法
            //  ·AddOptions：注册Options相关服务，如IOptions <>、IOptionsSnapshot <>、IOptionsMonitor <>、IOptionsFactory <>、IOptionsMonitorCache<> 等。很多服务都需要Options，所以很多服务注册的扩展方法会在内部调用AddOptions
            //  ·AddRouting：注册路由相关服务，如IInlineConstraintResolver、LinkGenerator、IConfigureOptions < RouteOptions >、RoutePatternTransformer等
            //  ·AddAddLogging：注册Logging相关服务，如ILoggerFactory、ILogger <>、IConfigureOptions < LoggerFilterOptions >> 等
            //  ·AddAuthentication：注册身份认证相关服务，以方便后续注册JwtBearer、Cookie等服务
            //  ·AddAuthorization：注册用户授权相关服务
            //  ·AddMvc：注册Mvc相关服务，比如Controllers、Views、RazorPages等
            //  ·AddHealthChecks：注册健康检查相关服务，如HealthCheckService、IHostedService等

            //开启文件路径浏览功能  不管是FileServer  还是   DirectoryBrowser  都需要他
            services.AddDirectoryBrowser();
        }

        // 在这里设置中间件是否启用;中间件就是ASP.NET的封装了，帮助开发省略了解析Http协议的部分。语法基本都是  Use{Middleware} 的扩展方法           
        //  ASP.NET Core 模板配置的管道支持：
        //  ·开发人员异常页                            UseDeveloperExceptionPage
        //  ·异常处理程序
        //  ·HTTP 严格传输安全性(HSTS)
        //  ·HTTPS 重定向
        //  ·静态文件
        //  ·ASP.NET Core MVC 和 Razor Pages
        //  ·路由 必须与 UseEndpoints 搭配使用           UseRouting
        //  ·UseEndpoints：执行路由所选择的Endpoint对应的委托
        //  ·UseAuthentication：身份认证中间件  //是否可以登录
        //  ·UseAuthorization：用户授权中间件，用于对请求用户进行授权。//是否拥有权限
        //  ·UseMvc：Mvc中间件
        //  ·UseHealthChecks：健康检查中间件
        //  ·UseMiddleware：用来添加匿名中间件的，通过该方法，可以方便的添加自定义中间件
        //  ·app.Use((context, next) =>{});  直接用的中间件
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
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

            IocManager.Instance.RegisterDll("MPS.Implement.dll");
            var staticFilePath = Path.Combine($"{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}", "..\\MyStaticFiles");

            app.Map("/GetAllFiles", app0 =>
            {
                app0.Run(async context =>
                {
                    var filePath = context.Request.Form["FilePath"];
                    var extensions = context.Request.Form["Extensions"].ToString().Split(',');
                    //var path = $"{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}/FilePath";
                    var fd = new FolderHelper();
                    var result = fd.GetAllFiles($"{staticFilePath}/{filePath}", extensions);
                    await context.Response.WriteAsync(Newtonsoft.Json.JsonConvert.SerializeObject(result));
                });
            });

            //app.UseHttpsRedirection();

            //FileExtensionContentTypeProvider 类包含 Mappings 属性，用作文件扩展名到 MIME 内容类型的映射。
            //在  StaticFileOptions  才能设置  没能理解干嘛的
            var provider = new FileExtensionContentTypeProvider();
            // Add new mappings
            provider.Mappings[".myapp"] = "application/x-msdownload";
            provider.Mappings[".png"] = "MP2960/";
            provider.Mappings.Remove(".jpg");

            //Path.Combine 的 用法
            //var path = Path.Combine(env.ContentRootPath, "../", "MyStaticFiles");
            //var path = Path.Combine(env.ContentRootPath, "bin", "Debug", "MyStaticFiles");

            //配置默认文件，不设置路径就在wwwroot里面，必须在调用 UseStaticFiles 之前
            //这玩意儿只能配合 wwwroot 使用
            var dOps = new DefaultFilesOptions();
            dOps.DefaultFileNames.Clear();
            dOps.DefaultFileNames.Add("htmlpage.html");
            app.UseDefaultFiles(dOps);

            //配置中间件提供静态文件。  这玩意儿只能返回.html文件  
            //静态文件存储在项目的 Web 根目录中。 默认目录为 {content root}/wwwroot，但可通过 传入 StaticFileOptions 更改目录
            // using Microsoft.Extensions.FileProviders; PhysicalFileProvider
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
            //app.UseDirectoryBrowser(new DirectoryBrowserOptions
            //{
            //    FileProvider = new PhysicalFileProvider(path),
            //    RequestPath = "/StaticFiles"
            //});

            //上面三个的集合体,可以直接不配上面两个的更多参数，UseStaticFiles使用无参以使能wwwroot文件夹的东西
            //浏览器能打开的文件都可以浏览了。包括图片啥的
            //app.UseFileServer(new FileServerOptions
            //{
            //    FileProvider = new PhysicalFileProvider(path),
            //    RequestPath = "/StaticFiles",
            //    EnableDirectoryBrowsing = true,//开启文件浏览，可以直接替代上面的配置
            //});

            //app.UseRouting();

            //app.UseAuthorization();

            //app.UseEndpoints(endpoints =>
            //{
            //    endpoints.MapRazorPages();
            //});

            //使用自定义的中间件
            //app.UseMiddleware<object>(new object());

            app.Map("/DownloadFile", app0 =>
            {
                app0.Run(async context =>
                {
                    var path = context.Request.Form["FilePath"];
                    if (File.Exists(path))
                    {
                        using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                        var sr = new StreamReader(fs);
                        await context.Response.WriteAsync(sr.ReadToEnd());
                        return;
                    }
                    await context.Response.WriteAsync("");
                });
            });

            app.Map("/DeleteFile", app0 =>
            {
                app0.Run(async context =>
                {
                    try
                    {
                        var filePath = context.Request.Form["FilePath"];
                        var realFp = $"{staticFilePath}/{filePath}";
                        var path = Path.GetDirectoryName(realFp);
                        if (!Directory.Exists(path))
                            await context.Response.WriteAsync("Directory not exist");
                        else if (File.Exists(realFp))
                        {
                            File.Delete(realFp);
                            await context.Response.WriteAsync("Delete success");
                        }
                        else
                            await context.Response.WriteAsync("File not exist");
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
                        var lastWriteTime = DateTime.Parse(context.Request.Form["LastWriteTime"]);
                        var file = files[0];
                        var realFp = $"{staticFilePath}/{filePath}";
                        var path = Path.GetDirectoryName(realFp);
                        if (!Directory.Exists(path))
                            Directory.CreateDirectory(path);
                        if (File.Exists(realFp))
                            File.Delete(realFp);
                        using var fs = new FileStream(realFp, FileMode.Create, FileAccess.Write);
                        await file.CopyToAsync(fs);
                        fs.Dispose();
                        var fi = new FileInfo(realFp);
                        fi.LastWriteTime = lastWriteTime;
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
