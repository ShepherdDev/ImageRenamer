using System;

using Foundation;

using ImageRenamer.Common;

namespace ImageRenamer.Mac
{
    /// <summary>
    /// Mac specific platform implementation of the platform code.
    /// </summary>
    public class MacPlatform : IPlatform
    {
        /// <summary>
        /// Determines if the current code is executing on the main thread.
        /// </summary>
        public bool IsMainThread => NSThread.Current.IsMainThread;

        /// <summary>
        /// Ensures that an action runs on the main thread. Does not wait
        /// for it to complete before returning.
        /// </summary>
        /// <param name="action">The action to be executed.</param>
        public void BeginInvokeOnMainThread( Action action )
        {
            NSRunLoop.Main.BeginInvokeOnMainThread( action.Invoke );
        }
    }
}
