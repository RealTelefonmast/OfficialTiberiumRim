using System.Collections.Generic;
using System.Text;

namespace TeleCore.Utility;

public static class StringUtils
{
    public static string ToStringListing<T>(this IEnumerable<T> collection)
    {
        StringBuilder sb = new();
        foreach (var item in collection)
        {
            sb.AppendLine($" - {item}");
        }
        return sb.ToString();
    }
}