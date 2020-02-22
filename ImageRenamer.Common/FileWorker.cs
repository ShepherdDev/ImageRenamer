using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Fluid;

namespace ImageRenamer.Common
{
    /// <summary>
    /// Handles processing of files.
    /// </summary>
    public class FileWorker : INotifyPropertyChanged
    {
        #region Events

        /// <summary>
        /// Event to be triggered when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Properties

        /// <summary>
        /// The file list that will be processed.
        /// </summary>
        public ObservableCollection<FileItem> FileList => _fileList;
        private readonly ObservableRangeCollection<FileItem> _fileList = new ObservableRangeCollection<FileItem>();

        /// <summary>
        /// The filename template that will be used when generating new names.
        /// </summary>
        public string FilenameTemplate
        {
            get => _filenameTemplate;
            set
            {
                _filenameTemplate = value;
                OnPropertyChanged();

                _ = UpdateNewNames();
            }
        }
        private string _filenameTemplate = string.Empty;

        /// <summary>
        /// Cancellation token for use during the UpdateNewNames() methods.
        /// </summary>
        protected CancellationTokenSource UpdateNewNamesCTS { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Clears the list of files to be processed.
        /// </summary>
        public void ClearFileList()
        {
            FileList.Clear();
        }

        /// <summary>
        /// Appends a new collection of filenames to be processed.
        /// </summary>
        /// <param name="filenames">The full paths and filenames to be processed.</param>
        public void AppendFileList( IEnumerable<string> filenames )
        {
            var fileList = filenames
                .Where( a => !_fileList.Any( b => b.FileName == a ) )
                .Select( a => new
                {
                    Extension = Path.GetExtension( a ).ToUpper(),
                    Filename = a
                } )
                .Where( a => a.Extension == ".JPG" || a.Extension == ".JPEG" || a.Extension == ".PNG" || a.Extension == ".TIF" || a.Extension == ".TIFF" )
                .Select( a => FileItem.LoadFrom( a.Filename ) )
                .Where( a => a != null )
                .ToList();

            _fileList.AddRange( fileList );

            _ = UpdateNewNames();
        }

        /// <summary>
        /// Removes a single file entry from the list of files to be processed.
        /// </summary>
        /// <param name="position">The position of the file to be removed.</param>
        public void RemoveFileAt( int position )
        {
            _fileList.RemoveAt( position );
        }

        /// <summary>
        /// Renames the files in the <see cref="FileList"/>.
        /// </summary>
        /// <param name="progressCallback">Called after each file is renamed to provide progress feedback.</param>
        /// <param name="cancellationToken">The cancellation token to tell us when to abort.</param>
        /// <returns>A task that represents this rename process.</returns>
        public async Task RenameFiles( Action<int, int> progressCallback, CancellationToken cancellationToken )
        {
            List<FileItem> processed = new List<FileItem>();

            await UpdateNewNames();

            //
            //  Ensure all files have new names.
            //
            if ( _fileList.Any( a => string.IsNullOrWhiteSpace( a.NewName ) ) )
            {
                throw new Exception( "One or more files have empty names." );
            }

            //
            // Loop through the list of files and rename each one.
            //
            for ( int i = 0; i < _fileList.Count && !cancellationToken.IsCancellationRequested; i++ )
            {
                var file = _fileList[i];

                try
                {
                    RenameFile( file.FileName, Path.Combine( Path.GetDirectoryName( file.FileName ), file.NewName ) );
                }
                catch
                {
                    await Device.InvokeOnMainThreadAsync( () => _fileList.RemoveRange( processed ) );
                    throw;
                }

                processed.Add( file );

                progressCallback?.Invoke( processed.Count, _fileList.Count );
            }

            await Device.InvokeOnMainThreadAsync( () => _fileList.RemoveRange( processed ) );
        }

        /// <summary>
        /// Renames a single file by doing any necessary hash replacement.
        /// </summary>
        /// <param name="originalName">The original filename to be renamed.</param>
        /// <param name="newNameTemplate">The new filename template to be used (hashes).</param>
        private void RenameFile( string originalName, string newNameTemplate )
        {
            if ( newNameTemplate.Contains( '#' ) )
            {
                var hashStart = newNameTemplate.IndexOf( '#' );
                int hashLength = 1;

                while ( newNameTemplate.Length > hashStart + hashLength && newNameTemplate[hashStart + hashLength] == '#' )
                {
                    hashLength += 1;
                }

                var hash = new string( '#', hashLength );
                for ( int i = 1; i < 100000; i++ )
                {
                    var newName = newNameTemplate.Replace( hash, i.ToString( "D" + hashLength.ToString() ) );

                    if ( !File.Exists( newName ) )
                    {
                        File.Move( originalName, newName );
                        return;
                    }
                }
            }
            else
            {
                if ( File.Exists( newNameTemplate ) )
                {
                    throw new Exception( $"Filename '{newNameTemplate}' already exists." );
                }

                File.Move( originalName, newNameTemplate );
            }
        }

        /// <summary>
        /// Sets up the task which will update all the new names of the files.
        /// </summary>
        /// <returns>A task that represents this action.</returns>
        private async Task UpdateNewNames()
        {
            if ( UpdateNewNamesCTS != null )
            {
                UpdateNewNamesCTS.Cancel();
                UpdateNewNamesCTS = null;
            }

            UpdateNewNamesCTS = new CancellationTokenSource();

            await Task.Run( async () => await UpdateNewNames( FilenameTemplate, UpdateNewNamesCTS.Token ) );
        }

        /// <summary>
        /// Sets up the task which will update all the new names of the files.
        /// </summary>
        /// <param name="source">The Liquid template to use.</param>
        /// <param name="cancellationToken">The cancellation token that will signal when we need to abort.</param>
        /// <returns>A task that represents this action.</returns>
        private Task UpdateNewNames( string source, CancellationToken cancellationToken )
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            if ( FluidTemplate.TryParse( source, out var template ) )
            {
                foreach ( var file in _fileList )
                {
                    var newName = GetNameForFluidTemplate( template, file );

                    if ( cancellationToken.IsCancellationRequested )
                    {
                        return Task.FromCanceled( cancellationToken );
                    }

                    file.NewName = newName;
                }
            }

            System.Diagnostics.Debug.WriteLine( $"Finished in {sw.ElapsedMilliseconds}" );

            return Task.CompletedTask;
        }

        /// <summary>
        /// Gets the new filename for the given template.
        /// </summary>
        /// <param name="template">The compiled liquid template.</param>
        /// <param name="file">The file to be processed.</param>
        /// <returns>The suggested new filename.</returns>
        private string GetNameForFluidTemplate( FluidTemplate template, FileItem file )
        {
            try
            {
                var context = new TemplateContext();
                context.MemberAccessStrategy.Register( typeof( MetadataItem ) );
                var metadataDirectories = file.Metadata.GroupBy( a => a.Directory );

                foreach ( var directory in metadataDirectories )
                {
                    var values = new Dictionary<string, string>();

                    foreach ( var value in directory )
                    {
                        values.Add( value.Name, value.Value );
                    }

                    context.SetValue( directory.Key, values );
                }

                return template.Render( context );
            }
            catch ( Exception ex )
            {
                System.Diagnostics.Debug.WriteLine( ex.Message );
                return string.Empty;
            }
        }

        /// <summary>
        /// Called when a property value has changed to notify any watchers.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected void OnPropertyChanged( [CallerMemberName] string propertyName = null )
        {
            if ( propertyName != null )
            {
                PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
            }
        }

        #endregion
    }
}
