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
using Microsoft.AspNetCore.Http;
namespace MiddlewareDemo
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
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// <summary>
        /// �м����ע��͵��ù��̣�����ע���ʹ�÷�Χ��㣬���緢������,ע��ע��˳��
        /// https://docs.microsoft.com/zh-cn/aspnet/core/fundamentals/middleware/?view=aspnetcore-3.1
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //�� Use ���������ί��������һ�� next ������ʾ�ܵ��е���һ��ί�С� ��ͨ�������� next ����ʹ�ܵ���·��
            app.Use(async (context, next) =>
            {
                //����ͻ��˷�����Ӧ��������� next.Invoke�� ��Ӧ��������� HttpResponse �ĸ��Ľ������쳣��
                //�����ͻ��˷�����Ӧ����Responseͷ��д��hello������next()�ᱨ������ͨ��context.Response.HasStarted���жϣ��ж��Ƿ�ʼ��ͻ��˷�����Ӧ
                // await context.Response.WriteAsync("Hello");�����ٵ���  await next()
                //await context.Response.WriteAsync("Hello");
                await next();
                if (context.Response.HasStarted)
                {
                    //һ���Ѿ���ʼ������������޸���Ӧͷ������
                }
                await context.Response.WriteAsync("Hello Use");
            });

            // Map �������·�ɽ��д��� �ȼ��� context.Request.Query.Keys.Equals("abc")
            app.Map("/abc", abcBuilder =>
            {
                // Use ������ app.Use
                abcBuilder.Use(async (context, next) =>
                {
                    await next();
                    await context.Response.WriteAsync("Hello Map");
                });
            });

            // MapWhen ������ĳ���ض�������������д���
            app.MapWhen(context =>
            {
                return context.Request.Path.Value.Contains("bbc");//·���а���
                //return context.Request.Query.Keys.Contains("abc");// query string �а���
            }, builder =>
            {
                //run ���м����������ĩ�ˣ������ٵ��ú����м��
                builder.Run(async context =>
                {
                    await context.Response.WriteAsync("Hello MapWhen" + context.Request.Path.Value);
                });

            });


            app.UseMyMiddleware();//�Զ����м��


            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            #region
            //app.Use(async (context, next) =>
            //{
            //    await next();
            //    await context.Response.WriteAsync("Hi ");
            //});


            //app.UseMyMiddleware();

            //app.Run(async context =>
            //{
            //    await context.Response.WriteAsync("Hello");
            //});

            //app.Map("/abc", builder =>
            //{


            //});

            //app.MapWhen(context => context.Request.IsHttps, builder =>
            //{

            //    builder.Run(async context2 => {

            //        await context2.Response.WriteAsync("is https");

            //    });
            //});
            #endregion
        }
    }
}
