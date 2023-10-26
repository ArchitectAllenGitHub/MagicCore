namespace MagicCore.GeneratorId;

/// <summary>
/// 生成分布式Id
/// </summary>
public interface IGeneratorId
{
    /// <summary>
    /// 生成唯一标识ID
    /// </summary>
    /// <returns></returns>
    long NewId();

    /// <summary>
    /// 解析雪花ID
    /// </summary>
    /// <returns></returns>
    string ResolveId(long id);
}