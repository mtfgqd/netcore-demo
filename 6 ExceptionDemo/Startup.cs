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
using ExceptionDemo.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace ExceptionDemo
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
            services.AddMvc(mvcOptions =>
            {
                //4 ��MVC controller���м���ڵ��쳣���д��������Ƕ��м��������쳣����������ǰ����
                mvcOptions.Filters.Add<MyExceptionFilter>();//������controller�����õ�ȫ���쳣����

                //MyExceptionFilterAttributeҲʵ����IExceptionFilterҲ��ע��Ϊȫ�ֵģ�Ҳ����ֻ�����Ҫ�����controller�� ʵ�ָ�ϸ���ȵĿ���
                mvcOptions.Filters.Add<MyExceptionFilterAttribute>();
            }).AddJsonOptions(jsonoptions =>
            {
                jsonoptions.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                //1 ������Ա�쳣ҳ ��ʾ�����쳣����ϸ��Ϣ������Ҫ�������쳣���κ��м��ǰ��
                //��ҳ���������쳣�������������Ϣ��
                //��ջ����
                //��ѯ�ַ�������������У�
                //Cookie������У�
                //��ͷ
                app.UseDeveloperExceptionPage();
            }
            else
            {
                //2 �쳣�������ҳ
                //Ϊ�������������Զ��������ҳ����ʹ���쳣�����м����
                //�м��
                //���񲢼�¼�쳣��
                //�ڱ��ùܵ���Ϊָ����ҳ�����������ִ������ �����Ӧ���������򲻻�����ִ������
                app.UseExceptionHandler("/error");
                app.UseHsts();
            }

            app.UseExceptionHandler(errApp =>
            {
                errApp.Run(async context =>
                {
                    //3 �����쳣lambda
                    //ʹ�� IExceptionHandlerPathFeature ���ʴ���������������ҳ�е��쳣��ԭʼ����·����
                    var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                    IKnownException knownException = exceptionHandlerPathFeature.Error as IKnownException;
                    if (knownException == null)
                    {
                        var logger = context.RequestServices.GetService<ILogger<MyExceptionFilterAttribute>>();
                        logger.LogError(exceptionHandlerPathFeature.Error, exceptionHandlerPathFeature.Error.Message);
                        knownException = KnownException.Unknown;
                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    }
                    else
                    {
                        knownException = KnownException.FromKnownException(knownException);
                        context.Response.StatusCode = StatusCodes.Status200OK;
                    }
                    var jsonOptions = context.RequestServices.GetService<IOptions<JsonOptions>>();
                    context.Response.ContentType = "application/json; charset=utf-8";
                    await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(knownException, jsonOptions.Value.JsonSerializerOptions));
                });
            });

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
