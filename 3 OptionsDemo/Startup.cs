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
using Microsoft.Extensions.Primitives;
using OptionsDemo.Services;

namespace OptionsDemo
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            #region ��ͨ��ʽע��
            services.AddSingleton<OrderServiceProperty>();
            services.AddSingleton<IOrderService, OrderService>();
            #endregion

            #region ѡ�ʽע�룬ʹ��Configure������ֻ�����������ֵ�Ƕ��٣������������������
            services.Configure<OrderServiceOptionProperty>(Configuration.GetSection(OrderServiceOptionProperty.SectionName));
            services.AddSingleton<IOrderServiceOption, OrderServiceOption>();

            //->IOptionsSnapshot ��Χ������ʹ��IOptionsSnapshot ֻ��ע��Ϊ������ģ�ע��Ϊ�����Ļᱨ��
            services.Configure<OrderServiceOptionPropertySnapshot>(Configuration.GetSection(OrderServiceOptionPropertySnapshot.SectionName));
            services.AddScoped<IOrderServiceOptionSnapshot, OrderServiceOptionSnapshot>();

            //->IOptionsMonitor ��������ʹ��IOptionsMonitor
            services.Configure<OrderServiceOptionPropertyMonitor>(Configuration.GetSection(OrderServiceOptionPropertyMonitor.SectionName));
            services.AddSingleton<IOrderServiceOptionMonitor, OrderServiceOptionMonitor>();
          

            ChangeToken.OnChange(() => Configuration.GetReloadToken(), () =>
            {
                Console.WriteLine("���¼�������");
            });
            #endregion

            //AddOrderService ��ΪIServiceCollection����չ��������IConfiguration��Ϊ��չ�����Ĳ���
            services.AddOrderService(Configuration);

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
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
