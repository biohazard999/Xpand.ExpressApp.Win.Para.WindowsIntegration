using DevExpress.ExpressApp.Model;

namespace Xpand.ExpressApp.Win.Para.WindowsIntegration.Model
{
    public interface IModelTaskbarJumplistCustomCategory : IModelNode, IModelList<IModelTaskbarJumplistItem>
    {
        string Caption { get; set; }
    }
}