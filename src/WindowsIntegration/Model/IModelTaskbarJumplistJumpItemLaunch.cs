namespace Xpand.ExpressApp.Win.Para.WindowsIntegration.Model
{
    public interface IModelTaskbarJumplistJumpItemLaunch : IModelTaskbarJumplistJumpItemBase
    {
        string PathToLaunch { get; set; }
        string Arguments { get; set; }

        string WorkingDirectory { get; set; }
    }
}