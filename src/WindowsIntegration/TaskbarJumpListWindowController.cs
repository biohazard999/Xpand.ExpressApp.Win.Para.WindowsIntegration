using System;
using System.Linq;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Win;
using DevExpress.Utils.Taskbar;
using Xpand.ExpressApp.Win.Para.WindowsIntegration.Model;
using Xpand.ExpressApp.Win.Para.WindowsIntegration.ResourceManagers;

namespace Xpand.ExpressApp.Win.Para.WindowsIntegration
{
    public class TaskbarJumpListWindowController : WindowController
    {
        public TaskbarJumpListWindowController()
        {
            TargetWindowType = WindowType.Main;
            Activated += TaskbarJumpListWindowController_Activated;
            Deactivated += TaskbarJumpListWindowController_Deactivated;
        }

        private void TaskbarJumpListWindowController_Activated(object sender, EventArgs e)
        {
            if (JumplistOption == null)
                return;

            InitJumpList();
        }

        private IModelTaskbarJumplistOption JumplistOption
        {
            get
            {
                if (Application == null || Application.Model == null || Application.Model.Options == null)
                    return null;

                var optionsTaskbar = Application.Model.Options as IModelTaskbarOptions;
                if (optionsTaskbar == null || optionsTaskbar.TaskbarJumplistOptions == null)
                    return null;

                if (!optionsTaskbar.TaskbarJumplistOptions.EnableJumpList)
                    return null;

                return (Application.Model.Options as IModelTaskbarOptions).TaskbarJumplistOptions;
            }
        }

        private void InitJumpList()
        {
            Window.TemplateChanged -= Window_TemplateChanged;
            Window.TemplateChanged += Window_TemplateChanged;
        }

        private void Window_TemplateChanged(object sender, EventArgs e)
        {
            var options = JumplistOption;

            if (options == null)
                return;


            var imageNames =
                options.Jumplists.TasksCategory.OfType<IModelTaskbarJumplistJumpItemBase>()
                    .Where(m => !String.IsNullOrEmpty(m.ImageName))
                    .Select(imageName => imageName.ImageName)
                    .ToList();

            foreach (var category in options.Jumplists.CustomCategories)
            {
                imageNames.AddRange(
                    category.OfType<IModelTaskbarJumplistJumpItemBase>()
                        .Where(m => !String.IsNullOrEmpty(m.ImageName))
                        .Select(imageName => imageName.ImageName));
            }

            var manager = new RuntimeImageResourceManager((Application as WinApplication).UserModelDifferenceFilePath);
            manager.AutomaticImageAssemblyName = options.AutomaticImageAssemblyName;

            var imageAssembly = manager.WriteImageResouces(imageNames);


            TaskbarAssistant = new TaskbarAssistant();

            TaskbarAssistant.IconsAssembly = imageAssembly;

            foreach (var item in options.Jumplists.TasksCategory.OrderBy(m => m.Index))
            {
                InitJumpList(TaskbarAssistant.JumpListTasksCategory, item, manager);
            }

            foreach (var itemCategory in options.Jumplists.CustomCategories.OrderBy(m => m.Index))
            {
                var category = new JumpListCategory(itemCategory.Caption);

                foreach (var item in itemCategory)
                    InitJumpList(category.JumpItems, item, manager);

                TaskbarAssistant.JumpListCustomCategories.Add(category);
            }

            TaskbarAssistant.ParentControl = (Window as WinWindow).Form;
        }


        private void InitJumpList(JumpListCategoryItemCollection collection, IModelTaskbarJumplistItem item,
            RuntimeImageResourceManager manager)
        {
            if (item is IModelTaskbarJumplistJumpItemLaunch)
            {
                var launcher = item as IModelTaskbarJumplistJumpItemLaunch;

                collection.Add(new JumpListItemTask(launcher.Caption)
                {
                    Arguments = launcher.Arguments,
                    Path = launcher.PathToLaunch,
                    WorkingDirectory = launcher.WorkingDirectory,
                    IconIndex = manager.GetImageIndex(launcher.ImageName),
                });
            }

            if (item is IModelTaskbarJumplistJumpItemNavigationItem)
            {
                var launcher = item as IModelTaskbarJumplistJumpItemNavigationItem;

                if (launcher.NavigationItem == null || launcher.NavigationItem.View == null)
                    return;

                collection.Add(new JumpListItemTask(launcher.Caption)
                {
                    Arguments = launcher.Arguments,
                    Path = launcher.Executable,
                    IconIndex = manager.GetImageIndex(launcher.ImageName),
                });
            }

            if (item is IModelTaskbarJumplistSeperatorItem)
            {
                collection.Add(new JumpListItemSeparator());
            }
        }



        public TaskbarAssistant TaskbarAssistant { get; set; }

        private void TaskbarJumpListWindowController_Deactivated(object sender, EventArgs e)
        {
            if (TaskbarAssistant != null)
            {
                TaskbarAssistant.Dispose();
                TaskbarAssistant = null;
            }
            if (Window != null)
                Window.TemplateChanged -= Window_TemplateChanged;
        }
    }
}


