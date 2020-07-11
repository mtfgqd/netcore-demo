using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore;
using System.IO;
using CoreDemo.ConfigDemo;
using CoreDemo.IOC;
using Autofac.Extensions.DependencyInjection;

namespace CoreDemo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //���� CreateHostBuilder �����Դ�������������������
            //��������������� Build �� Run ����
            var host = CreateHostBuilder(args).Build();

            using (var serviceScope = host.Services.CreateScope())
            {
                var services = serviceScope.ServiceProvider;

                try
                {
                    //ʹ�� IServiceScopeFactory.CreateScope ���� IServiceScope �Խ���Ӧ�÷�Χ�ڵ������÷�Χ�ķ���
                    //�˷�����������������ʱ������������ķ����Ա����г�ʼ������
                     var serviceContext = services.GetRequiredService<OperationService>();
                    // Use the context here
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred.");
                }
            }

            host.Run();
        }

        // ����Ӧ�ó����ִ��˳�� 
        // 1(ConfigureWebHostDefaults)->2(ConfigureHostConfiguration)->3(ConfigureAppConfiguration)-4(ConfigureServices/ConfigureLogging/Startup->Startup.ConfigureServices)->5(Startup.Configure)
        // ���� ConfigureServices �� Startup->Startup.ConfigureServices��ִ��˳��ȡ����webBuilder.UseStartup<Startup>()����дλ��
        // ���webBuilder.UseStartup<Startup>()����ConfigureServices��ôStartup->Startup.ConfigureServices��ִ��
        // ��������������������һ���Ķ���ע��Ӧ�õ����������Startup.ConfigureServices��ȥ���÷���Ϳ�����
        // ��Program.CreateHostBuilder()��Ҳ����ֱ��ConfigureServices() �� Configure() �������ʾ��
        public static IHostBuilder CreateHostBuilder(string[] args) =>
              Host.CreateDefaultBuilder(args)// ��������˳��ΪӦ���ṩĬ�����ã�
                                             // ChainedConfigurationProvider��������� IConfiguration ��ΪԴ�� ��Ĭ������ʾ���У�����������ã�����������ΪӦ�� ���õĵ�һ��Դ��
                                             //ʹ�� JSON �����ṩ����ͨ�� appsettings.json �ṩ��
                                             //ʹ�� JSON �����ṩ����ͨ�� appsettings.Environment.json �ṩ �� ���磬appsettings.Production.json �� appsettings.Development.json ��
                                             //Ӧ���� Development ����������ʱ��Ӧ�û��ܡ�
                                             //ʹ�û������������ṩ����ͨ�����������ṩ��
                                             //ʹ�������������ṩ����ͨ�������в����ṩ��
                                             //ʵ���������CoreDemo.Model.ConfigurationDemo.cs
             .UseServiceProviderFactory(new AutofacServiceProviderFactory())//ע����������������
             .ConfigureWebHostDefaults(webBuilder =>
             {
                 Console.WriteLine("1 ConfigureWebHostDefaults");
                 // Ӧ�ó����Ҫ���������/����
                 webBuilder.UseStartup<Startup>();
                 #region webBuilder.UseStartup<Startup>() ����Startup.ConfigureServices()��Startup.Configure()
                 //// �ȼ���Startup.ConfigureServices()
                 //webBuilder.ConfigureServices(services =>
                 //{
                 //    Console.WriteLine();
                 //    services.AddControllers();
                 //});
                 //// �ȼ���Startup.Configure()
                 //webBuilder.Configure(app =>
                 //{
                 //    app.UseHttpsRedirection();

                 //    app.UseRouting();

                 //    app.UseAuthorization();

                 //    app.UseEndpoints(endpoints =>
                 //    {
                 //        endpoints.MapControllers();
                 //    });
                 //});
                 #endregion
             })
            .ConfigureHostConfiguration(builder =>
            {
                Console.WriteLine("2 ConfigureHostConfiguration");
                // Ӧ�ó�������ʱ��Ҫ������������Ķ˿�/URL
            })
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                Console.WriteLine("3 ConfigureAppConfiguration");
                // Ƕ�������ļ���Ӧ�ó����ȡ

                config.Sources.Clear();

                var env = hostingContext.HostingEnvironment;

                //IniConfigurationProvider ������ʱ�� INI �ļ���ֵ�Լ�������
                //config.AddIniFile("MyIniConfig.ini", optional: true, reloadOnChange: true)
                //     .AddIniFile($"MyIniConfig.{env.EnvironmentName}.ini",optional: true, reloadOnChange: true);


                //XmlConfigurationProvider ������ʱ�� XML �ļ���ֵ�Լ�������
                //config.AddXmlFile("MyXMLFile.xml", optional: true, reloadOnChange: true)
                //     .AddXmlFile($"MyXMLFile.{env.EnvironmentName}.xml",optional: true, reloadOnChange: true);


                config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                      .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

                config.AddJsonFile("MyConfig.json", optional: true, reloadOnChange: true)
                      .AddJsonFile($"MyConfig.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);


                //AddEFConfiguration Ϊ�Զ���������ṩ����        using CoreDemo.ConfigDemo;
                //config.AddEFConfiguration(options => options.UseInMemoryDatabase("InMemoryDb"));

                config.AddEnvironmentVariables();

                //MyConfig.json �� MyConfig.Environment.json �ļ��е����� ��
                //����� appsettings.json �� appsettings.Environment.json �ļ��е����� ��
                //�ᱻ�������������ṩ����������������ṩ�����е������������

                //KeyPerFileConfigurationProvider ʹ��Ŀ¼���ļ���Ϊ���ü�ֵ�ԡ� �ü����ļ����� ��ֵ�����ļ������ݡ� Key-per-file �����ṩ�������� Docker �йܷ�����
                var path = Path.Combine(Directory.GetCurrentDirectory(), "path/to/files");
                config.AddKeyPerFile(directoryPath: path, optional: true);

                var Dict = new Dictionary<string, string>
                {
                     {"MyKey", "Dictionary MyKey Value"},
                     {"Position:Title", "Dictionary_Title"},
                     {"Position:Name", "Dictionary_Name" },
                     {"Logging:LogLevel:Default", "Warning"}
                };
                config.AddInMemoryCollection(Dict);

                if (args != null)
                {
                    config.AddCommandLine(args);
                }
            })
            .ConfigureServices(services =>
            {
                Console.WriteLine("4 ConfigureServices");
                //ע�� IStartupFilter
                services.AddTransient<IStartupFilter,RequestSetOptionsStartupFilter>();
            });

        public static IWebHost BuildWebHost(string[] args) =>
                        WebHost.CreateDefaultBuilder(args)
                        .UseStartup<Startup>()
                        .Build();
    }
}