// UnityWebBrowser (UWB)
// Copyright (c) 2021-2022 Voltstro-Studios
// 
// This project is under the MIT license. See the LICENSE.md file for more details.

using Xilium.CefGlue;

namespace UnityWebBrowser.Engine.Cef.Browser;

public class UwbCefRenderProcessHandler : CefRenderProcessHandler
{
    internal static string UNITY_POST_MESSAGE_FUNC = "UnityPostMessage";

    protected override void OnContextCreated(CefBrowser browser, CefFrame frame, CefV8Context context)
    {
        CefV8Value v8Object = context.GetGlobal();

        CefV8Value versionValue = CefV8Value.CreateString(ThisAssembly.Info.InformationalVersion);
        v8Object.SetValue("uwbEngineVersion", versionValue);
        
        CefV8Value engineName = CefV8Value.CreateString("CEF (Chromium Embedded Framework)");
        v8Object.SetValue("webEngineName", engineName);

        CefV8Value cefVersion = CefV8Value.CreateString(CefRuntime.ChromeVersion);
        v8Object.SetValue("webEngineVersion", cefVersion);

        CefV8Value func = CefV8Value.CreateFunction(UNITY_POST_MESSAGE_FUNC, new UnityPostMessageHandler());
        v8Object.SetValue(UNITY_POST_MESSAGE_FUNC, func, CefV8PropertyAttribute.None);
    }

    class UnityPostMessageHandler : CefV8Handler {
        protected override bool Execute(string name, CefV8Value obj, CefV8Value[] arguments, out CefV8Value returnValue, out string exception)
        {
            if (name == UNITY_POST_MESSAGE_FUNC) {
                logMessage($"Called {UNITY_POST_MESSAGE_FUNC}");
                if (arguments.Length == 1 && arguments[0].IsString) {
                    var message = arguments[0].GetStringValue();
                    logMessage($"{UNITY_POST_MESSAGE_FUNC}:{message}");
                }

                returnValue = CefV8Value.CreateBool(true);
                exception = null;
                return true;
            }

            // Function does not exist.
            returnValue = null;
            exception = null;
            return false;
        }

        private void logMessage(string message) {
            var browser = CefV8Context.GetCurrentContext().GetBrowser();
            browser.GetMainFrame().SendProcessMessage(CefProcessId.Browser, CefProcessMessage.Create(message));
        }
    };
}