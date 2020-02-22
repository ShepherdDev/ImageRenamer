using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

using ImageRenamer.Common;

namespace ImageRenamer.Wpf
{
    /// <summary>
    /// Interaction logic for ProgressDialog.xaml
    /// </summary>
    public partial class ProgressDialog : Window
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
        /// Initializes a new instance of the <see cref="ProgressDialog"/> class.
        /// </summary>
        public ProgressDialog()
        {
            InitializeComponent();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Starts the operation.
        /// </summary>
        private void StartOperation()
        {
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
                        MessageBox.Show( ex.Message, "Unexpected error" );

                        return;
                    } );
                }

                if ( !CancellationTokenSource.IsCancellationRequested )
                {
                    Device.BeginInvokeOnMainThread( () => DialogResult = true );
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
                ProgressBar.IsIndeterminate = false;
                ProgressBar.Maximum = total;
                ProgressBar.Value = completedCount;
            } );
        }

        /// <summary>
        /// Cancels the operation and closes the dialog.
        /// </summary>
        private void CancelOperation()
        {
            CancellationTokenSource.Cancel();

            //
            // Wait a few moments for the task to complete.
            //
            if ( !RenameTask.IsCompleted )
            {
                for ( int i = 0; i < 20 && !RenameTask.IsCompleted; i++ )
                {
                    Thread.Sleep( 100 );
                }
            }

            DialogResult = false;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Window.ContentRendered" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnContentRendered( EventArgs e )
        {
            base.OnContentRendered( e );

            StartOperation();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Window.Closing" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.ComponentModel.CancelEventArgs" /> that contains the event data.</param>
        protected override void OnClosing( CancelEventArgs e )
        {
            base.OnClosing( e );

            if ( !DialogResult.HasValue )
            {
                CancelOperation();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnCancel_Click( object sender, RoutedEventArgs e )
        {
            CancelOperation();
        }

        #endregion
    }
}
