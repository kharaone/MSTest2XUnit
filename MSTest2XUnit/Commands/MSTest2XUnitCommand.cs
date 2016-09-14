//------------------------------------------------------------------------------
// <copyright file="MSTest2XUnitCommand.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using NuGet.VisualStudio;
using VSLangProj;
using XUnitConverter;

namespace MSTest2XUnit
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class MSTest2XUnitCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("8fdde5ef-0eba-404b-b40a-962e31969775");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private static Package Package;

        /// <summary>
        /// Initializes a new instance of the <see cref="MSTest2XUnitCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private MSTest2XUnitCommand(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException(nameof(package));
            }

            Package = package;

            OleMenuCommandService commandService =
                ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
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
        public static MSTest2XUnitCommand Instance { get; private set; }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private static IServiceProvider ServiceProvider
        {
            get { return MSTest2XUnitCommand.Package; }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            var componentModel = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));
            Workspace = componentModel.GetService<VisualStudioWorkspace>();

            Instance = new MSTest2XUnitCommand(package);

            Dte = (DTE2)ServiceProvider.GetService(typeof(DTE));
            NugetPackage = (IVsPackageInstaller) componentModel.GetService<IVsPackageInstaller>();
        }

        public static IVsPackageInstaller NugetPackage { get; set; }

        public static DTE2 Dte { get; set; }

        public static VisualStudioWorkspace Workspace { get; set; }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            if (CanSolutionBeMigrated(Dte))
            {
                var cts = new CancellationTokenSource();

                foreach (var project in Dte.Solution.GetAllProjects())
                {
                    var hasChangesBeenApplied = RunAsync(project.FullName, cts.Token).Result;
                    if (!hasChangesBeenApplied)
                    {
                        RunAsync(project.FullName, cts.Token).Wait(cts.Token);
                    }
                    var msTestReferenceName = "Microsoft.VisualStudio.QualityTools.UnitTestFramework";
                    if (project.CollectReferenceNames()
                            .Any(
                                projectName =>
                                        projectName.Contains(msTestReferenceName)))
                    {
                        NugetPackage.InstallPackage("All", project, "XUnit", "2.1.0", false);
                        NugetPackage.InstallPackage("All", project, "XUnit.runner.console", "2.1.0", false);
                        project.RemoveReferenceByName(msTestReferenceName);
                    }
                }
            }
        }

        private async Task<bool> RunAsync(string projectPath, CancellationToken cancellationToken)
        {
            var converters = new ConverterBase[]
                {
                    new MSTestToXUnitConverter(),
                    new TestAssertTrueOrFalseConverter(),
                    new AssertArgumentOrderConverter(),
                    new IgnoreToSkipConverter(),
                    new TestCategoryToTraitConverter(),
                    new ExpectedExceptionConverter()
                };


            var project = Workspace.CurrentSolution.Projects.First(p => p.FilePath == projectPath);

            foreach (var converter in converters)
            {
                var solution = await converter.ProcessAsync(project, cancellationToken);
                project = solution.GetProject(project.Id);
            }

            return Workspace.TryApplyChanges(project.Solution);
        }

        private bool CanSolutionBeMigrated(DTE2 dte)
        {
            return dte.Solution.IsOpen;
        }
    }
}