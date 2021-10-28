using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MPS.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //WebHost.Start(async context =>
            //{
            //    await context.Response.WriteAsync("<h1>A Simple Host!</h1>");
            //}).WaitForShutdownAsync();
            //WebHost.CreateDefaultBuilder(args).UseStartup<StartupWebHost>().Build().RunAsync();
            //CreateHostBuilder(args).Build().Run();
            try
            {
                WebHost.CreateDefaultBuilder(args)
                        .UseStartup<WebHostStartup>()
                        .UseUrls("http://0.0.0.0:5000")
                        .Build()
                        .Run();
                var d = test("0");
                d.a = "123";
                var (_, e) = test("1");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            Console.ReadLine();
        }

        static (string a, string b) test(string c)
        {
            return (c, $"{c}1");
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            //Host�����Ժ��������WebHost�ᱻ��̭��
            Host.CreateDefaultBuilder(args)
            //��Startup��������ЧΪ���ķ���
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<WebHostStartup>();
            })
            //�ȼ���startup  �Ĺ��캯��
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                })
            //�ȼ� startup �� configure service �� configure
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureServices(services =>
                    {
                        services.AddControllersWithViews();
                    })
                    .Configure(app =>
                    {
                        var loggerFactory = app.ApplicationServices
                            .GetRequiredService<ILoggerFactory>();
                        var logger = loggerFactory.CreateLogger<Program>();
                        var env = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
                        var config = app.ApplicationServices.GetRequiredService<IConfiguration>();

                        logger.LogInformation("Logged in Configure");

                        if (env.IsDevelopment())
                        {
                            app.UseDeveloperExceptionPage();
                        }
                        else
                        {
                            app.UseExceptionHandler("/Home/Error");
                            app.UseHsts();
                        }

                        var configValue = config["MyConfigKey"];
                    });
                });
    }
}
