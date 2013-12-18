﻿using System.Windows.Forms;

namespace Xpand.ExpressApp.Win.Para.WindowsIntegration.Helpers
{
    public static class ControlExtentions
    {
        public delegate void InvokeHandler();

        public static void SafeInvoke(this Control control, InvokeHandler handler)
        {
            if (control.InvokeRequired) control.Invoke(handler);
            else handler();
        }
    }
}