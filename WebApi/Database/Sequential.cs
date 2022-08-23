using System.Collections.Immutable;
using WebApi.TorchUtilities.Services;

namespace WebApi.Database;

public class Sequential
{
    public string Name { get; set; } = null!;
    public short Layers { get; set; }
    public DateTime CreateTime { get; set; }
    public short UsedCount { get; set; }
    public string Remark { get; set; } = null!;
    public string ContentJson { get; set; } = null!;
    public string ParamsName { get; set; } = null!;
    public string ParamsType { get; set; } = null!;
    public string ParamsRemark { get; set; } = null!;
    public string ParamsDefault { get; set; } = null!;

}

