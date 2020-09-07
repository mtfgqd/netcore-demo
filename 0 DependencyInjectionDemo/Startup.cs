using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using DependencyInjectionDemo.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DependencyInjectionDemo
{
    public class Startup
    {
        //1 �����ע�᷽ʽ ֱ��ע�� ������ʽ try��ʽ ���� �滻���Ƴ�
        //2 ������������� ���� ������ ˲ʱ
        //3 ����Ļ�ȡ��ʽ ��controller��

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            #region ע�����ͬ�������ڵķ���
            services.AddSingleton<IMySingletonService, MySingletonService>();//���������ǵ���

            //���������������������������������ǵ����ģ��������ͷŵ�������Ҳ�ᱻ�ͷŵ�
            services.AddScoped<IMyScopedService, MyScopedService>();

            services.AddTransient<IMyTransientService, MyTransientService>();// ˲ʱ
            #endregion

            #region ��ʽע�� 1���ֶ�����ʵ����ע�� 2������ע�� 3����������
            services.AddSingleton<ISingletonOrderService>(new SingletonOrderService());  //�ֶ��������󣬷�������,�����������ͷ�
            services.AddSingleton<ISingletonOrderService, SingletonOrderServiceEx>(); //���������𴴽�

            // ������ʽע��
            services.AddSingleton<ISingletonOrderService>(serviceProvider =>
            {
                // ���Դ������л�ȡ��Ԫ�أ�ȥƴװ����������ʵ��
                var dependencyClass = serviceProvider.GetService<IMyTransientService>();
                return new SingletonOrderServiceFactory();
            });

            services.AddScoped<IScopeOrderService>(serviceProvider =>
            {
                return new ScopeOrderService();
            });
            #endregion

            #region ����ע��
            //���񣨽ӿڣ�ע����Ͳ���ע����
            services.TryAddSingleton<ISingletonOrderService, SingletonOrderServiceEx>();

            // ֻҪ���񣨽ӿڣ���ʵ�ֲ�ͬ�ͻᱻע�ᣬ��ͬ��ʵ���಻�ᱻע��
            services.TryAddEnumerable(ServiceDescriptor.Singleton<ISingletonOrderService, SingletonOrderServiceTryAdd>());
            //services.TryAddEnumerable(ServiceDescriptor.Singleton<ISingletonOrderService, SingletonOrderServiceTryAdd>());
            //services.TryAddEnumerable(ServiceDescriptor.Singleton<ISingletonOrderService>(new SingletonOrderServiceTryAdd()));
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IOrderService>(p =>
            {
                return new OrderService();
            }));
            #endregion

            #region �Ƴ����滻ע��
            // OrderServiceEx ���滻��IOrderService��ʵ��
            services.Replace(ServiceDescriptor.Singleton<IOrderService, OrderServiceEx>());

            //ɾ������IScopeOrderService��ʵ��
            services.RemoveAll<IScopeOrderService>();
            #endregion

            #region ע�᷺��ģ��
            services.AddSingleton(typeof(IGenericService<>), typeof(GenericService<>));
            #endregion




            #region �����ͷ���Ϊ
            // ʵ��IDisposeable�ӿڵĶ���
            // 1 ����ֻ�ͷ����䴴���Ķ����������Լ������󱻷Ž������ģ������������ͷ�
            // 2 �������������������ͷ�ʱ���Ż��ͷ����䴴���Ķ���
            // 3 ��ȫ�������������ͷŶ���
            // 4 ��Ҫ�ڸ������д�����ע��Ϊ˲ʱ����Ķ���

            //TransitDispose �ӿڣ�ÿ������ᱻ�ֱ��ͷ�,���ǲ�Ҫ�ڸ������д�������
            services.AddTransient<ITransitDisposableOrderService, TransitDisposableOrderService>();

            //ScopeDispose �ӿڣ�ÿ���������ڶ�ֻ�ܻ�ȡһ����ͬ�Ķ��󣬽ӿ�ִ����Ϻ󼸸���������ͷż�������
            services.AddScoped<IScopeDisposableOrderService>(p => new ScopeDisposableOrderService());

            //SingletonDispose �ӿڣ���������������ֻ��һ�����󣬽ӿ�ִ����Ϻ����Ҳ���ᱻ�ͷ�
            services.AddSingleton<ISingletonDisposableOrderService, SingletonDisposableOrderService>();

            #region DisposeLifeTime�ӿ� �Լ�����ʵ����ע�ᣬ������������ǹ���������������
            var service = new DisposableOrderService();
            services.AddSingleton<IDisposableOrderService>(service);
            #endregion

            #endregion

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //�Ӹ������л�ȡ˲ʱ����������Щ�Ӹ������л�ȡ�ķ��񶼻�פ���ڸ������У�
            //���������еĶ���ֻ����Ӧ�ó���ر�ʱ���գ��������ֲ�����ܺ��ڴ�
            //var dd = app.ApplicationServices.GetService<ITransitDisposableOrderService>();


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
