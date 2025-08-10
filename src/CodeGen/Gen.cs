using System;
using System.Text;

namespace CodeGen;

public static class Gen
{
    public static string Join(int count, Func<int, string> part)
    {
        var sb = new StringBuilder();
        for (int i = 1; i <= count; i++) {
            sb.Append(part(i));
        }
        return sb.ToString();
    }
    
    public static string Join(int count, Func<int, string> part, string separator)
    {
        var sb = new StringBuilder();
        for (int i = 1; i <= count; i++) {
            sb.Append(part(i));
            if (i < count) {
                sb.Append(separator);
            }
        }
        return sb.ToString();
    }
}