using System;
using System.Threading.Tasks;

namespace ImageRenamer.Common
{
    /// <summary>
    /// Provides common properties and methods used to access the running device.
    /// </summary>
    public static class Device
    {
        #region Properties

        /// <summary>
        /// Gets or sets the platform implementator for this device.
        /// </summary>
        public static IPlatform Platform { get; set; }

        /// <summary>
        /// Gets a value that determines if the executing code is running on
        /// the main thread.
        /// </summary>
        public static bool IsMainThread => Platform.IsMainThread;

        #endregion

        #region Methods

        /// <summary>
        /// Starts an action on the main thread. Does not wait for action
        /// to be executed.
        /// </summary>
        /// <param name="action">The action to be executed.</param>
        public static void BeginInvokeOnMainThread( Action action )
        {
            if ( IsMainThread )
            {
                action();
            }
            else
            {
                Platform.BeginInvokeOnMainThread( action );
            }
        }

        /// <summary>
        /// Starts an action on the main thread and returns a <see cref="Task"/>
        /// that can be monitored to know when the action has completed.
        /// </summary>
        /// <param name="action">The action to be executed.</param>
        /// <returns>A <see cref="Task"/> that can be awaited.</returns>
        public static Task InvokeOnMainThreadAsync( Action action )
        {
            if ( IsMainThread )
            {
                action();

                return Task.CompletedTask;
            }

            var tcs = new TaskCompletionSource<bool>();

            BeginInvokeOnMainThread( () =>
            {
                try
                {
                    action();
                    tcs.TrySetResult( true );
                }
                catch ( Exception ex )
                {
                    tcs.TrySetException( ex );
                }
            } );

            return tcs.Task;
        }

        #endregion
    }
}
