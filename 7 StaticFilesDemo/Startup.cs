using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.FileProviders;
namespace StaticFilesDemo
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
            //services.AddDirectoryBrowser();//3 ������ʾ�̬�ļ�Ŀ¼  ��Configure��Ҳ��Ҫ����  app.UseDirectoryBrowser();
            // ����Ҫȥ�� app.UseDefaultFiles();��Ĭ��ҳ������
        }
        const int BufferSize = 64 * 1024;
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            //app.UseDirectoryBrowser();//�м������˳�� ȥ������Ĭ��ҳ����,�����ֱ�ӵ�Ĭ��ҳ��

            //2 ����Ĭ�Ϸ���ҳ: index/default
            //app.UseDefaultFiles();


            //1 ����������̬�ļ�Ŀ¼ʱ,������wwwroot�Ҳ�������file,���Ƕ�����StaticFileOptions ��path֮��,ֻ��file��ӳ��Ŀ¼

            app.UseStaticFiles();

            //1.1 ӳ���fileĿ¼
            app.UseStaticFiles(new StaticFileOptions
            {
               // RequestPath = "/files",// RequestPath ��url��ַ�� ȱʡ�ò�����Ĭ��ӳ�䵽վ�����ַ������ https://localhost:5001��������ӳ�䵽file�ļ���
                                       //������RequestPath֮�� ���� https://localhost:5001/files, ��ӳ�䵽file�ļ���
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "file")) // �ļ�ϵͳ�е��ļ��ṹ�������ļ�·�� file/page.html
            });

            //app.UseFileServer();

            app.MapWhen(context =>
            {
                return !context.Request.Path.Value.StartsWith("/api");// ����API�������ض��� index.html
            }, appBuilder =>
            {
                var option = new RewriteOptions();
                option.AddRewrite(".*", "/index.html", true);
                appBuilder.UseRewriter(option);

                appBuilder.UseStaticFiles(); // Ȼ����ʹ�þ�̬�ļ�

                //ֱ������ļ��ķ�ʽ�е�HttpRequest ��header�ǲ�һ����
                //appBuilder.Run(async c =>
                //{
                //    var file = env.WebRootFileProvider.GetFileInfo("index.html");

                //    c.Response.ContentType = "text/html";
                //    using (var fileStream = new FileStream(file.PhysicalPath, FileMode.Open, FileAccess.Read))
                //    {
                //        await StreamCopyOperation.CopyToAsync(fileStream, c.Response.Body, null, BufferSize, c.RequestAborted);
                //    }
                //});
            });



            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
