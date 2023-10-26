namespace MagicCore.Extensions;

/// <summary>
/// IEnumerable扩展类
/// </summary>
public static class IEnumerableExtensions
{
    /// <summary>
    ///  查询 null 或为空的 IEnumerable 扩展方法
    /// </summary>
    /// <param name="this"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? @this)
    {
        return @this == null || !@this.Any();
    }
}