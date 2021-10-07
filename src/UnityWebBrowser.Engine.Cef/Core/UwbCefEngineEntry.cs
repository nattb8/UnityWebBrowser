using System;
using UnityWebBrowser.Engine.Shared;
using Xilium.CefGlue;

namespace UnityWebBrowser.Engine.Cef.Core
{
	/// <summary>
	///     <see cref="EngineEntryPoint" /> for the Cef engine
	/// </summary>
	public class UwbCefEngineEntry : EngineEntryPoint
    {
        private CefEngineManager cefEngineManager;

        protected override void EntryPoint(LaunchArguments launchArguments, string[] args)
        {
            cefEngineManager = new CefEngineManager(launchArguments, args);
            cefEngineManager.Init(ClientActions);

            SetupIpc(cefEngineManager, launchArguments);
            Ready(launchArguments);

            //Calling run message loop will cause the main thread to lock (what we want)
            CefRuntime.RunMessageLoop();

            //If the message loop quits
            Logger.Debug("Message loop quit.");
            Dispose();
            Environment.Exit(0);
        }

        #region Destroy

        protected override void ReleaseResources()
        {
            cefEngineManager?.Dispose();
            base.ReleaseResources();
        }

        #endregion
    }
}