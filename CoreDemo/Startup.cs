using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Autofac.Extras.DynamicProxy;
using CoreDemo.IOC;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CoreDemo
{
    public class Startup
    {
        private readonly IHostEnvironment hostEnvironment;
        private readonly IWebHostEnvironment webHostEnvironment;

        /// <summary>
        /// IHostBuilderʱ��ֻ�ܽ����·�������ע�� Startup ���캯����
        /// IWebHostEnvironment 
        /// IHostEnvironment 
        /// IConfiguration
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="webHostEnvironment"></param>
        /// <param name="hostEnvironment"></param>
        public Startup(IConfiguration configuration, IWebHostEnvironment webHostEnvironment, IHostEnvironment hostEnvironment)
        {
            Console.WriteLine("4 Startup");
            Configuration = configuration;
            //var builder = new ConfigurationBuilder().SetBasePath(hostEnvironment.ContentRootPath).AddJsonFile("appsettings.json",optional: true, reloadOnChange: true);
            //Configuration = builder.Build();
            this.webHostEnvironment = webHostEnvironment;
            this.hostEnvironment = hostEnvironment;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // Defines the Services used by your app (for example, ASP.NET Core MVC, Entity Framework Core, Identity)
        public void ConfigureServices(IServiceCollection services)
        {
            // ע��Ӧ�õ����/���� ����־
            Console.WriteLine("4 Startup.ConfigureServices");

            services.AddDirectoryBrowser();//����Ŀ¼���

            //services.AddMvc();
            //services.AddAuthentication();
            //services.AddAuthorization();
            services.AddControllers();
            services.AddDbContext<DWQueueContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DemoDatabase")));
            //services.AddDbContext<DWQueueContext>(options => options.UseSqlServer(Configuration.GetConnectionString("Database")));

            //ʹ��ѡ��ģʽʱ����������ǰ� Position ���ֲ�������ӵ�������ע�����������
            services.Configure<ConfigDemo.PositionOptions>(Configuration.GetSection("Position"));

            //services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
            //.AddEntityFrameworkStores<DWQueueContext>();

            #region ���������� 
            //����������  https://docs.microsoft.com/zh-cn/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-3.1
            //��ʱ   ��ʱ�����ڷ��� (AddTransient) ��ÿ�δӷ���������������ʱ�����ġ� �����������ʺ��������� ��״̬�ķ���
            //��Χ��  �����������ڷ��� (AddScoped) ��ÿ���ͻ����������ӣ�һ�εķ�ʽ������
            //����   ��һʵ�������ڷ��� (AddSingleton) ���ڵ�һ������ʱ������������ Startup.ConfigureServices ����ʹ�÷���ע��ָ��ʵ��ʱ�������ġ� ÿ����������ʹ����ͬ��ʵ���� ���Ӧ����Ҫ��һʵ����Ϊ������������������������������ڡ� ��Ҫʵ�ֵ�һʵ�����ģʽ���ṩ�û�������������������е������ڡ�
            services.AddScoped<IMyDependency, MyDependency>();
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));//ע�᷺����


            //TryAdd{ LIFETIME}       ����������δע��ʵ��ʱ��ע��÷���
            // The following line has no effect:��Ϊ IMyDependency ����һ����ע���ʵ��
            services.TryAddSingleton<IMyDependency, DifMyDependency>();


            services.AddTransient<IOperationTransient, Operation>();
            services.AddScoped<IOperationScoped, Operation>();
            services.AddSingleton<IOperationSingleton, Operation>();
            services.AddSingleton<IOperationSingletonInstance>(new Operation(Guid.Empty));

            // OperationService depends on each of the other Operation types.
            services.AddTransient<OperationService, OperationService>();
            #endregion
        }

        private static void HandleMapTest1(IApplicationBuilder app)
        {
            app.Run(async context =>
            {
                await context.Response.WriteAsync("Map Test 1");
            });
        }

        private static void HandleMapTest2(IApplicationBuilder app)
        {
            app.Run(async context =>
            {
                await context.Response.WriteAsync("Map Test 2");
            });
        }
        private static void HandleMultiSeg(IApplicationBuilder app)
        {
            app.Run(async context =>
            {
                await context.Response.WriteAsync("Map multiple segments.");
            });
        }
        private static void HandleBranch(IApplicationBuilder app)
        {
            app.Run(async context =>
            {
                var branchVer = context.Request.Query["branch"];
                await context.Response.WriteAsync($"Branch used = {branchVer}");
            });
        }

        private void HandleBranchAndRejoin(IApplicationBuilder app)
        {
            app.Use(async (context, next) =>
            {
                var branchVer = context.Request.Query["branch"];
                //_logger.LogInformation("Branch used = {branchVer}", branchVer);

                // Do work that doesn't write to the Response.
                await next();
                // Do other work that doesn't write to the Response.
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        // Defines the middleware for the request pipeline
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            #region ���м���ܵ����з�֧.Map ��չ����Լ���������ܵ���֧��Map ���ڸ�������·����ƥ��������������ܵ���֧�� �������·���Ը���·����ͷ����ִ�з�֧��
            app.Map("/map1", HandleMapTest1);

            app.Map("/map2", HandleMapTest2);

            //Map ֧��Ƕ��
            app.Map("/level1", level1App => {
                level1App.Map("/level2a", level2AApp => {
                    // "/level1/level2a" processing
                });
                level1App.Map("/level2b", level2BApp => {
                    // "/level1/level2b" processing
                });
            });
            //Map ����ͬʱƥ������
            app.Map("/map1/seg1", HandleMultiSeg);
            //MapWhen ���ڸ���ν�ʵĽ����������ܵ���֧
            app.MapWhen(context => context.Request.Query.ContainsKey("branch"),
                               HandleBranch);

            #endregion
            //UseWhen Ҳ���ڸ���ν�ʵĽ����������ܵ���֧�� �� MapWhen ��ͬ���ǣ���������֧��������·������ն��м����������¼������ܵ�
            app.UseWhen(context => context.Request.Query.ContainsKey("branch"),
                             HandleBranchAndRejoin);

            #region autofac ����������
            this.AutofacContainer = app.ApplicationServices.GetAutofacRoot();

            var servicenamed = this.AutofacContainer.Resolve<IMyService>();
            servicenamed.ShowCode();


            var service = this.AutofacContainer.ResolveNamed<IMyService>("service2");
            service.ShowCode();

            #region ������

            using (var myscope = AutofacContainer.BeginLifetimeScope("myscope"))
            {
                var service0 = myscope.Resolve<MyNameService>();
                using (var scope = myscope.BeginLifetimeScope())
                {
                    var service1 = scope.Resolve<MyNameService>();
                    var service2 = scope.Resolve<MyNameService>();
                    Console.WriteLine($"service1=service2:{service1 == service2}");
                    Console.WriteLine($"service1=service0:{service1 == service0}");
                }
            }
            #endregion
            #endregion
            //�м��˳��
            //https://docs.microsoft.com/zh-cn/aspnet/core/fundamentals/middleware/?view=aspnetcore-3.1


            // ע���м�� ����HttpContext����
            Console.WriteLine("5 Startup.Configure");
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                //app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }


            app.UseHttpsRedirection();

            app.UseWebSockets();


            //Ҫ�ṩĬ���ļ��������� UseStaticFiles ǰ���� UseDefaultFiles�� 
            //UseDefaultFiles ʵ����������д URL�����ṩ�ļ��� 
            //ͨ�� UseStaticFiles ���þ�̬�ļ��м�����ṩ�ļ���
            app.UseDefaultFiles();//��������ļ���������default.htm��  default.html��index.htm��index.html
                                  //http://<server_address>/StaticFiles

            DefaultFilesOptions options = new DefaultFilesOptions();
            options.DefaultFileNames.Clear();
            options.DefaultFileNames.Add("mydefault.html");// ��mydefault.html����ΪĬ��ҳ
            app.UseDefaultFiles(options);

            ////wwwroot
            //    //css
            //    //images
            //    //js
            ////MyStaticFiles
            //    //images
            //        //banner1.svg
            app.UseStaticFiles(); // For the wwwroot folder
            //< img src = "~/images/banner1.svg" alt = "ASP.NET" class="img-responsive" />
            //�ȼ���   wwwroot/images/banner1.svg 


            //http://<server_address>/StaticFiles/images/banner1.svg ǰ������ĵ�ַ
            //Ӧ���ض���StaticFiles��MyStaticFiles
            //< img src = "~/StaticFiles/images/banner1.svg" alt = "ASP.NET" class="img-responsive" />
            //�ȼ���    MyStaticFiles/images/banner1.svg 
            var cachePeriod = env.IsDevelopment() ? "600" : "604800";

            var provider = new FileExtensionContentTypeProvider();
            // Add new mappings
            provider.Mappings[".myapp"] = "application/x-msdownload";
            provider.Mappings[".htm3"] = "text/html";
            provider.Mappings[".image"] = "image/png";
            // Replace an existing mapping
            provider.Mappings[".rtf"] = "application/x-msdownload";
            // Remove MP4 videos.
            provider.Mappings.Remove(".mp4");

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "MyStaticFiles")),
                RequestPath = "/StaticFiles",
                OnPrepareResponse = ctx =>
                    {
                        // Requires the following import:
                        // using Microsoft.AspNetCore.Http;
                        ctx.Context.Response.Headers.Append("Cache-Control", $"public, max-age={cachePeriod}");
                    },
                ContentTypeProvider = provider
                //��̬�ļ���Ȩ
                //var file = Path.Combine(Directory.GetCurrentDirectory(),
                //        "MyStaticFiles", "images", "banner1.svg");

                //return PhysicalFile(file, "image/svg+xml");
            });

            //����Ŀ¼��� ���ڰ�ȫ����Ĭ�Ϲر�
            //ͬʱ��Ҫ���� Startup.ConfigureServices �е�services.AddDirectoryBrowser(); ����������������
            app.UseDirectoryBrowser(new DirectoryBrowserOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images")),
                RequestPath = "/MyImages"
            });


            //UseFileServer ����� UseStaticFiles��UseDefaultFiles �� UseDirectoryBrowser����ѡ���Ĺ���
            app.UseFileServer();//���´����ṩ��̬�ļ���Ĭ���ļ��� δ����Ŀ¼�����

            app.UseFileServer(enableDirectoryBrowsing: true);//ͨ������Ŀ¼��������޲������ؽ��й�����

            app.UseFileServer(new FileServerOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "MyStaticFiles")),
                RequestPath = "/StaticFiles",
                EnableDirectoryBrowsing = true
            });

            app.UseRouting();
            // app.UseRequestLocalization();
            // app.UseCors();
            app.UseAuthentication();

            app.UseAuthorization();
            // app.UseSession();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapControllerRoute(
                                        name: "default",
                                        pattern: "{controller=Home}/{action=Index}/{id?}");
            });


            //Run ί�в����յ� next ������ ��һ�� Run ί��ʼ��Ϊ�նˣ�������ֹ�ܵ���
            //Run ��һ��Լ���� ĳЩ�м��������ܻṫ���ڹܵ�ĩβ���е� Run[Middleware] ����
            //�� Use ���������ί��������һ�� next ������ʾ�ܵ��е���һ��ί�С� ��ͨ���� ���� next ����ʹ�ܵ���·�� 
            app.Use(async (context, next) =>
            {
                // Do work that doesn't write to the Response.
                await next.Invoke();
                // Do logging or other work that doesn't write to the Response.
            });

            app.Run(async context =>
            {
                await context.Response.WriteAsync("Hello, World!");
            });

        }

        public ILifetimeScope AutofacContainer { get; private set; }

        /// <summary>
        /// autofac ��ȡ��ConfigureServices�����õ�����ע����
        /// </summary>
        /// <param name="builder"></param>
        public void ConfigureContainer(ContainerBuilder builder)
        {
            //builder.RegisterType<MyService>().As<IMyService>();
            #region ����ע��
            //builder.RegisterType<MyServiceV2>().Named<IMyService>("service2");
            #endregion

            #region ����ע��
            //builder.RegisterType<MyNameService>();
            //builder.RegisterType<MyServiceV2>().As<IMyService>().PropertiesAutowired();
            #endregion

            #region AOP
            builder.RegisterType<MyInterceptor>();
            builder.RegisterType<MyNameService>();
            builder.RegisterType<MyServiceV2>().As<IMyService>().PropertiesAutowired().InterceptedBy(typeof(MyInterceptor)).EnableInterfaceInterceptors();
            #endregion

            #region ������
            builder.RegisterType<MyNameService>().InstancePerLifetimeScope();
            #endregion

        }
    }
}
