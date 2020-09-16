using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Formatting.Compact;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace LoggingDemo
{
    public class Program
    {
        public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
           .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
           .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
           .AddEnvironmentVariables()
           .Build();

        // ��־����
        // ��־��������
        // ��־�����ȡ
        // ��־��¼�ķ���
        public static void Main(string[] args)
        {
            // LogDemo(args);
            ShowSerilog(args);
        }

        private static void LogDemo(string[] args)
        {
            MySimpleLog(args);
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        public static void MySimpleLog(string[] args)
        {
            IConfigurationBuilder configBuilder = new ConfigurationBuilder();
            configBuilder.AddCommandLine(args);
            configBuilder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            var config = configBuilder.Build();

            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IConfiguration>(p => config); //�ù���ģʽ�����ö���ע�ᵽ��������          
            serviceCollection.AddLogging(builder =>
            {
                builder.AddConfiguration(Configuration.GetSection("Logging"));
                builder.AddConsole();
            });


            serviceCollection.AddTransient<OrderService>();

            IServiceProvider service = serviceCollection.BuildServiceProvider();
            ShowLogUseInjection(service);
            ShowLogUseILogger(service);
            ShowLogWithScope(service);

        }

        private static void ShowLogUseInjection(IServiceProvider service)
        {
            var order = service.GetService<OrderService>();// use logger in service �Ƽ�ʹ��ǿ���͵ķ�ʽע��logger
            order.Show();
        }

        private static void ShowLogUseILogger(IServiceProvider service)
        {
            ILoggerFactory loggerFactory = service.GetService<ILoggerFactory>();
            ILogger alogger = loggerFactory.CreateLogger("alogger");
            alogger.LogInformation(2001, "aiya");// ����Event IDΪ2001��ӡ��Event ID ����־��Ϣ
            alogger.LogInformation("hello");
            var ex = new Exception("������");
            alogger.LogError(ex, "������");
            var alogger2 = loggerFactory.CreateLogger("alogger2");
            alogger2.LogDebug("aiya");
        }

        //һ����������������
        //����������־����
        //������׷������������̶�Ӧ
        private static void ShowLogWithScope(IServiceProvider service)
        {
            var logger = service.GetService<ILogger<Program>>();

            // �����ļ��м���  "IncludeScopes": true,
            while (Console.ReadKey().Key != ConsoleKey.Escape)
            {
                using (logger.BeginScope("ScopeId:{scopeid}", Guid.NewGuid()))
                {
                    logger.LogInformation("����Info");
                    logger.LogError("����Error");
                }
                Console.WriteLine("+++++++++++++++");
            }

            logger.LogInformation(new EventId(201, "xihuaa"), "world!");
        }

        /// <summary>
        /// ʵ����־�澯
        /// ʵ�������ĵĹ���
        /// ʵ����׷�ټ���
        /// </summary>
        private static int ShowSerilog(string[] args)
        {
            Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(Configuration)
          .MinimumLevel.Debug()
          .Enrich.FromLogContext()
          .WriteTo.Console(new RenderedCompactJsonFormatter())
          .WriteTo.File(formatter: new CompactJsonFormatter(), "logs\\myapp.txt", rollingInterval: RollingInterval.Day)
          .CreateLogger();
            try
            {
                Log.Information("Starting web host");
                CreateHostBuilder(args).UseSerilog(dispose: true).Build().Run();//UseSerilog(dispose: true) ʹ���м�� �滻ϵͳ��־���
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
