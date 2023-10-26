using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MagicCore.GeneratorId;

public static class GeneratorIdExtensions
{
    /// <summary>
    /// 添加生成Id服务
    /// </summary>
    /// <param name="this"></param>
    /// <returns></returns>
    public static IServiceCollection AddGeneratorId(this IServiceCollection @this)
    {
        @this.AddSingleton<IGeneratorId, GeneratorSnowflakeId>();

        return @this;
    }
}