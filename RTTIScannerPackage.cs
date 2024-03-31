﻿global using Community.VisualStudio.Toolkit;
global using Microsoft.VisualStudio.Shell;
global using System;
global using Task = System.Threading.Tasks.Task;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using RTTIScanner.Implement;
using System.Runtime.InteropServices;
using System.Threading;

namespace RTTIScanner
{
    [Guid(PackageGuids.RTTIScannerString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ProvideAutoLoad(UIContextGuids80.NoSolution)] // sync autoload will be deprecated in the future
    [ProvideAutoLoad(UIContextGuids80.SolutionBuilding)] // but at least it works perfects now
    [ProvideAutoLoad(UIContextGuids80.Debugging)] // i have no idea at now.
    public sealed class RTTIScannerPackage : ToolkitPackage
    {
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            await this.RegisterCommandsAsync();

            DebuggerIfaces.Instance ??= new DebuggerIfaces();
            IVsDebugger debugger = (IVsDebugger)await GetServiceAsync(typeof(SVsShellDebugger));
            int hr = debugger.AdviseDebugEventCallback(new DebugEventCallback());
            ErrorHandler.ThrowOnFailure(hr);
        }

        public class DebugEventCallback : IDebugEventCallback2
        {
            public int Event(IDebugEngine2 engine, IDebugProcess2 process, IDebugProgram2 program, IDebugThread2 thread, IDebugEvent2 debugEvent, ref Guid riidEvent, uint attributes)
            {
                DebuggerIfaces.Instance.Update(engine, process, program, thread, debugEvent);
                return VSConstants.S_OK;
            }
        }
    }
}