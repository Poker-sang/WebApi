namespace WebApi.TorchUtilities.Misc;

public abstract class OptionalBase
{
    public abstract Type Type { get; }
    public static bool IsValidType(Type type) => !type.IsSubclassOf(typeof(OptionalBase));

    protected void RestrictGenerics()
    {
        if (!IsValidType(Type))
            throw new InvalidDataException($"Invalid Generic {Type.FullName}");
    }
}
