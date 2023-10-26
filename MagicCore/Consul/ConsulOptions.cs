using Consul;

namespace MagicCore.Consul;

/// <summary>
/// Consul配置
/// </summary>
public class ConsulOptions
{
    /// <summary>
    /// Consul服务地址
    /// </summary>
    public string ConsulAddress { get; set; }

    /// <summary>
    /// Consul连接Token
    /// </summary>
    public string ConsulToken { get; set; }

    /// <summary>
    /// 服务名称
    /// </summary>
    public string ServiceName { get; set; }

    /// <summary>
    /// 健康检查地址
    /// </summary>
    public string HealthCheck { get; set; }

    /// <summary>
    /// 服务地址
    /// </summary>
    public string ServiceAddress { get; set; }

    /// <summary>
    /// 是否启用Grpc服务
    /// </summary>
    public bool EnableGrpcService { get; set; }

    /// <summary>
    /// 是否启用Api服务
    /// </summary>
    public bool EnableApiService { get; set; } = true;

    /// <summary>
    /// Grpc服务权重，默认：1
    /// </summary>
    public int GrpcWeight { get; set; } = 1;

    /// <summary>
    /// 健康检查配置
    /// </summary>
    public AgentServiceCheck Check { get; set; }

    /// <summary>
    /// Consul ServiceEntry内存缓存刷新时间间隔，单位ms
    /// </summary>
    public int? ServiceEntryRefreshInterval { get; set; } = 20000;

    /// <summary>
    /// AddHttpClient注入的名称，此名称用于HttpClientFactory创建HttpClient使用
    /// </summary>
    public string HttpClientName { get; set; }
}