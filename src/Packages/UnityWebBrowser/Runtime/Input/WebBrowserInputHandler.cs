using UnityEngine;
using UnityWebBrowser.Shared;

namespace UnityWebBrowser.Input
{
    /// <summary>
    ///     Abstraction layer for getting input
    /// </summary>
    public abstract class WebBrowserInputHandler : ScriptableObject
    {
        //Mouse functions
        
        /// <summary>
        ///     Get the scroll
        /// </summary>
        /// <returns></returns>
        public abstract float GetScroll();
        
        /// <summary>
        ///     Get the current cursor position on the screen as a <see cref="Vector2"/>
        /// </summary>
        /// <returns></returns>
        public abstract Vector2 GetCursorPos();

        /// <summary>
        ///     Get all keys that are down this frame
        /// </summary>
        /// <returns>Returns an array of <see cref="WindowsKey"/> that are up</returns>
        public abstract WindowsKey[] GetDownKeys();
        
        /// <summary>
        ///     Get all keys that are up this frame
        /// </summary>
        /// <returns>Returns an array of <see cref="WindowsKey"/> that are down</returns>
        public abstract WindowsKey[] GetUpKeys();

        /// <summary>
        ///     Gets the input buffer for this frame
        /// </summary>
        /// <returns></returns>
        public abstract string GetFrameInputBuffer();
        
        //General
        
        /// <summary>
        ///     Called when inputs are started
        /// </summary>
        public abstract void OnStart();
        
        /// <summary>
        ///     Called when inputs are stopped
        /// </summary>
        public abstract void OnStop();
    }
}