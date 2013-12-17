using System.Deployment.Application;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.Persistent.Base;

namespace Xpand.ExpressApp.Win.Para.WindowsIntegration.Model
{
    public interface IModelTaskbarJumplistJumpItemNavigationItem : IModelTaskbarJumplistJumpItemBase
    {
        [DataSourceProperty("Application.NavigationItems.AllItems")]
        IModelNavigationItem NavigationItem { get; set; }

        bool UseProtocolIfAvailable { get; set; }

        string Executable { get; }
        string Arguments { get; }

        bool UseProtocols { get; }
    }

    [DomainLogic(typeof(IModelTaskbarJumplistJumpItemNavigationItem))]
    public static class ModelTaskbarJumplistJumpItemNavigationItem_Logic
    {
        public static bool Get_UseProtocolIfAvailable(this IModelTaskbarJumplistJumpItemNavigationItem item)
        {
            if (item != null && item.Application != null && item.Application.Options is IModelTaskbarOptions)
            {
                return (item.Application.Options as IModelTaskbarOptions).TaskbarJumplistOptions.NavigationItemJumplistUseCustomProtocol;
            }
            return false;
        }

        public static string Get_Executable(IModelTaskbarJumplistJumpItemNavigationItem item)
        {
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                return WindowsIntegrationWindowsFormsModule.ExecutablePath;
            }

            if (item.UseProtocols)
            {
                var newArgs = GetPureArguments(item);

                var optionsProtocol = (item.Application.Options as IModelCustomProtocolOptions);

                return optionsProtocol.CustomProtocolOptions.ProtocolName + "://" + newArgs;
            }

            return WindowsIntegrationWindowsFormsModule.ExecutablePath;
        }

        private static string GetPureArguments(IModelTaskbarJumplistJumpItemNavigationItem item)
        {
            if (item == null || item.NavigationItem == null || item.NavigationItem.View == null)
                return null;

            var sc = new ViewShortcut(item.NavigationItem.View.Id, item.NavigationItem.ObjectKey);
            var newArgs = sc.ToString();

            newArgs = (item.Application.Options as IModelTaskbarOptions).TaskbarJumplistOptions.NavigationItemJumplistArgumentName + newArgs;
            return newArgs;
        }

        public static string Get_Arguments(IModelTaskbarJumplistJumpItemNavigationItem item)
        {
            var newArgs = GetPureArguments(item);

            if (ApplicationDeployment.IsNetworkDeployed)
                return newArgs;

            if (item.UseProtocols)
                return null; //Handled by Get_Executable

            return newArgs;
        }

        public static bool Get_UseProtocols(IModelTaskbarJumplistJumpItemNavigationItem item)
        {
            if (item.UseProtocolIfAvailable)
            {
                if (item.Application.Options is IModelCustomProtocolOptions)
                {
                    var optionsProtocol = (item.Application.Options as IModelCustomProtocolOptions);

                    if (optionsProtocol.CustomProtocolOptions.EnableProtocols)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}