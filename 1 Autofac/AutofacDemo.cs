﻿using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace _1_Autofac
{
    public class MyDIServiceProviderFactory : IServiceProviderFactory<MyContainerBuilder>
    {
        public MyContainerBuilder CreateBuilder(IServiceCollection services)
        {
            return new MyContainerBuilder() { Services = services };
        }
        public IServiceProvider CreateServiceProvider(MyContainerBuilder containerBuilder)
        {
            return new MyServiceProvider(containerBuilder.Services.BuildServiceProvider());
        }
    }


    public class MyServiceProvider : IServiceProvider
    {
        IServiceProvider _serviceProvider;
        public MyServiceProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public object GetService(Type serviceType)
        {
            Console.WriteLine($"正在创建对象:{serviceType.FullName}");
            return _serviceProvider.GetService(serviceType);
        }
    }


    public class MyContainerBuilder
    {
        internal IServiceCollection Services { get; set; }
    }

    public class MyInterceptor : IInterceptor
    {
        public void Intercept(IInvocation invocation)
        {
            Console.WriteLine($"Intercept before,Method:{invocation.Method.Name}");
            invocation.Proceed();
            Console.WriteLine($"Intercept after,Method:{invocation.Method.Name}");
        }
    }
}
