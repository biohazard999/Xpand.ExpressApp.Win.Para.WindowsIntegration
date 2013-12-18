using DevExpress.ExpressApp.Model;

namespace Xpand.ExpressApp.Win.Para.WindowsIntegration.Model
{
    public interface IModelTaskbarJumplists : IModelNode
    {
        IModelTaskbarJumplistTaskCategory TasksCategory { get; }

        IModelTaskbarJumplistCustomCategories CustomCategories { get; }
    }
}