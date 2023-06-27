﻿// UnityWebBrowser (UWB)
// Copyright (c) 2021-2022 Voltstro-Studios
// 
// This project is under the MIT license. See the LICENSE.md file for more details.

using System.Numerics;
using VoltRpc.Proxy;
using VoltstroStudios.UnityWebBrowser.Shared.Events;

namespace VoltstroStudios.UnityWebBrowser.Shared.Core;

/// <summary>
///     Shared interface for a UWB engine
///     <para>
///         Defines what RPC calls the client can call to invoke on the UWB engine
///     </para>
/// </summary>
[GenerateProxy(GeneratedName = "EngineControls", GeneratedNamespace = "VoltstroStudios.UnityWebBrowser.Shared.Core")]
internal interface IEngineControls
{

    /// <summary>
    ///     Shutdown the UWB engine
    /// </summary>
    public void Shutdown();

    /// <summary>
    ///     Send a keyboard event to the UWB engine
    /// </summary>
    /// <param name="keyboardEvent"></param>
    public void SendKeyboardEvent(KeyboardEvent keyboardEvent);

    /// <summary>
    ///     Send a mouse move event to the UWB engine
    /// </summary>
    /// <param name="mouseMoveEvent"></param>
    public void SendMouseMoveEvent(MouseMoveEvent mouseMoveEvent);

    /// <summary>
    ///     Send a mouse click event to the UWB engine
    /// </summary>
    /// <param name="mouseClickEvent"></param>
    public void SendMouseClickEvent(MouseClickEvent mouseClickEvent);

    /// <summary>
    ///     Send a mouse scroll event to the UWB engine
    /// </summary>
    /// <param name="mouseScrollEvent"></param>
    public void SendMouseScrollEvent(MouseScrollEvent mouseScrollEvent);

    /// <summary>
    ///     Tells the UWB engine to go forward (if it can)
    /// </summary>
    public void GoForward();

    /// <summary>
    ///     Tells the UWB engine to go back (if it can)
    /// </summary>
    public void GoBack();

    /// <summary>
    ///     Tells the UWB engine to refresh
    /// </summary>
    public void Refresh();

    /// <summary>
    ///     Tells the UWB engine to load a url
    /// </summary>
    /// <param name="url"></param>
    public void LoadUrl(string url);

    /// <summary>
    ///     Tells the UWB engine to load some HTML
    /// </summary>
    /// <param name="html"></param>
    public void LoadHtml(string html);

    /// <summary>
    ///     Tells the UWB engine to execute some JS
    /// </summary>
    /// <param name="js"></param>
    public void ExecuteJs(string js);
}