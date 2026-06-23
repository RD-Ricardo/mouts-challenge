using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;

namespace Ambev.DeveloperEvaluation.ORM.Extensions;

public static class QueryableExtensions
{
    private static readonly HashSet<string> ReservedKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "_page", "_size", "_order"
    };

    public static IQueryable<T> ApplyOrder<T>(this IQueryable<T> source, string? order)
    {
        if (string.IsNullOrWhiteSpace(order)) return source;
        var parts = order.Trim('"', '\'').Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var clauses = new List<string>();
        foreach (var part in parts)
        {
            var pieces = part.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var prop = properties.FirstOrDefault(p => p.Name.Equals(pieces[0], StringComparison.OrdinalIgnoreCase));
            if (prop == null) continue;
            var dir = pieces.Length > 1 && pieces[1].Equals("desc", StringComparison.OrdinalIgnoreCase) ? "desc" : "asc";
            clauses.Add($"{prop.Name} {dir}");
        }
        return clauses.Count == 0 ? source : source.OrderBy(string.Join(",", clauses));
    }

    public static IQueryable<T> ApplyFilters<T>(this IQueryable<T> source, IDictionary<string, string> filters)
    {
        if (filters == null || filters.Count == 0) return source;
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var kv in filters)
        {
            if (string.IsNullOrEmpty(kv.Value)) continue;
            if (ReservedKeys.Contains(kv.Key)) continue;

            var key = kv.Key;
            string? rangeOp = null;
            if (key.StartsWith("_min", StringComparison.OrdinalIgnoreCase))
            {
                rangeOp = "min";
                key = key.Substring(4);
            }
            else if (key.StartsWith("_max", StringComparison.OrdinalIgnoreCase))
            {
                rangeOp = "max";
                key = key.Substring(4);
            }

            var prop = properties.FirstOrDefault(p => p.Name.Equals(key, StringComparison.OrdinalIgnoreCase));
            if (prop == null) continue;

            var parameter = Expression.Parameter(typeof(T), "x");
            var member = Expression.Property(parameter, prop);
            Expression? body = null;
            var propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

            if (rangeOp != null)
            {
                if (!TryConvert(kv.Value, propType, out var typed)) continue;
                var constant = Expression.Constant(typed, prop.PropertyType);
                body = rangeOp == "min"
                    ? Expression.GreaterThanOrEqual(member, constant)
                    : Expression.LessThanOrEqual(member, constant);
            }
            else if (propType == typeof(string))
            {
                var value = kv.Value;
                var startsWith = value.EndsWith("*");
                var endsWith = value.StartsWith("*");
                var trimmed = value.Trim('*');
                var constant = Expression.Constant(trimmed, typeof(string));
                MethodInfo method;
                if (startsWith && endsWith)
                    method = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) })!;
                else if (startsWith)
                    method = typeof(string).GetMethod(nameof(string.StartsWith), new[] { typeof(string) })!;
                else if (endsWith)
                    method = typeof(string).GetMethod(nameof(string.EndsWith), new[] { typeof(string) })!;
                else
                    method = typeof(string).GetMethod(nameof(string.Equals), new[] { typeof(string) })!;
                body = Expression.Call(member, method, constant);
            }
            else
            {
                if (!TryConvert(kv.Value, propType, out var typed)) continue;
                var constant = Expression.Constant(typed, prop.PropertyType);
                body = Expression.Equal(member, constant);
            }

            if (body == null) continue;
            var lambda = Expression.Lambda<Func<T, bool>>(body, parameter);
            source = source.Where(lambda);
        }

        return source;
    }

    private static bool TryConvert(string value, Type targetType, out object? result)
    {
        try
        {
            if (targetType == typeof(Guid)) { result = Guid.Parse(value); return true; }
            if (targetType.IsEnum) { result = Enum.Parse(targetType, value, true); return true; }
            if (targetType == typeof(DateTime)) { result = DateTime.Parse(value, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal); return true; }
            result = Convert.ChangeType(value, targetType, System.Globalization.CultureInfo.InvariantCulture);
            return true;
        }
        catch
        {
            result = null;
            return false;
        }
    }
}
