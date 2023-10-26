using Consul;
using MagicCore.Extensions;
using MagicCore.GeneratorId;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MagicCore.Consul;

/// <summary>
/// Consul扩展类
/// </summary>
public static class ConsulExtensions
{
    #region AddConsul

    /// <summary>
    /// 注入Consul，内部注入Consul配置
    /// </summary>
    /// <param name="this"></param>
    /// <param name="configuration">consul配置</param>
    /// <param name="useHealthCheck">是否使用AddHealthChecks</param>
    /// <returns></returns>
    /// <remarks>consul集群建议：server端3或者5个，client端根据程序副本数量配置即可，即每个副本服务器连接一个client</remarks>
    /// <example>
    ///     <code>
    ///         public void ConfigureServices(IServiceCollection services)
    ///         {
    ///             services.AddConsul(ConsulConfig);
    ///         }
    ///     </code>
    /// </example>
    public static IServiceCollection AddConsul(
        this IServiceCollection @this,
        IConfiguration configuration,
        bool useHealthCheck = true)
    {
        //注入健康检查
        if (useHealthCheck)
            @this.AddHealthChecks();

        @this.Configure<ConsulOptions>(configuration.GetSection("Consul"));
        @this.AddSingleton<IConsulClient>(p =>
            new ConsulClient(config =>
            {
                var options = p.GetRequiredService<IOptions<ConsulOptions>>().Value;
                config.Address = new Uri(options.ConsulAddress);
                config.Token = options.ConsulToken;
            }));

        @this.AddGeneratorId();
        return @this;
    }

    #endregion

    #region UseConsul

    /// <summary>
    /// 使用Consul，注册服务到Consul
    /// </summary>
    /// <param name="this"></param>
    /// <param name="lifetime"></param>
    /// <returns></returns>
    /// <remarks>
    ///     <code>
    ///         public void Configure(IApplicationBuilder app,IWebHostEnvironment env,IHostApplicationLifetime lifetime,IOptions&lt;ConsulServiceOptions&gt; options)
    ///         {
    ///             app.UseConsul(lifeTime,options.Value);
    ///         }
    ///     </code>
    /// </remarks>
    public static IApplicationBuilder UseConsul(
        this IApplicationBuilder @this,
        IHostApplicationLifetime lifetime)
    {
        var options = @this.ApplicationServices.GetRequiredService<IOptions<ConsulOptions>>().Value;
        @this.UseHealthChecks(options.HealthCheck);

        //Consul客户端
        var client = @this.ApplicationServices.GetRequiredService<IConsulClient>();
        var generatorId = @this.ApplicationServices.GetRequiredService<IGeneratorId>();

        //注册Consul服务
        var serviceId = client.RegisterConsulService(options, generatorId);

        //应用程序终止时，注销服务
        lifetime.ApplicationStopping.Register(() =>
        {
            if (!serviceId.IsNullOrEmpty())
                client.Agent.ServiceDeregister(serviceId).Wait();
        });

        return @this;
    }

    #endregion

    #region RegisterConsulService

    /// <summary>
    /// 注册Consul服务
    /// </summary>
    /// <param name="this"></param>
    /// <param name="options">服务配置</param>
    /// <param
    ///     name="generatorId">
    /// </param>
    /// <returns>Consul服务Id</returns>
    private static string RegisterConsulService(
        this IConsulClient @this,
        ConsulOptions options,
        IGeneratorId generatorId)
    {
        //服务Id
        var serviceId = string.Empty;

        //api服务
        var uri = new Uri(options.ServiceAddress);

        //服务查询结果
        var queryResult = @this.Health.Service(options.ServiceName).ConfigureAwait(false).GetAwaiter().GetResult();

        //判断服务是否存在
        if (queryResult != null && !queryResult.Response.IsNullOrEmpty())
            serviceId = queryResult.Response.FirstOrDefault(x =>
                    x.Service.Service == options.ServiceName &&
                    x.Service.Address == uri.Host &&
                    x.Service.Port == uri.Port)
                ?.Service.ID;

        //判断服务Id是否为空
        if (!serviceId.IsNullOrEmpty())
            return serviceId;

        //服务Id
        serviceId = generatorId.NewId().ToString();

        //服务Tags
        var tags = new List<string>();

        if (options.EnableApiService)
            tags.Add("Api");

        if (options.EnableGrpcService)
            tags.Add("gRPC");

        // 节点服务注册对象
        var registration = new AgentServiceRegistration
        {
            ID = serviceId,
            Name = options.ServiceName, // 服务名
            Address = uri.Host,
            Port = uri.Port, // 服务端口
            Tags = tags.ToArray(),
            Meta = new Dictionary<string, string>
            {
                ["Scheme"] = uri.Scheme
            },
            Check = options.Check ?? new AgentServiceCheck
            {
                // 注册超时
                Timeout = TimeSpan.FromMilliseconds(5000),
                // 健康检查时间间隔
                Interval = TimeSpan.FromMilliseconds(5000),
                // 服务停止多久后注销服务
                DeregisterCriticalServiceAfter = TimeSpan.FromMilliseconds(5000)
            }
        };

        //gRPC
        if (options.EnableGrpcService)
        {
            registration.Meta["GrpcWeight"] = (options.GrpcWeight <= 0 ? 1 : options.GrpcWeight).ToString();
            registration.Check.GRPC = options.HealthCheck;
            registration.Check.GRPCUseTLS = true;
            registration.Check.TLSSkipVerify = true;
        }
        //Tcp
        else if (options.HealthCheck.StartsWith("https", StringComparison.OrdinalIgnoreCase))
        {
            var health = new Uri(options.HealthCheck);
            registration.Check.TCP = $"{health.Host}:{health.Port}";
        }
        //Http
        else
        {
            registration.Check.HTTP = options.HealthCheck.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                ? options.HealthCheck
                : $"{uri.Scheme}://{uri.Host}:{uri.Port}{options.HealthCheck}";
        }

        // 注册服务
        @this.Agent.ServiceRegister(registration).ConfigureAwait(false).GetAwaiter().GetResult();

        return serviceId;
    }

    #endregion
}