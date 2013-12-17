using System.Drawing;

namespace Xpand.ExpressApp.Win.Para.WindowsIntegration.Helpers
{
    /// <summary>The collection item for the <see cref="IconFileReader.Images"/> property.</summary>
    /// <remarks>I made this an interface so that it can easily be extended at the UI level. The
    /// application adds a Modified property (to a struct that implements this interface) to keep track
    /// of user edits.</remarks>
    public interface IIconData
    {
        int BitDepth { get; set; }

        Icon Icon { get; set; }
    }
}
