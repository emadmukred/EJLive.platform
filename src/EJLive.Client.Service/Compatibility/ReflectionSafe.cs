using System.Reflection;

namespace EJLive.Client.Service.Compatibility;

internal static class ReflectionSafe
{
    public static IEnumerable<Type> SafeGetTypes(this Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t != null)!;
        }
        catch
        {
            return Array.Empty<Type>();
        }
    }

    public static Type? FindType(string fullName, string? assemblyQualifiedName = null)
    {
        if (!string.IsNullOrWhiteSpace(assemblyQualifiedName))
        {
            var direct = Type.GetType(assemblyQualifiedName, throwOnError: false);
            if (direct != null)
                return direct;
        }

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var match = assembly.SafeGetTypes().FirstOrDefault(t => t.FullName == fullName);
            if (match != null)
                return match;
        }

        return null;
    }

    public static object? InvokeIfExists(object target, string methodName, params object?[] args)
    {
        var types = args.Select(a => a?.GetType()).ToArray();
        var methods = target.GetType()
            .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(m => m.Name == methodName)
            .ToArray();

        foreach (var method in methods)
        {
            var parameters = method.GetParameters();
            if (parameters.Length != args.Length)
                continue;

            var compatible = true;
            for (var i = 0; i < parameters.Length; i++)
            {
                if (args[i] == null)
                    continue;

                if (!parameters[i].ParameterType.IsAssignableFrom(types[i]!))
                {
                    compatible = false;
                    break;
                }
            }

            if (!compatible)
                continue;

            return method.Invoke(target, args);
        }

        return null;
    }

    public static T? GetPropertyValue<T>(object target, params string[] names)
    {
        foreach (var name in names)
        {
            var property = target.GetType().GetProperty(
                name,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (property == null)
                continue;

            var value = property.GetValue(target);
            if (value == null)
                return default;

            if (value is T typed)
                return typed;

            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return default;
            }
        }

        return default;
    }
}
