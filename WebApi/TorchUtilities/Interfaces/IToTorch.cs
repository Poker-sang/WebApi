namespace WebApi.TorchUtilities.Interfaces;

internal interface IToTorch<TSelf> where TSelf : IToTorch<TSelf>
{
    public TSelf ToTorch();
}
