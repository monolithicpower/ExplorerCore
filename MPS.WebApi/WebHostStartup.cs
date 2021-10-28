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
    //�����������Ҫ����Ҫ��������ķ���һ��Ҫ��ȷ��
    public class WebHostStartup
    {
        private readonly IWebHostEnvironment _env;

        public IConfiguration Configuration { get; }

        public IHostEnvironment HostEnvironment { get; set; }

        // ���Ҫ��д���캯������Ҫʹ�����������д��
        public WebHostStartup(IConfiguration configuration, IHostEnvironment hostEnvironment, IWebHostEnvironment env)
        {
            Configuration = configuration;
            _env = env;
            HostEnvironment = hostEnvironment;
        }

        //�ȵ�������������÷���  services �����ṩ�˸��� Add{Services}  ����,�緽����ע����ʾ
        //����Щ���������ٲ�ͬ�������ռ���д����չ����
        public void ConfigureServices(IServiceCollection services)
        {
            //Microsoft.Extensions.DependencyInjection.MvcServiceCollectionExtensions
            //services.AddMvc();
            //services.AddRazorPages();
            //Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions
            //services.AddSingleton(typeof(object));
            //��Ȼ  Add{Services} �Ǹ�·��չ����ô��չ����������������ʲô�أ�
            //Ϊʲô���ⲿ�ַŵ���չ�����windows�����ṩ�ⲿ��Դ�롣
            // *���õķ����У����ַ�������Ĭ��ע�ᣩ��
            //  ��AddControllers��ע��Controller��ط����ڲ�������AddMvcCore��AddApiExplorer��AddAuthorization��AddCors��AddDataAnnotations��AddFormatterMappings�ȶ����չ����
            //  ��AddOptions��ע��Options��ط�����IOptions <>��IOptionsSnapshot <>��IOptionsMonitor <>��IOptionsFactory <>��IOptionsMonitorCache<> �ȡ��ܶ������ҪOptions�����Ժܶ����ע�����չ���������ڲ�����AddOptions
            //  ��AddRouting��ע��·����ط�����IInlineConstraintResolver��LinkGenerator��IConfigureOptions < RouteOptions >��RoutePatternTransformer��
            //  ��AddAddLogging��ע��Logging��ط�����ILoggerFactory��ILogger <>��IConfigureOptions < LoggerFilterOptions >> ��
            //  ��AddAuthentication��ע�������֤��ط����Է������ע��JwtBearer��Cookie�ȷ���
            //  ��AddAuthorization��ע���û���Ȩ��ط���
            //  ��AddMvc��ע��Mvc��ط��񣬱���Controllers��Views��RazorPages��
            //  ��AddHealthChecks��ע�ὡ�������ط�����HealthCheckService��IHostedService��

            //�����ļ�·���������  ������FileServer  ����   DirectoryBrowser  ����Ҫ��
            services.AddDirectoryBrowser();
        }

        // �����������м���Ƿ�����;�м������ASP.NET�ķ�װ�ˣ���������ʡ���˽���HttpЭ��Ĳ��֡��﷨��������  Use{Middleware} ����չ����           
        //  ASP.NET Core ģ�����õĹܵ�֧�֣�
        //  ��������Ա�쳣ҳ                            UseDeveloperExceptionPage
        //  ���쳣�������
        //  ��HTTP �ϸ��䰲ȫ��(HSTS)
        //  ��HTTPS �ض���
        //  ����̬�ļ�
        //  ��ASP.NET Core MVC �� Razor Pages
        //  ��·�� ������ UseEndpoints ����ʹ��           UseRouting
        //  ��UseEndpoints��ִ��·����ѡ���Endpoint��Ӧ��ί��
        //  ��UseAuthentication�������֤�м��  //�Ƿ���Ե�¼
        //  ��UseAuthorization���û���Ȩ�м�������ڶ������û�������Ȩ��//�Ƿ�ӵ��Ȩ��
        //  ��UseMvc��Mvc�м��
        //  ��UseHealthChecks����������м��
        //  ��UseMiddleware��������������м���ģ�ͨ���÷��������Է��������Զ����м��
        //  ��app.Use((context, next) =>{});  ֱ���õ��м��
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //��������Ա�쳣ҳ//���õ��м������������������û��catch ��չʾ������Ϣ
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

            //FileExtensionContentTypeProvider ����� Mappings ���ԣ������ļ���չ���� MIME �������͵�ӳ�䡣
            //��  StaticFileOptions  ��������  û���������
            var provider = new FileExtensionContentTypeProvider();
            // Add new mappings
            provider.Mappings[".myapp"] = "application/x-msdownload";
            provider.Mappings[".png"] = "MP2960/";
            provider.Mappings.Remove(".jpg");

            //Path.Combine �� �÷�
            //var path = Path.Combine(env.ContentRootPath, "../", "MyStaticFiles");
            //var path = Path.Combine(env.ContentRootPath, "bin", "Debug", "MyStaticFiles");

            //����Ĭ���ļ���������·������wwwroot���棬�����ڵ��� UseStaticFiles ֮ǰ
            //�������ֻ����� wwwroot ʹ��
            var dOps = new DefaultFilesOptions();
            dOps.DefaultFileNames.Clear();
            dOps.DefaultFileNames.Add("htmlpage.html");
            app.UseDefaultFiles(dOps);

            //�����м���ṩ��̬�ļ���  �������ֻ�ܷ���.html�ļ�  
            //��̬�ļ��洢����Ŀ�� Web ��Ŀ¼�С� Ĭ��Ŀ¼Ϊ {content root}/wwwroot������ͨ�� ���� StaticFileOptions ����Ŀ¼
            // using Microsoft.Extensions.FileProviders; PhysicalFileProvider
            app.UseStaticFiles(
                new StaticFileOptions
                {
                    //Path.Combine֧����ôʹ�ã�Path.Combine(env.ContentRootPath, "../outImgs")
                    FileProvider = new PhysicalFileProvider(staticFilePath),//�ļ��о���·��
                    RequestPath = "/StaticFiles",//�������еķ����ַ���ʵ��ʹ�þͻᱻ�滻�������·��
                                                 //"~/StaticFiles/images/red-rose.jpg"  ��   "~/MyStaticFiles/Tmp/images/red-rose.jpg" 
                                                 //ContentTypeProvider = provider,
                }
                );

            //�����Ǹ�һ����·
            //�����Ǹ��������汾���Ͼ�StaticFileֻ�ܷ���.html  ������Է�������Ŀ¼�Լ��ļ�
            //���Ǿ�����ļ���������˵ġ�  �������ó�һ���Ļ�������������е�htmlҳ�档
            //app.UseDirectoryBrowser(new DirectoryBrowserOptions
            //{
            //    FileProvider = new PhysicalFileProvider(path),
            //    RequestPath = "/StaticFiles"
            //});

            //���������ļ�����,����ֱ�Ӳ������������ĸ��������UseStaticFilesʹ���޲���ʹ��wwwroot�ļ��еĶ���
            //������ܴ򿪵��ļ�����������ˡ�����ͼƬɶ��
            //app.UseFileServer(new FileServerOptions
            //{
            //    FileProvider = new PhysicalFileProvider(path),
            //    RequestPath = "/StaticFiles",
            //    EnableDirectoryBrowsing = true,//�����ļ����������ֱ��������������
            //});

            //app.UseRouting();

            //app.UseAuthorization();

            //app.UseEndpoints(endpoints =>
            //{
            //    endpoints.MapRazorPages();
            //});

            //ʹ���Զ�����м��
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
