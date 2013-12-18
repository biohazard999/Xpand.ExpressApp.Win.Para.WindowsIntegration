using DevExpress.ExpressApp.DC;
using Xpand.ExpressApp.Win.Para.WindowsIntegration.Model;

namespace Xpand.ExpressApp.Win.Para.WindowsIntegration
{
    [DomainLogic(typeof(IModelTaskbarJumplistJumpItemNavigationItem))]
    public static class ModelTaskbarJumplistJumpItemNavigationItem_Logic
    {
        public static string Get_Caption(IModelTaskbarJumplistJumpItemNavigationItem item)
        {
            if (item == null)
                return null;
            if (item.NavigationItem != null)
                return item.NavigationItem.Caption;
            return null;
        }

        public static string Get_ImageName(IModelTaskbarJumplistJumpItemNavigationItem item)
        {
            if (item == null)
                return null;
            if (item.NavigationItem != null)
                return item.NavigationItem.ImageName;
            return null;
        }
    }
}