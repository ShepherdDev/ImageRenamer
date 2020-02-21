using System;

namespace ImageRenamer.Common
{
    /// <summary>
    /// Defines the required properties and methods for platform specific code.
    /// </summary>
    public interface IPlatform
    {
        /// <summary>
        /// Determines if the current code is executing on the main thread.
        /// </summary>
        bool IsMainThread { get; }

        /// <summary>
        /// Ensures that an action runs on the main thread. Does not wait
        /// for it to complete before returning.
        /// </summary>
        /// <param name="action">The action to be executed.</param>
        void BeginInvokeOnMainThread( Action action );
    }
}
