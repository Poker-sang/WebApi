using WebApi.Database;

namespace WebApi;

public static class App
{
    public static CnnTestContext Database { get; set; } = null!;

    private static readonly DateTime _unixStart = new(1970, 1, 1, 8, 0, 0);
    public static long Now => (long)(DateTime.Now - _unixStart).TotalMilliseconds;
    public static long ToUnixTimeStamp(this DateTime d) => (long)(d - _unixStart).TotalMilliseconds;
}
