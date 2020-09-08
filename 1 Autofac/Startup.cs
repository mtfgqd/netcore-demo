using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Autofac.Extras.DynamicProxy;
using DependencyInjectionAutofacDemo.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace _1_Autofac
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // Ĭ�ϵ�����
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddControllersAsServices();
        }

        /// ������IOC��������ע��
        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterType<MyService>().As<IMyService>();//һ��ע��

            #region ����ע��
            builder.RegisterType<NamedService>().Named<IMyService>("NamedService");
            #endregion

            #region ����ע��
            builder.RegisterType<WiredProperty>();//ע������,PropertyAutoWiredService��ʹ�õ���WiredProperty����Ҫ�Ƚ���ע��
            builder.RegisterType<PropertyAutoWiredService>().As<IMyServiceV2>().PropertiesAutowired();
            #endregion

            #region AOP �ڲ��ı�ԭ���߼��Ļ����ϣ���ִ�е�����������߼�
            builder.RegisterType<MyInterceptor>(); //1 ע��������
            builder.RegisterType<AOPService>().As<IAOPService>()
                .PropertiesAutowired()//��Ҫ����ע���ʱ��
                .InterceptedBy(typeof(MyInterceptor)) // ʹ�õ�������
                .EnableInterfaceInterceptors();//���ýӿ������������ã�����������ǽӿ�   ������������
            #endregion

            #region ������ ������������Ϊmyscope�������������������ǻ�ȡ������������
            builder.RegisterType<ScopedService>().InstancePerMatchingLifetimeScope("myscope");
            #endregion

        }
        public ILifetimeScope AutofacContainer { get; private set; }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            this.AutofacContainer = app.ApplicationServices.GetAutofacRoot();

            var service = this.AutofacContainer.Resolve<IMyService>();//ʹ��һ�����
            service.ShowCode();

            #region ʹ����������
            var namedService = this.AutofacContainer.ResolveNamed<IMyService>("NamedService");
            namedService.ShowCode();
            #endregion

            #region ʹ������ע��ķ���
            var autoWiredService = this.AutofacContainer.Resolve<IMyServiceV2>();
            autoWiredService.ShowCode();
            #endregion

            #region AOP ʹ��������
            var aopService = this.AutofacContainer.Resolve<IAOPService>();//ʹ��һ�����
            aopService.ShowCode();
            #endregion

            #region ʹ��������
            //��������Ϊmyscope��������������Լ�������������������õ��Ķ���ͬһ�����������������������ǻ�ȡ������������,
            using (var myscope = this.AutofacContainer.BeginLifetimeScope("myscope"))
            {
                var service0 = myscope.Resolve<ScopedService>();
                using (var scope = myscope.BeginLifetimeScope())
                {
                    var service1 = scope.Resolve<ScopedService>();
                    var service2 = scope.Resolve<ScopedService>();
                    Console.WriteLine($"service1=service2:{service1 == service2}"); //true
                    Console.WriteLine($"service1=service0:{service1 == service0}"); //true
                }
            }
            #endregion


            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
