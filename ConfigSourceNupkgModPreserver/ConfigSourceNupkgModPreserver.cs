﻿using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using ConfigSourceNupkgModPreserver.Contracts.Merging;
using ConfigSourceNupkgModPreserver.Contracts.Orchestration;
using ConfigSourceNupkgModPreserver.Contracts.VisualStudioFacade;
using ConfigSourceNupkgModPreserver.Contracts.WindowsFacade;
using ConfigSourceNupkgModPreserver.Contracts.WppTargetsFileHandling;
using ConfigSourceNupkgModPreserver.Implementation.Merging;
using ConfigSourceNupkgModPreserver.Implementation.Orchestration;
using ConfigSourceNupkgModPreserver.Implementation.VisualStudioFacade;
using ConfigSourceNupkgModPreserver.Implementation.WindowsFacade;
using ConfigSourceNupkgModPreserver.Implementation.WppTargetsFileHandling;
using EnvDTE;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using NuGet.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace ConfigSourceNupkgModPreserver
{
    /// <inheritdoc />
    /// <summary>
    /// Implements the package exposed by this assembly.
    /// </summary>
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [Guid(ConfigSourceNupkgModPreserver.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists)]
    public sealed class ConfigSourceNupkgModPreserver : Package
    {
        private IVsPackageInstallerProjectEvents _packageInstallerProjectEvents;
        private IVsUIShell _vsUiShell;
        private DTE _dte;
        private IVsOutputWindow _vsOutputWindow;
        private IMerger _merger;
        private IFileSystemFacade _fileSystemFacade;
        private IWppTargetsFilesReader _wppTargetsFilesReader;
        private IWppTargetsXmlParser _wppTargetsFileParser;
        private IVisualStudioFacade _visualStudioFacade;
        private IWindowsShellFacade _windowsShellFacade;
        private NuGetFacade _nuGetFacade;
        private IPrompter _prompter;

        private Orchestrator _orchestrator;

        public const string PackageGuidString = "36fa07a8-d764-4bbc-93af-858e6584bea8";
        
        protected override void Initialize()
        {
            base.Initialize();

            var componentModel = (IComponentModel)GetService(typeof(SComponentModel));
            _packageInstallerProjectEvents = componentModel.GetService<IVsPackageInstallerProjectEvents>();
            _vsUiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
            _vsOutputWindow = GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;
            _dte = (DTE)GetService(typeof(DTE));

            _visualStudioFacade = new VisualStudioFacade(_vsUiShell, _vsOutputWindow);
            _windowsShellFacade = new WindowsShellFacade();
            _merger = new Merger(_windowsShellFacade);
            _nuGetFacade = new NuGetFacade(_packageInstallerProjectEvents);
            _fileSystemFacade = new FileSystemFacade();
            _wppTargetsFilesReader = new WppTargetsFilesReader(_fileSystemFacade);
            _wppTargetsFileParser = new WppTargetsXmlParser();
            _prompter = new Prompter(_visualStudioFacade);

            _orchestrator = new Orchestrator(_fileSystemFacade, _wppTargetsFilesReader, _wppTargetsFileParser, _merger, _visualStudioFacade, _prompter);

            _nuGetFacade.BindNuGetPackageEvents(RunMerge);
        }

        private void RunMerge() => _orchestrator.RunMerge(_dte.Solution.FullName);
    }
}