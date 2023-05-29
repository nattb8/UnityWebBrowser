// UnityWebBrowser (UWB)
// Copyright (c) 2021-2022 Voltstro-Studios
// 
// This project is under the MIT license. See the LICENSE.md file for more details.

using System;
using Xilium.CefGlue;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UnityWebBrowser.Engine.Cef.Browser;

public class UwbCefRenderProcessHandler : CefRenderProcessHandler
{
    internal static string UNITY_POST_MESSAGE_FUNC = "UnityPostMessage";
    internal static string REGISTER_FUNC = "registerFunction";

    private static CefV8Value? jsCallbackFunction = null;
    private static CefV8Context? jsCallbackFunctionContext = null;

    protected override void OnContextCreated(CefBrowser browser, CefFrame frame, CefV8Context context)
    {
        CefV8Value v8Object = context.GetGlobal();

        CefV8Value versionValue = CefV8Value.CreateString(ThisAssembly.Info.InformationalVersion);
        v8Object.SetValue("uwbEngineVersion", versionValue);
        
        CefV8Value engineName = CefV8Value.CreateString("CEF (Chromium Embedded Framework)");
        v8Object.SetValue("webEngineName", engineName);

        CefV8Value cefVersion = CefV8Value.CreateString(CefRuntime.ChromeVersion);
        v8Object.SetValue("webEngineVersion", cefVersion);

        UnityPostMessageHandler handler = new UnityPostMessageHandler();

        CefV8Value func = CefV8Value.CreateFunction(UNITY_POST_MESSAGE_FUNC, handler);
        v8Object.SetValue(UNITY_POST_MESSAGE_FUNC, func, CefV8PropertyAttribute.None);

        CefV8Value func2 = CefV8Value.CreateFunction(REGISTER_FUNC, handler);
        v8Object.SetValue(REGISTER_FUNC, func2, CefV8PropertyAttribute.None);
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

            if (name == REGISTER_FUNC) {
                logMessage($"Called {REGISTER_FUNC}");
                if (arguments.Length == 1 && arguments[0].IsFunction) {
                    jsCallbackFunction = arguments[0];
                    jsCallbackFunctionContext = CefV8Context.GetCurrentContext();
                    logMessage($"Registered JS function");
                }
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

    class JsFunctionParams {
        public string fxName { get; set; }
        public string requestId { get; set; }
    }

    protected override bool OnProcessMessageReceived(CefBrowser browser, CefFrame frame, CefProcessId sourceProcess, CefProcessMessage message)
    {
        try {
            frame.SendProcessMessage(CefProcessId.Browser, CefProcessMessage.Create($"UwbCefRenderProcessHandler OnProcessMessageReceived: {message.Name}"));

            JsFunctionParams? jsFunctionParams = JsonSerializer.Deserialize<JsFunctionParams>(message.Name);
            frame.SendProcessMessage(CefProcessId.Browser, CefProcessMessage.Create($"Check not null? {jsFunctionParams != null} {jsFunctionParams?.fxName != null} {jsFunctionParams?.requestId != null} {jsCallbackFunction != null} {jsCallbackFunctionContext != null}"));
            if (jsFunctionParams != null && jsFunctionParams?.fxName != null && jsFunctionParams?.requestId != null 
                && jsCallbackFunction != null && jsCallbackFunctionContext != null) {    
                
                    frame.SendProcessMessage(CefProcessId.Browser, CefProcessMessage.Create($"Call {jsFunctionParams?.fxName}..."));
                    CefV8Value response = jsCallbackFunction.ExecuteFunctionWithContext(
                            jsCallbackFunctionContext, null, new CefV8Value[]{CefV8Value.CreateString(message.Name)}
                        );
                    if (response != null) {
                        frame.SendProcessMessage(CefProcessId.Browser, CefProcessMessage.Create($"{jsFunctionParams?.fxName}(): success"));
                    } else if (response.HasException) {
                        CefV8Exception exception = response.GetException();
                        frame.SendProcessMessage(CefProcessId.Browser, CefProcessMessage.Create($"{jsFunctionParams?.fxName}(): failed with exception {exception.Message} ({exception.ScriptResourceName}:{exception.SourceLine})"));
                    } else {
                        frame.SendProcessMessage(CefProcessId.Browser, CefProcessMessage.Create($"{jsFunctionParams?.fxName}(): failed"));
                    }  
            }
        } catch (Exception e) {
            frame.SendProcessMessage(CefProcessId.Browser, CefProcessMessage.Create($"UwbCefRenderProcessHandler OnProcessMessageReceived error: {e}"));
        }

        return false;
    }
}