using WebApi.TorchUtilities.Services;

namespace WebApi.Database;

public static class ParamUtilities
{
    public record Param(string Name, Type Type, string? Remark, object Default);

    public static List<Param> GetParams(this Sequential s)
    {
        var p = new List<Param>();
        var names = s.ParamsName.Split(';');
        var types = s.ParamsType.Split(';');
        var remarks = s.ParamsRemark.Split(';');
        var defaultValues = s.ParamsDefault.Split(';');
        if (names.Length != types.Length || types.Length != remarks.Length ||
            remarks.Length != defaultValues.Length)
            throw new InvalidDataException();
        for (var i = 0; i < names.Length; ++i)
        {
            var type = types[i].ParseType();
            p.Add(new(names[i], type, remarks[i] is "null" ? null : remarks[i], defaultValues[i].ParseValue(type)));
        }

        return p;
    }
}
