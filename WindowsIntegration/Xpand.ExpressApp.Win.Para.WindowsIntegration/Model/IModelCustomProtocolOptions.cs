using DevExpress.ExpressApp.Model;

namespace Xpand.ExpressApp.Win.Para.WindowsIntegration.Model
{
    public interface IModelCustomProtocolOptions : IModelNode
    {
        IModelCustomProtocolOption CustomProtocolOptions { get; }
    }
}