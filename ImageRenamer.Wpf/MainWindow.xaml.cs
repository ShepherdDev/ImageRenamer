using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

using ImageRenamer.Common;

using Microsoft.Win32;

namespace ImageRenamer.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Properties

        /// <summary>
        /// Gets the view model.
        /// </summary>
        /// <value>
        /// The view model.
        /// </value>
        protected ViewModel ViewModel { get; private set; }

        /// <summary>
        /// Gets the worker.
        /// </summary>
        /// <value>
        /// The worker.
        /// </value>
        protected FileWorker Worker { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            Worker = new FileWorker()
            {
                FilenameTemplate = Properties.Settings.Default.FilenameTemplate
            };
            DataContext = ViewModel = new ViewModel( Worker );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Window.Closing" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.ComponentModel.CancelEventArgs" /> that contains the event data.</param>
        protected override void OnClosing( CancelEventArgs e )
        {
            base.OnClosing( e );

            Properties.Settings.Default.FilenameTemplate = Worker.FilenameTemplate;
            Properties.Settings.Default.Save();
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the Click event of the AddFiles control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void AddFiles_Click( object sender, RoutedEventArgs e )
        {
            var dialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "Images (PNG, JPG, TIF)|*.png;*.jpg;*.jpeg;*.tif;*.tiff"
            };

            if ( dialog.ShowDialog() == true )
            {
                Worker.AppendFileList( dialog.FileNames );
            }
        }

        /// <summary>
        /// Handles the Click event of the RemoveFile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void RemoveFile_Click( object sender, RoutedEventArgs e )
        {
            if ( dgFileList.SelectedIndex != -1 )
            {
                int index = dgFileList.SelectedIndex;

                Worker.RemoveFileAt( index );

                if ( Worker.FileList.Count > 0 && index >= Worker.FileList.Count )
                {
                    dgFileList.SelectedIndex = Worker.FileList.Count - 1;
                }
                else if ( Worker.FileList.Count > 0 )
                {
                    dgFileList.SelectedIndex = index;
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the ClearList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ClearList_Click( object sender, RoutedEventArgs e )
        {
            Worker.ClearFileList();
        }

        /// <summary>
        /// Handles the Click event of the ProcessFiles control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ProcessFiles_Click( object sender, RoutedEventArgs e )
        {
            new ProgressDialog()
            {
                Owner = this,
                Worker = Worker
            }.ShowDialog();
        }

        /// <summary>
        /// Handles the SelectionChanged event of the dgFileList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void dgFileList_SelectionChanged( object sender, SelectionChangedEventArgs e )
        {
            var fileItem = ( FileItem ) dgFileList.SelectedItem;

            if ( fileItem != null )
            {
                try
                {
                    using ( var stream = System.IO.File.OpenRead( fileItem.FileName ) )
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = stream;
                        bitmap.EndInit();
                        bitmap.Freeze();

                        ViewModel.ImagePreview = bitmap;
                    }
                }
                catch
                {
                    ViewModel.ImagePreview = null;
                }

                ViewModel.ParameterPreview = fileItem.Metadata;
            }
            else
            {
                ViewModel.ImagePreview = null;
                ViewModel.ParameterPreview = new List<MetadataItem>();
            }
        }

        /// <summary>
        /// Handles the MouseDoubleClick event of the dgMetadataPreview control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        private void dgMetadataPreview_MouseDoubleClick( object sender, System.Windows.Input.MouseButtonEventArgs e )
        {
            if ( ( ( FrameworkElement ) e.OriginalSource ).DataContext is MetadataItem metadata )
            {
                var template = Worker.FilenameTemplate ?? string.Empty;

                template += "{{" + metadata.QualifiedName + "}}";

                Worker.FilenameTemplate = template;
            }
        }

        #endregion
    }
}
