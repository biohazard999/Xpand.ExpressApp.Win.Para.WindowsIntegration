using DevExpress.ExpressApp.Model;

namespace Xpand.ExpressApp.Win.Para.WindowsIntegration.Model
{
    public interface IModelTaskbarOptions : IModelNode
    {
        IModelTaskbarJumplistOption TaskbarJumplistOptions { get; }
    }
}