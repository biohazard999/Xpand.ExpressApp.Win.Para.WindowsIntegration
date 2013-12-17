using System.ComponentModel;
using System.Drawing.Design;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Model;

namespace Xpand.ExpressApp.Win.Para.WindowsIntegration.Model
{
    [ModelAbstractClass]
    public interface IModelTaskbarJumplistJumpItemBase : IModelTaskbarJumplistItem
    {
        string Caption { get; set; }

        [Editor("DevExpress.ExpressApp.Win.Core.ModelEditor.ImageGalleryModelEditorControl, DevExpress.ExpressApp.Win" + XafAssemblyInfo.VersionSuffix + XafAssemblyInfo.AssemblyNamePostfix, typeof(UITypeEditor))]
        string ImageName { get; set; }
    }
}