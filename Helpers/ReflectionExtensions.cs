using System.Reflection;

namespace Metagen.Helpers;

internal static class ReflectionExtensions
{
    public static bool IsAssignableFrom(this ParameterInfo[] parameterInfos, object?[] arguments)
        => parameterInfos.Length == arguments.Length
            && parameterInfos
                .Select((parameterInfo, i)
                    => parameterInfo.ParameterType.IsAssignableFrom(arguments[i]?.GetType()))
                .All(x => x == true);
}