using System;

namespace Xpand.ExpressApp.Win.Para.WindowsIntegration
{
    /// <summary>
    ///     Holds a list of arguments given to an application at startup.
    /// </summary>
    public class ArgumentsReceivedEventArgs : EventArgs
    {
        public String[] Args { get; set; }
    }
}