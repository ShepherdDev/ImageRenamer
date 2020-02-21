using System;
using System.Collections.Generic;
using System.Collections.Specialized;

using AppKit;

using Foundation;

using ImageRenamer.Common;

namespace ImageRenamer.Mac
{
    /// <summary>
    /// Handles the UI coordination for the main view.
    /// </summary>
    public partial class MainViewController : NSViewController
	{
        #region Preference Keys

        /// <summary>
        /// The application preference key to get or set the filename template.
        /// </summary>
        public const string PreferenceFilenameTemplateKey = "FilenameTemplate";

        #endregion

        #region Properties

        /// <summary>
        /// The current filename template presented in tye UI.
        /// </summary>
        /// <remarks>
        /// Bound property.
        /// </remarks>
        [Export( nameof( FilenameTemplate ) )]
        public string FilenameTemplate
        {
            get => Worker.FilenameTemplate;
            set
            {
                WillChangeValue( nameof( FilenameTemplate ) );
                Worker.FilenameTemplate = value;
                DidChangeValue( nameof( FilenameTemplate ) );
            }
        }

        /// <summary>
        /// The worker object that will handle renaming the files.
        /// </summary>
        protected FileWorker Worker { get; private set; } = new FileWorker();

        /// <summary>
        /// The list of metadata items to be presented in the UI for the
        /// currently selected file.
        /// </summary>
        protected IReadOnlyList<MetadataItem> MetadataPreview { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="MainViewController"/> class.
        /// </summary>
        /// <param name="handle">The handle of the Mac object to be instantiated with.</param>
        public MainViewController( IntPtr handle ) : base( handle )
        {
            Worker.FileList.CollectionChanged += FileList_CollectionChanged;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Called when the view has loaded but not yet been displayed.
        /// </summary>
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            //
            // Prepare the data source for the file list view.
            //
            fileListView.DataSource = new TableDataSource<FileItem>( () => Worker.FileList,
                nameof( FileItem.OriginalName ),
                nameof( FileItem.NewName ) );

            //
            // Prepare the data source for the metadata list view.
            //
            metadataListView.DataSource = new TableDataSource<MetadataItem>( () => MetadataPreview,
                nameof( MetadataItem.QualifiedName ),
                nameof( MetadataItem.Value ) );

            fileListView.SelectionDidChange += FileListView_SelectionDidChange;
            metadataListView.DoubleClick += MetadataListView_DoubleClick;

            FilenameTemplate = NSUserDefaults.StandardUserDefaults.StringForKey( PreferenceFilenameTemplateKey ) ?? string.Empty;
        }

        /// <summary>
        /// Called just before the view disappears.
        /// </summary>
        public override void ViewWillDisappear()
        {
            base.ViewWillDisappear();

            NSUserDefaults.StandardUserDefaults.SetString( FilenameTemplate, PreferenceFilenameTemplateKey );
        }

        /// <summary>
        /// Final preparations for the transition to the target segue.
        /// </summary>
        /// <param name="segue">The segue that will perform the transition.</param>
        /// <param name="sender">The sender that initiated the action.</param>
        public override void PrepareForSegue( NSStoryboardSegue segue, NSObject sender )
        {
            base.PrepareForSegue( segue, sender );

            if ( segue.Identifier == "ShowProgress" )
            {
                var progressDialog = ( ProgressDialogController ) segue.DestinationController;

                progressDialog.Worker = Worker;
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the CollectionChanged event of the FileList.
        /// </summary>
        /// <param name="sender">The object that caused the event.</param>
        /// <param name="e">The event arguments</param>
        private void FileList_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
        {
            BeginInvokeOnMainThread( () => fileListView.ReloadData() );

            foreach ( FileItem row in Worker.FileList )
            {
                row.PropertyChanged += FileItem_PropertyChanged;
            }
        }

        /// <summary>
        /// Handles the PropertyChanged event of the FileItem.
        /// </summary>
        /// <param name="sender">The object that caused the event.</param>
        /// <param name="e">The event arguments</param>
        private void FileItem_PropertyChanged( object sender, System.ComponentModel.PropertyChangedEventArgs e )
        {
            if ( e.PropertyName == nameof( FileItem.NewName ) )
            {
                int index = Worker.FileList.IndexOf( ( FileItem ) sender );
                BeginInvokeOnMainThread( () => fileListView.ReloadData( NSIndexSet.FromIndex( index ), NSIndexSet.FromIndex( 1 ) ) );
            }
        }

        /// <summary>
        /// Handles the SelectionDidChange event of the FileListView.
        /// </summary>
        /// <param name="sender">The object that caused the event.</param>
        /// <param name="e">The event arguments</param>
        private void FileListView_SelectionDidChange( object sender, EventArgs e )
        {
            var index = ( int ) fileListView.SelectedRow;

            //
            // Update the previews.
            //
            if ( index < 0 )
            {
                MetadataPreview = null;
                previewImageView.Image = null;
            }
            else
            {
                MetadataPreview = Worker.FileList[index].Metadata;
                previewImageView.Image = new NSImage( Worker.FileList[index].FileName );
            }

            metadataListView.ReloadData();
        }

        /// <summary>
        /// Handles the DoubleClick event of the MetadataListView.
        /// </summary>
        /// <param name="sender">The object that caused the event.</param>
        /// <param name="e">The event arguments</param>
        private void MetadataListView_DoubleClick( object sender, EventArgs e )
        {
            if ( metadataListView.SelectedRow >= 0 )
            {
                var metadata = MetadataPreview[( int ) metadataListView.SelectedRow];
                var template = FilenameTemplate ?? string.Empty;

                template += "{{" + metadata.QualifiedName + "}}";

                FilenameTemplate = template;
            }
        }

        /// <summary>
        /// Called when the user wants to add additional files.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        [Action( "addFiles:" )]
        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Style", "IDE0060:Remove unused parameter", Justification = "Required by Objective-C" )]
        public void AddFiles( NSObject sender )
		{
            var panel = NSOpenPanel.OpenPanel;

            panel.AllowsMultipleSelection = true;

            if ( panel.RunModal() != ( int ) NSModalResponse.OK )
            {
                return;
            }

            Worker.AppendFileList( panel.Filenames );

            fileListView.ReloadData();
		}

        /// <summary>
        /// Called when the user wants to clear the file list.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        [Action( "clearFileList:" )]
        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Style", "IDE0060:Remove unused parameter", Justification = "Required by Objective-C" )]
        public void ClearFileList( NSObject sender )
        {
            Worker.ClearFileList();
        }

        /// <summary>
        /// Called when the user wants to remove a single file from the file list.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        [Action( "removeFile:" )]
        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Style", "IDE0060:Remove unused parameter", Justification = "Required by Objective-C" )]
        public void RemoveFile( NSObject sender )
        {
            if ( fileListView.SelectedRow >= 0 )
            {
                Worker.RemoveFileAt( ( int ) fileListView.SelectedRow );

                if ( Worker.FileList.Count == 0 )
                {
                    fileListView.DeselectAll( this );
                }
                else if ( fileListView.SelectedRow >= Worker.FileList.Count )
                {
                    fileListView.SelectRow( Worker.FileList.Count - 1, false );
                }

                FileListView_SelectionDidChange( fileListView, EventArgs.Empty );
            }
        }

        /// <summary>
        /// Called when the user taps on the PRocess Files button.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        [Action( "processFiles:" )]
        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Style", "IDE0060:Remove unused parameter", Justification = "Required by Objective-C" )]
        public void ProcessFiles_Clicked( NSObject sender )
		{
            PerformSegue( "ShowProgress", this );
		}

        #endregion
    }
}
