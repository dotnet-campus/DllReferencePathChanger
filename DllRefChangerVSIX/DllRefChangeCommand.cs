using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;

namespace DllRefChangerVSIX
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class DllRefChangeCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("a6f55334-ae07-484f-8377-71b3ed3cc0d8");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="DllRefChangeCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private DllRefChangeCommand(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static DllRefChangeCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        private EnvDTE.DTE _dte;
        internal EnvDTE.DTE DTE
        {
            get
            {
                if (_dte == null)
                    _dte = ServiceProvider.GetService(typeof(EnvDTE.DTE)) as EnvDTE.DTE;

                return _dte;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new DllRefChangeCommand(package);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            var solution = DTE.Solution;
            if (!solution.IsOpen)
            {
                string message = "Not exsit any solution here.";
                string title = "Empty Solution";

                VsShellUtilities.ShowMessageBox(
                    this.ServiceProvider,
                    message,
                    title,
                    OLEMSGICON.OLEMSGICON_INFO,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                return;
            }
            else
            {
                DllRefChangerSettingView.SettingView.ShowSettingWindow(solution.FullName, string.Empty);
            }



        }
    }
}
