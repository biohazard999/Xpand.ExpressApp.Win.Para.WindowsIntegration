using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;

namespace Xpand.ExpressApp.Win.Para.WindowsIntegration.Model
{
    public interface IModelTaskbarJumplistOption : IModelNode
    {
        bool EnableJumpList { get; set; }

        IModelTaskbarJumplists Jumplists { get; }

        [DefaultValue("JumplistImages.dll")]
        string AutomaticImageAssemblyName { get; set; }

        string NavigationItemJumplistArgumentName { get; set; }

        bool NavigationItemJumplistUseCustomProtocol { get; set; }
    }

    [DomainLogic(typeof(IModelTaskbarJumplistOption))]
    public static class ModelTaskbarJumplistOption_Logic
    {
        public static bool Get_NavigationItemJumplistUseCustomProtocol(this IModelTaskbarJumplistOption item)
        {
            if (item != null && item.Application != null && item.Application.Options is IModelCustomProtocolOptions)
            {
                return (item.Application.Options as IModelCustomProtocolOptions).CustomProtocolOptions.EnableProtocols;
            }
            return false;
        }
    }
}