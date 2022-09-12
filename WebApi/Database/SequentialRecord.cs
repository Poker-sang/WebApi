namespace WebApi.Database;

public class SequentialRecord
{
    public string Name { get; set; } = "";
    public short Layers { get; set; }
    public DateTime CreateTime { get; set; }
    public short UsedCount { get; set; }
    public string Remark { get; set; } = null!;
    public string ContentJson { get; set; } = "[]";
    public string ParamsName { get; set; } = "";
    public string ParamsType { get; set; } = "";
    public string ParamsRemark { get; set; } = "";
    public string ParamsDefault { get; set; } = "";
}
