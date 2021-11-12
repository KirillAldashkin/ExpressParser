using System.Text;

namespace ExpressParser;

internal static class Utils
{
    public static unsafe string GetString(this ReadOnlySpan<char> data)
    {
        if (data.Length == 0) return "";
        fixed (char* start = &data[0])
            return new(start, 0, data.Length);
    }
    public static unsafe StringBuilder Append(this StringBuilder str, ReadOnlySpan<char> data)
    {
        if (data.Length == 0) return str;
        fixed (char* start = &data[0]) str.Append(start, data.Length);
        return str;
    }
    public static int GetPair(ReadOnlySpan<char> src, int at, char left, char right)
    {
        int level = 1;
        while (level > 0 && at < src.Length - 1)
        {
            at++;
            if (src[at] == left) level++;
            if (src[at] == right) level--;
        }
        return level == 0 ? at : -1;
    }
}
