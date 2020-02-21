using System;
using System.Threading;
using System.Threading.Tasks;

using AppKit;

using Foundation;

using ImageRenamer.Common;

namespace ImageRenamer.Mac
{
    /// <summary>
    /// Handles the display of the file rename progress.
    /// </summary>
    public partial class ProgressDialogController : NSViewController
    {
        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="FileWorker"/> that will be used for the rename process.
        /// </summary>
        public FileWorker Worker { get; set; }

        /// <summary>
        /// Gets or sets the cancellation token source.
        /// </summary>
        private CancellationTokenSource CancellationTokenSource { get; set; }

        /// <summary>
        /// Gets or sets the task that is running the rename operation.
        /// </summary>
        private Task RenameTask { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="ProgressDialogController"/> class.
        /// </summary>
        /// <param name="handle">The native handle.</param>
        public ProgressDialogController( IntPtr handle ) : base( handle )
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// The view has appeared and we can start processing.
        /// </summary>
        public override void ViewDidAppear()
        {
            base.ViewDidAppear();

            progressIndicatorView.StartAnimation( this );

            CancellationTokenSource = new CancellationTokenSource();

            RenameTask = Task.Run( async () =>
            {
                try
                {
                    await Worker.RenameFiles( UpdateProgress, CancellationTokenSource.Token );
                }
                catch ( Exception ex )
                {
                    System.Diagnostics.Debug.WriteLine( ex.Message );

                    await Device.InvokeOnMainThreadAsync( () =>
                    {
                        var alert = new NSAlert
                        {
                            MessageText = "Unexpected error",
                            InformativeText = ex.Message
                        };

                        alert.AddButton( "Ok" );
                        alert.RunModal();
                    } );
                }

                if ( !CancellationTokenSource.IsCancellationRequested )
                {
                    Device.BeginInvokeOnMainThread( () => DismissController( this ) );
                }
            } );
        }

        /// <summary>
        /// Update the progress bar to show how far along we are.
        /// </summary>
        /// <param name="completedCount">The number of items that have been completed.</param>
        /// <param name="total">The total number of items to be processed.</param>
        private void UpdateProgress( int completedCount, int total )
        {
            Device.BeginInvokeOnMainThread( () =>
            {
                progressIndicatorView.Indeterminate = false;
                progressIndicatorView.MaxValue = total;
                progressIndicatorView.DoubleValue = completedCount;
            } );
        }


        /// <summary>
        /// Cancels the operation and closes the dialog.
        /// </summary>
        /// <param name="sender">The object that initiated the action.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Style", "IDE0060:Remove unused parameter", Justification = "Required by Objective-C" )]
        partial void CancelOperation( NSObject sender )
        {
            CancellationTokenSource.Cancel();

            //
            // Wait a few moments for the task to complete.
            //
            if ( !RenameTask.IsCompleted )
            {
                for ( int i = 0; i < 20 && !RenameTask.IsCompleted; i++ )
                {
                    NSThread.SleepFor( 0.1 );
                }
            }

            DismissController( this );
        }

        #endregion
    }
}
