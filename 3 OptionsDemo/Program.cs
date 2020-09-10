using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace OptionsDemo
{
    /// <summary>
    /// 
    /// ѡ��ģʽ֧��
    /// 1 ����ģʽ��ȡ����
    /// 2 ֧�ֿ���
    /// 3 ֧�ֱ��֪ͨ
    /// 4 ����ʱ��̬�޸�������
    /// ���ʱ
    /// 1 ʹ��XXXOptions
    /// 2 ʹ��IOptions<XXXOptions>  
    /// 3 ��Χ������ʹ��IOptionsSnapshot<XXXOptions>  
    /// 4 ��������ʹ��IOptionsMonitor<XXXOptions> ��Ϊ�������
    ///     IOptions<TOptions>��

    /*IOptions<TOptions>��
        ��֧�֣�
        ��Ӧ���������ȡ�������ݡ�
        ����ѡ��
        ע��Ϊ��һʵ���ҿ���ע�뵽�κη��������ڡ�
    IOptionsSnapshot<TOptions>��
        ��ÿ������ʱӦ���¼���ѡ��ķ��������á� �й���ϸ��Ϣ�������ʹ�� IOptionsSnapshot ��ȡ�Ѹ��µ����ݡ�
        ע��Ϊ��Χ�ڣ�����޷�ע�뵽��һʵ������
        ֧������ѡ��
    IOptionsMonitor<TOptions>��
            ���ڼ���ѡ����� TOptions ʵ����ѡ��֪ͨ��
            ע��Ϊ��һʵ���ҿ���ע�뵽�κη��������ڡ�
        ֧�֣�
            ����֪ͨ
            ����ѡ��
            ����������
            ѡ����ѡ��ʧЧ (IOptionsMonitorCache<TOptions>)
    */

    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
