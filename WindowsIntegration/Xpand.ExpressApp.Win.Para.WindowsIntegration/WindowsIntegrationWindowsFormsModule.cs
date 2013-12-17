using System;
using System.Collections.Generic;
using System.Deployment.Application;
using System.IO;
using System.Linq;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Updating;
using DevExpress.ExpressApp.Win;
using DevExpress.ExpressApp.Win.SystemModule;
using Microsoft.Win32;
using Xpand.ExpressApp.Win.Para.WindowsIntegration.Helpers;
using Xpand.ExpressApp.Win.Para.WindowsIntegration.Model;

namespace Xpand.ExpressApp.Win.Para.WindowsIntegration
{
    public sealed class WindowsIntegrationWindowsFormsModule : ModuleBase
    {
        protected override ModuleTypeList GetRequiredModuleTypesCore()
        {
            return new ModuleTypeList(
                typeof(SystemModule),
                typeof(SystemWindowsFormsModule)
                );
        }

        public override IEnumerable<ModuleUpdater> GetModuleUpdaters(IObjectSpace objectSpace, Version versionFromDB)
        {
            return ModuleUpdater.EmptyModuleUpdaters;
        }

        protected override IEnumerable<Type> GetDeclaredControllerTypes()
        {
            return new[]
            {
                typeof(TaskbarJumpListWindowController),
                typeof(TaskbarJumpListHandleStartupItemController),
            };
        }

        protected override IEnumerable<Type> GetDeclaredExportedTypes()
        {
            return Type.EmptyTypes;
        }

        protected override void RegisterEditorDescriptors(List<EditorDescriptor> editorDescriptors)
        {

        }

        public override void ExtendModelInterfaces(ModelInterfaceExtenders extenders)
        {
            base.ExtendModelInterfaces(extenders);
            extenders.Add<IModelOptions, IModelTaskbarOptions>();
            extenders.Add<IModelOptions, IModelCustomProtocolOptions>();
        }

        public override void Setup(XafApplication application)
        {
            base.Setup(application);

            if (application != null)
                application.SetupComplete += application_SetupComplete;
        }

        void application_SetupComplete(object sender, EventArgs e)
        {
            if (sender is XafApplication)
                (sender as XafApplication).SetupComplete -= application_SetupComplete;

            AutoRegisterProtocols();
        }

        private void AutoRegisterProtocols()
        {
            if (ProtocolOption != null && ProtocolOption.AutoRegisterProtocols)
            {
                RegisterProtocols();
            }
        }

        public void RegisterProtocols()
        {
            var key = Registry.CurrentUser.OpenSubKey(@"Software\Classes", true);

            if (key != null)
            {
                var appPath = PathToRealExecutable;

                RegistryKey protocolKey;
                using (protocolKey = key.OpenSubKey(ProtocolOption.ProtocolName, true))
                {
                    if (protocolKey == null)
                        protocolKey = key.CreateSubKey(ProtocolOption.ProtocolName);

                    protocolKey.SetValue("URL Protocol", "");
                    protocolKey.SetValue("", "URL:" + ProtocolOption.ProtocolDescription);

                    RegistryKey defaultIcon;
                    using (defaultIcon = protocolKey.OpenSubKey("DefaultIcon", true))
                    {
                        if (defaultIcon == null)
                            defaultIcon = protocolKey.CreateSubKey("DefaultIcon");

                        defaultIcon.SetValue("", "\"" + appPath + "\"" + ",0");

                        defaultIcon.Close();
                    }

                    RegistryKey shell;
                    using (shell = protocolKey.OpenSubKey("shell", true))
                    {
                        if (shell == null)
                            shell = protocolKey.CreateSubKey("shell");

                        RegistryKey open;

                        using (open = shell.OpenSubKey("open", true))
                        {
                            if (open == null)
                                open = shell.CreateSubKey("open");

                            RegistryKey command;
                            using (command = open.OpenSubKey("command", true))
                            {
                                if (command == null)
                                    command = open.CreateSubKey("command");

                                command.SetValue("", "\"" + appPath + "\" " + "\"%l\"");

                                command.Close();
                            }
                            open.Close();
                        }
                        shell.Close();
                    }

                    protocolKey.Close();
                }
            }
        }

        private IModelCustomProtocolOption ProtocolOption
        {
            get
            {
                if (Application == null)
                    return null;
                if (Application.Model == null || Application.Model.Options == null)
                    return null;

                var optionsTaskbar = Application.Model.Options as IModelCustomProtocolOptions;
                if (optionsTaskbar == null || optionsTaskbar.CustomProtocolOptions == null)
                    return null;

                if (!optionsTaskbar.CustomProtocolOptions.EnableProtocols)
                    return null;

                return (Application.Model.Options as IModelCustomProtocolOptions).CustomProtocolOptions;
            }
        }

        public static string ExecutablePath
        {
            get
            {
                if (ApplicationDeployment.IsNetworkDeployed)
                {
                    return GetShortcutApprefMsName();
                }

                return PathToRealExecutable;
            }
        }

        private static string PathToRealExecutable
        {
            get
            {
                return System.Windows.Forms.Application.ExecutablePath;
            }
        }

        public static string GetShortcutApprefMsName()
        {
            using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall"))
            {
                var updateLocation = System.Deployment.Application.ApplicationDeployment.CurrentDeployment.UpdateLocation;

                string appKey = key.GetSubKeyNames().FirstOrDefault(x => GetValue(key, x, "UrlUpdateInfo") == updateLocation.ToString());

                var shortcutFolder = GetShortcutFolderName(key, appKey);
                var shortcutSuite = GetShortcutSuiteName(key, appKey);
                var shortcutFilename = GetShortcutFileName(key, appKey);

                var p = Path.Combine(shortcutFolder, shortcutSuite, shortcutFilename);

                p = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), p);

                return p;
            }
        }

        private static string GetShortcutFolderName(RegistryKey key, string appKey)
        {
            if (appKey == null)
            {
                return null;
            }
            return GetValue(key, appKey, "ShortcutFolderName");
        }

        private static string GetShortcutSuiteName(RegistryKey key, string appKey)
        {

            if (appKey == null)
            {
                return null;
            }
            return GetValue(key, appKey, "ShortcutSuiteName");
        }

        private static string GetShortcutFileName(RegistryKey key, string appKey)
        {

            if (appKey == null)
            {
                return null;
            }
            return GetValue(key, appKey, "ShortcutFileName") + ".appref-ms";
        }

        private static string GetValue(RegistryKey key, string app, string value)
        {
            if (string.IsNullOrEmpty(app))
                return null;

            using (var subKey = key.OpenSubKey(app))
            {
                if (!subKey.GetValueNames().Contains(value)) { return null; }
                return subKey.GetValue(value).ToString();
            }
        }

        public static WinApplication TaskbarApplication;

        public static void InstanceOnArgumentsReceived(object sender, ArgumentsReceivedEventArgs argumentsReceivedEventArgs)
        {
            if (TaskbarApplication == null || !(TaskbarApplication.MainWindow is WinWindow))
                return;

            var window = TaskbarApplication.MainWindow as WinWindow;

            var arguments = argumentsReceivedEventArgs.Args;

            if (arguments.Length > 0)
            {
                window.Form.SafeInvoke(() =>
                {
                    var sc = CreateViewShortcutFromArguments(arguments[0]);
                    ShowViewFromShortcut(sc);
                });
            }
        }

        public static void ShowViewFromShortcut(ViewShortcut sc)
        {
            View shortCutView = TaskbarApplication.ProcessShortcut(sc);

            TaskbarApplication.ShowViewStrategy.ShowView(new ShowViewParameters(shortCutView), new ShowViewSource(null, null));

            var winWindow = (TaskbarApplication.MainWindow as WinWindow);
            if(winWindow == null)
                return;

            if (winWindow.Form.WindowState == System.Windows.Forms.FormWindowState.Minimized)
                winWindow.Form.WindowState = System.Windows.Forms.FormWindowState.Normal;

            if (!winWindow.Form.TopMost)
            {
                winWindow.Form.TopMost = true;
                winWindow.Form.TopMost = false;
            }
            
            winWindow.Form.BringToFront();
            winWindow.Form.Focus();
        }

        public static ViewShortcut CreateViewShortcutFromArguments(string argument)
        {
            var a2 = CleanUriProtocols(argument);

            a2 = CleanNavigationItemJumplistArgumentName(TaskbarApplication, a2);

            var sc = ViewShortcut.FromString(a2);
            return sc;
        }

        private static string CleanUriProtocols(string argument)
        {
            if (argument.Length > 0)
            {
                if (TaskbarApplication != null && TaskbarApplication.Model.Options is IModelCustomProtocolOptions)
                {
                    var options = TaskbarApplication.Model.Options as IModelCustomProtocolOptions;

                    if (options.CustomProtocolOptions.EnableProtocols)
                    {
                        if (argument.StartsWith(options.CustomProtocolOptions.ProtocolHandler, StringComparison.InvariantCultureIgnoreCase))
                        {
                            argument = argument.Substring(options.CustomProtocolOptions.ProtocolHandler.Length, argument.Length - options.CustomProtocolOptions.ProtocolHandler.Length);

                            if (argument.Length > 0)
                            {
                                if (argument.EndsWith("/"))
                                {
                                    argument = argument.Substring(0, argument.Length - 1);
                                }
                            }

                            if (!argument.StartsWith("/", StringComparison.InvariantCultureIgnoreCase))
                            {
                                argument = "/" + argument;
                            }
                        }
                    }

                }

            }
            return argument;
        }

        private static string CleanNavigationItemJumplistArgumentName(XafApplication application, string argument)
        {
            if (application == null)
                return argument;

            if (application.Model == null || !(application.Model.Options is IModelTaskbarOptions))
                return argument;

            var optionsTaskbar = application.Model.Options as IModelTaskbarOptions;

            var navigationArgument = optionsTaskbar.TaskbarJumplistOptions.NavigationItemJumplistArgumentName;

            if (!string.IsNullOrEmpty(navigationArgument))
            {
                if (!navigationArgument.StartsWith("/", StringComparison.InvariantCultureIgnoreCase))
                    navigationArgument = "/" + navigationArgument;

                if (argument.Length > 0 && argument.StartsWith(navigationArgument))
                    argument = argument.Substring(navigationArgument.Length, argument.Length - navigationArgument.Length);
            }

            return argument;
        }
    }
}
