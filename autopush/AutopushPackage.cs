using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using autopush;

namespace chun.autopush
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the information needed to show this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists)]
    [ProvideOptionPage(typeof(OptionPageGrid), "autopush", "normal", 0, 0, true)]
    [Guid(GuidList.guidautopushPkgString)]
    public sealed class autopushPackage : Package, IVsSolutionEvents
    {
        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        /// 
        public const string PackageGuidString = "5b4ebbbd-6591-417a-8cd4-209210c21b9f";
        private uint m_solutionCookie = 0;
        private SaveListener m_saveListener;
        private AutopushService service;
        public EnvDTE.DTE m_dte { get; set; }

        public autopushPackage()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }



        /////////////////////////////////////////////////////////////////////////////
        // Overridden Package Implementation
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Debug.WriteLine (string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();
            IVsSolution solution = (IVsSolution)GetService(typeof(SVsSolution));
            ErrorHandler.ThrowOnFailure(solution.AdviseSolutionEvents(this, out m_solutionCookie));
        }
        #endregion
        #region IVsSolutionEvents Members

        public int OnAfterCloseSolution(object pUnkReserved)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
        {
            InitializeWithDTEAndSolutionReady();
            return VSConstants.S_OK;
        }

        private void InitializeWithDTEAndSolutionReady()
        {
            m_dte = (EnvDTE.DTE)this.GetService(typeof(EnvDTE.DTE));

            if (m_dte == null)
                ErrorHandler.ThrowOnFailure(1);

            var solution_path = "";
            if (m_dte.Solution != null)
            {
                solution_path = System.IO.Path.GetDirectoryName(m_dte.Solution.FullName);
                this.service = new AutopushService((OptionPageGrid)GetDialogPage(typeof(OptionPageGrid)),
                                                    (IVsUIShell)GetService(typeof(SVsUIShell)), solution_path);
                m_saveListener = new SaveListener();
                m_saveListener.register(m_dte, service);
            }

        }




        public int OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeCloseSolution(object pUnkReserved)
        {
            if (m_saveListener != null)
            {
                this.service.push();
                m_saveListener.shutdown();
                m_saveListener = null;
            }

            return VSConstants.S_OK;
        }

        public int OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        #endregion
    }
}
