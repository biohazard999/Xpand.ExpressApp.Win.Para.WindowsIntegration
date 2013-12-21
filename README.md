Xpand.ExpressApp.Win.Para.WindowsIntegration
============================================

Deeper Windows Integration Features to XAF/Xpand


## The WindowsIntegrationWindowsFormsModule ##

This Module allows you to integrate the [TaskbarAssistent](http://documentation.devexpress.com/#WindowsForms/clsDevExpressUtilsTaskbarTaskbarAssistanttopic "TaskbarAssistent Component") into XAF.


### Getting started ###

Integrate the `WindowsIntegrationWindowsFormsModule` like you would do with any Module.
This is a WindowsForms only Module.

Rebuild your project and you will see 2 additional nodes in the Options section:

![](http://i.imgur.com/vur7hpM.png)

The `TaskbarJumpListOptions` node allows you to specify `JumplistCategories` and `JumplistItems`. 

### Jumplists ###

Set the `EnableJumplist` option to `True` and specify a argument name that will be used to launch your application with command line arguments.

![](http://i.imgur.com/EsCmOa4.png)


> Note if you only like to launch external applications you can skip the `NavigationItemJumplistArgumentName`
> The `NavigationItemJumplistArgumentName` should end with a **colon**.

You see two nodes:


1. The `CustomCategories` node: This allows you to specify custom categories with `JumpItems` in it.
2. The `TasksCategory` is the default category provided by windows.


![](http://i.imgur.com/CjcAtQ1.png)

Currently there are 3 types of `JumpListItems`:

1. The `TaskbarJumplistJumpItemLaunch` allows you to specify any program that you'd like to launch. You can provide arguments and a `WorkingDirectory`.
2. The `TaskbarJumplistJumpItemNavigationItem` allows you to specify a `NavigationItem` the user can select from the the Jumplist.
3. The `TaskbarJumplistSeperatorItem` is a simple seperator that draws a horizontal line.

![](http://i.imgur.com/sJtdh5W.png)


#### TaskbarJumplistJumpItemLaunch ####
![](http://i.imgur.com/QSAoYJY.png)

You can currently specify:

- `PathToLaunch`: The program you like to launch
- `Arguments`: The arguments that are passed to the application
- `WorkingDirectory`: Specifies the folder in which the program is launched
- `ImageName`: An [ImageName][#Images] to provide an icon for the JumpListItem
- `Caption`: The Text that is displayed to the user
- `Index`: The order of the JumpListItem
- `Id`: The Id of the item

#### TaskbarJumplistJumpItemNavigationItem ####
![](http://i.imgur.com/q4XDmM7.png)

You can currently specify:

- `NavigationItem`: Specifies the NavigationItem that should be shown
- `UseProtocolIfAvailable`: Uses the protocol handler if available
- `ImageName`: An [ImageName][#Images] to provide an icon for the JumpListItem 
- `Caption`: The Text that is displayed to the user
- `Index`: The order of the JumpListItem
- `Id`: The Id of the item

#### TaskbarJumplistSeperatorItem ####
![](http://i.imgur.com/zOAhFkd.png)

You can currently specify:

- `Index`: The order of the JumpListItem
- `Id`: The Id of the item


#### Custom Categories ####
![](http://i.imgur.com/4pUwdBX.png)

![](http://i.imgur.com/bVMs33r.png)


You can currently specify:

- `Caption`: The caption of the Category
- `Index`: The order of the JumpListItem
- `Id`: The Id of the item

Adding new items is exact the same as for the `TasksCategory`.


### Bootstrapping code for NavigationItemJumplistItems ###

    static class Program
    {
        private static WinApplication _Application;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var assemblyName = typeof(Program).Assembly.GetName();
            var mutexName = assemblyName.Name + "_" + assemblyName.Version.ToString(3);

    #if DEBUG
            mutexName += "_Debug";
    #endif
            using (var instance = new SingleInstance(mutexName))
            {
                if (instance.IsFirstInstance)
                {
                    instance.ArgumentsReceived += WindowsIntegrationWindowsFormsModule.InstanceOnArgumentsReceived;

                    instance.ListenForArgumentsFromSuccessiveInstances();

Specify a mutex name. This is an ordinary string, my experiance has shown that a combination of the assemblyName combined with the version of the application and a debug constant works very well for the most scenarios.

Create an instance of the `SingleInstance` class that manages our application instances.
Check if this is the first instance launched, attach the `ArgumentsReceived` event handler to the `WindowsIntegrationWindowsFormsModule.InstanceOnArgumentsReceived` method and call the `ListenForArgumentsFromSuccessiveInstances` method to listen for new arguments on the `NamedPipe`.


Create your application as you always would:

    Application.EnableVisualStyles();
    Application.SetCompatibleTextRenderingDefault(false);

    _Application = new WinApplication
    {
        ApplicationName = assemblyName.Name,
        SplashScreen = new DevExpress.ExpressApp.Win.Utils.DXSplashScreen()
    };

    _Application.CreateCustomObjectSpaceProvider += (sender, args) =>
    {
        args.ObjectSpaceProvider = new XPObjectSpaceProvider(new ConnectionStringDataStoreProvider(args.ConnectionString));
    };

    _Application.DatabaseVersionMismatch += (sender, args) =>
    {
        args.Updater.Update();
        args.Handled = true;
    };

    _Application.Modules.Add(new SystemModule());
    _Application.Modules.Add(new SystemWindowsFormsModule());
    _Application.Modules.Add(new WindowsIntegrationWindowsFormsModule());
    _Application.Modules.Add(new DemoCenterModule());
    _Application.Modules.Add(new DemoCenterWindowsFormsModule());

**Before** you start the application make sure you pass the `WinApplication` instance to the `WindowsIntegrationWindowsFormsModule.TaskbarApplication` propety:

    WindowsIntegrationWindowsFormsModule.TaskbarApplication = _Application;

Than setup and launch your application

    InMemoryDataStoreProvider.Register();
    _Application.ConnectionString = InMemoryDataStoreProvider.ConnectionString;

    try
    {
        _Application.Setup();

        _Application.Start();
    }
    catch (Exception e)
    {
        _Application.HandleException(e);
    }

If the application is **not** the first instance pass the arguments to the first instance:

    }
    else
    {
        instance.PassArgumentsToFirstInstance();
    }

The whole bootstrapper now should look like this:

    using System;
    using System.Configuration;
    using System.Windows.Forms;
    using DevExpress.ExpressApp.SystemModule;
    using DevExpress.ExpressApp.Win;
    using DevExpress.ExpressApp.Win.SystemModule;
    using DevExpress.ExpressApp.Xpo;
    using Xpand.Demo.Para.DemoCenter.Module.Win;
    using Xpand.ExpressApp.Win.Para.WindowsIntegration;
    
    namespace Xpand.Demo.Para.DemoCenter.Win
    {
        static class Program
        {
            private static WinApplication _Application;
    
            /// <summary>
            /// The main entry point for the application.
            /// </summary>
            [STAThread]
            static void Main()
            {
                var assemblyName = typeof(Program).Assembly.GetName();
                var mutexName = assemblyName.Name + "_" + assemblyName.Version.ToString(3);
    
    #if DEBUG
                mutexName += "_Debug";
    #endif
                using (var instance = new SingleInstance(mutexName))
                {
                    if (instance.IsFirstInstance)
                    {
                        instance.ArgumentsReceived += WindowsIntegrationWindowsFormsModule.InstanceOnArgumentsReceived;
    
                        instance.ListenForArgumentsFromSuccessiveInstances();
    
                        Application.EnableVisualStyles();
                        Application.SetCompatibleTextRenderingDefault(false);
    
                        _Application = new WinApplication
                        {
                            ApplicationName = assemblyName.Name,
                            SplashScreen = new DevExpress.ExpressApp.Win.Utils.DXSplashScreen()
                        };
    
                        _Application.CreateCustomObjectSpaceProvider += (sender, args) =>
                        {
                            args.ObjectSpaceProvider = new XPObjectSpaceProvider(new ConnectionStringDataStoreProvider(args.ConnectionString));
                        };
    
                        _Application.DatabaseVersionMismatch += (sender, args) =>
                        {
                            args.Updater.Update();
                            args.Handled = true;
                        };
    
                        _Application.Modules.Add(new SystemModule());
                        _Application.Modules.Add(new SystemWindowsFormsModule());
                        _Application.Modules.Add(new WindowsIntegrationWindowsFormsModule());
                        _Application.Modules.Add(new DemoCenterModule());
                        _Application.Modules.Add(new DemoCenterWindowsFormsModule());
    
                        WindowsIntegrationWindowsFormsModule.TaskbarApplication = _Application;
    
                        InMemoryDataStoreProvider.Register();
                        _Application.ConnectionString = InMemoryDataStoreProvider.ConnectionString;
    
                        try
                        {
                            _Application.Setup();
    
                            _Application.Start();
                        }
                        catch (Exception e)
                        {
                            _Application.HandleException(e);
                        }
                    }
                    else
                    {
                        instance.PassArgumentsToFirstInstance();
                    }
                }
            }
        }
    }


That's it!

### Custom Protocols ###

The custom protocol options allow you to launch your application via an custom protocol. This can be handy if you like to send a link to another workstation to open the application with a specific window. You can think about this like a normal hyperlink but this works for your machine.

Select the CustomProtocolOptions node:

![](http://i.imgur.com/rgCkl9u.png)

Set the `EnableProtocols` to `True` and specify a `ProtolName`. You see a demo of the protocol under the `ProtocolHandler` node.

You can currently specify:

- `AutoRegisterProtols`: This will write the needed registry keys automatically, when the application launches
- `EnableProtocols`: This en/disables the whole logic for protocol handlers
- `ProtocolDescription`: This is a hint in the registry what this protocol does
- `ProtoclName`: The name of your protocol. This should not start with a number, dashes are allowed. See more under the [microsoft documentation](http://msdn.microsoft.com/en-us/library/aa767914(VS.85).aspx)


### Images ###

The Images will automatically be compiled into a new dll (based on `WinApplication.UserModelDifferenceFilePath`)
You can specify the name of the generated assembly via the `Options.AutomaticImageAssemblyName` parameter. The default leads to `JumplistImages.dll`.

This is necessary because windows needs a `NativeResource` assembly. This will be full automatically generated for all images used by your `JumplistItems`.

> If you launch your application, windows is sometimes caching the icons in the jumplists, so you may not see the actual image you set. After a reboot (or a windows logon/logoff) your application icon should be updated.
