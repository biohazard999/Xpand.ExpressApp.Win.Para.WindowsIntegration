using DevExpress.ExpressApp.DC;
using Xpand.ExpressApp.Win.Para.WindowsIntegration.Model;

namespace Xpand.ExpressApp.Win.Para.WindowsIntegration
{
    [DomainLogic(typeof(IModelCustomProtocolOption))]
    public static class ModelCustomProtocolOption_Logic
    {
        public static string Get_ProtocolDescription(IModelCustomProtocolOption option)
        {
            if (option == null || string.IsNullOrEmpty(option.ProtocolName))
                return "Protocol Handler";

            return option.ProtocolName + " Protocol";
        }
    }
}