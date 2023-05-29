﻿// UnityWebBrowser (UWB)
// Copyright (c) 2021-2022 Voltstro-Studios
// 
// This project is under the MIT license. See the LICENSE.md file for more details.

using System;
using System.Numerics;
using VoltstroStudios.UnityWebBrowser.Engine.Shared.Core;
using VoltstroStudios.UnityWebBrowser.Engine.Shared.Popups;
using VoltstroStudios.UnityWebBrowser.Shared;
using VoltstroStudios.UnityWebBrowser.Shared.Events;
using VoltstroStudios.UnityWebBrowser.Shared.Popups;
using Xilium.CefGlue;
using UnityWebBrowser.Engine.Cef.Core;
using System.Text.RegularExpressions;

namespace UnityWebBrowser.Engine.Cef.Browser;

/// <summary>
///     Offscreen CEF
/// </summary>
public class UwbCefClient : CefClient, IDisposable
{
    public readonly ClientControlsActions ClientControls;

    private readonly UwbCefContextMenuHandler contextMenuHandler;
    private readonly UwbCefDisplayHandler displayHandler;
    private readonly UwbCefLifespanHandler lifespanHandler;

    private readonly UwbCefLoadHandler loadHandler;
    private readonly UwbCefRenderHandler renderHandler;
    private readonly UwbCefRequestHandler requestHandler;

    private CefBrowser browser;
    private CefBrowserHost browserHost;

    /// <summary>
    ///     Creates a new <see cref="UwbCefClient" /> instance
    /// </summary>
    public UwbCefClient(CefSize size, PopupAction popupAction, EnginePopupManager popupManager, ProxySettings proxySettings, ClientControlsActions clientControlsActions)
    {
        ClientControls = clientControlsActions;

        //Setup our handlers
        loadHandler = new UwbCefLoadHandler(this);
        renderHandler = new UwbCefRenderHandler(size);
        lifespanHandler = new UwbCefLifespanHandler(popupAction, popupManager, proxySettings);
        lifespanHandler.AfterCreated += cefBrowser =>
        {
            browser = cefBrowser;
            browserHost = cefBrowser.GetHost();
        };
        displayHandler = new UwbCefDisplayHandler(this);
        requestHandler = new UwbCefRequestHandler(proxySettings);
        contextMenuHandler = new UwbCefContextMenuHandler();
    }

    /// <summary>
    ///     Destroys the <see cref="UwbCefClient" /> instance
    /// </summary>
    public void Dispose()
    {
        browserHost?.CloseBrowser(true);
        browserHost?.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Gets the pixel data of the CEF window
    /// </summary>
    /// <returns></returns>
    public byte[] GetPixels()
    {
        if (browserHost == null)
            return Array.Empty<byte>();

        return renderHandler.Pixels;
    }

    protected override CefLoadHandler GetLoadHandler()
    {
        return loadHandler;
    }

    protected override CefRenderHandler GetRenderHandler()
    {
        return renderHandler;
    }

    protected override CefLifeSpanHandler GetLifeSpanHandler()
    {
        return lifespanHandler;
    }

    protected override CefDisplayHandler GetDisplayHandler()
    {
        return displayHandler;
    }

    protected override CefRequestHandler GetRequestHandler()
    {
        return requestHandler;
    }

    protected override CefContextMenuHandler GetContextMenuHandler()
    {
        return contextMenuHandler;
    }

    #region Engine Events

    /// <summary>
    ///     Process a <see cref="KeyboardEvent" />
    /// </summary>
    /// <param name="keyboardEvent"></param>
    public void ProcessKeyboardEvent(KeyboardEvent keyboardEvent)
    {
        //Keys down
        foreach (WindowsKey i in keyboardEvent.KeysDown)
            KeyEvent(new CefKeyEvent
            {
                WindowsKeyCode = (int) i,
                EventType = CefKeyEventType.KeyDown
            });

        //Keys up
        foreach (WindowsKey i in keyboardEvent.KeysUp)
            KeyEvent(new CefKeyEvent
            {
                WindowsKeyCode = (int) i,
                EventType = CefKeyEventType.KeyUp
            });

        //Chars
        foreach (char c in keyboardEvent.Chars)
            KeyEvent(new CefKeyEvent
            {
#if WINDOWS
                WindowsKeyCode = c,
#else
                Character = c,
#endif
                EventType = CefKeyEventType.Char
            });
    }

    /// <summary>
    ///     Process a <see cref="VoltstroStudios.UnityWebBrowser.Shared.Events.MouseMoveEvent" />
    /// </summary>
    /// <param name="mouseEvent"></param>
    public void ProcessMouseMoveEvent(MouseMoveEvent mouseEvent)
    {
        MouseMoveEvent(new CefMouseEvent
        {
            X = mouseEvent.MouseX,
            Y = mouseEvent.MouseY
        });
    }

    /// <summary>
    ///     Process a <see cref="VoltstroStudios.UnityWebBrowser.Shared.Events.MouseClickEvent" />
    /// </summary>
    /// <param name="mouseClickEvent"></param>
    public void ProcessMouseClickEvent(MouseClickEvent mouseClickEvent)
    {
        MouseClickEvent(new CefMouseEvent
            {
                X = mouseClickEvent.MouseX,
                Y = mouseClickEvent.MouseY
            }, mouseClickEvent.MouseClickCount,
            (CefMouseButtonType) mouseClickEvent.MouseClickType,
            mouseClickEvent.MouseEventType == MouseEventType.Up);
    }

    /// <summary>
    ///     Process a <see cref="VoltstroStudios.UnityWebBrowser.Shared.Events.MouseScrollEvent" />
    /// </summary>
    /// <param name="mouseScrollEvent"></param>
    public void ProcessMouseScrollEvent(MouseScrollEvent mouseScrollEvent)
    {
        MouseScrollEvent(new CefMouseEvent
        {
            X = mouseScrollEvent.MouseX,
            Y = mouseScrollEvent.MouseY
        }, mouseScrollEvent.MouseScroll);
    }

    private void KeyEvent(CefKeyEvent keyEvent)
    {
        browserHost.SendKeyEvent(keyEvent);
    }

    private void MouseMoveEvent(CefMouseEvent mouseEvent)
    {
        browserHost.SendMouseMoveEvent(mouseEvent, false);
    }

    private void MouseClickEvent(CefMouseEvent mouseEvent, int clickCount, CefMouseButtonType button, bool mouseUp)
    {
        browserHost.SendMouseClickEvent(mouseEvent, button, mouseUp, clickCount);
    }

    private void MouseScrollEvent(CefMouseEvent mouseEvent, int scroll)
    {
        browserHost.SendMouseWheelEvent(mouseEvent, 0, scroll);
    }

    public void LoadUrl(string url)
    {
        browser.GetMainFrame()?.LoadUrl(url);
        //mainFrame.LoadUrl(url);
    }

    public Vector2 GetMouseScrollPosition()
    {
        return renderHandler.MouseScrollPosition;
    }

    public void LoadHtml(string html)
    {
        browser.GetMainFrame()?.LoadUrl($"data:text/html,{html}");
    }

    public void ExecuteJs(string js)
    {
        browser.GetMainFrame()?.ExecuteJavaScript(js, "", 0);
    }

    public void GoBack()
    {
        if (browser.CanGoBack)
            browser.GoBack();
    }

    public void GoForward()
    {
        if (browser.CanGoForward)
            browser.GoForward();
    }

    public void Refresh()
    {
        browser.Reload();
    }

    public void Resize(Resolution resolution)
    {
        renderHandler.Resize(new CefSize((int) resolution.Width, (int) resolution.Height));
        browserHost.WasResized();
    }

    #endregion

    public void CallJsFunction(string jsonData)
    {
        browser.GetMainFrame().SendProcessMessage(CefProcessId.Renderer, CefProcessMessage.Create(jsonData));
    }

    protected override bool OnProcessMessageReceived(CefBrowser browser, CefFrame frame, CefProcessId sourceProcess,
        CefProcessMessage message)
    {
        if (message.Name.StartsWith(UwbCefRenderProcessHandler.UNITY_POST_MESSAGE_FUNC)) {
            var regex = new Regex(Regex.Escape($"{UwbCefRenderProcessHandler.UNITY_POST_MESSAGE_FUNC}:"));
            var postMessage = regex.Replace(message.Name, "", 1);
            ClientControls.UnityPostMessage(postMessage);
        } else {
            CefLoggerWrapper.Debug($"[UwbCefClient] {message.Name}");
        }
        return false;
    }
}