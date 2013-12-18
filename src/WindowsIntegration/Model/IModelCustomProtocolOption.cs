using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;

namespace Xpand.ExpressApp.Win.Para.WindowsIntegration.Model
{
    public interface IModelCustomProtocolOption : IModelNode
    {
        bool EnableProtocols { get; set; }

        [DefaultValue(true)]
        bool AutoRegisterProtocols { get; set; }
        string ProtocolName { get; set; }
        string ProtocolDescription { get; set; }

        string ProtocolHandler { get; }
    }

    [DomainLogic(typeof(IModelCustomProtocolOption))]
    public static class ModelCustomProtocolOption_Logic
    {
        public static string Get_ProtocolHandler(IModelCustomProtocolOption option)
        {
            if (option == null || string.IsNullOrEmpty(option.ProtocolName))
                return null;

            return option.ProtocolName + "://";
        }
    }
}