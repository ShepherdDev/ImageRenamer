using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;

using MetadataExtractor;

namespace ImageRenamer.Common
{
    /// <summary>
    /// Identifies a single file that is going to be renamed.
    /// </summary>
    public class FileItem : INotifyPropertyChanged
    {
        #region Events

        /// <summary>
        /// Raised when a property's value has changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the full path and filename of this file.
        /// </summary>
        public string FileName { get; private set; }

        /// <summary>
        /// Gets the original filename of this file.
        /// </summary>
        public string OriginalName { get; private set; }

        /// <summary>
        /// Gets or sets the new filename of this file.
        /// </summary>
        public string NewName
        {
            get => _newName;
            set
            {
                _newName = value;
                PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( nameof( NewName ) ) );
            }
        }
        private string _newName;

        /// <summary>
        /// Gets the metadata for this file.
        /// </summary>
        public IReadOnlyList<MetadataItem> Metadata { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a new <see cref="FileItem"/> from the given path and
        /// filename on disk.
        /// </summary>
        /// <param name="filename">The full path to the file.</param>
        /// <returns>A new <see cref="FileItem"/> or <c>null</c> if it could not be parsed.</returns>
        public static FileItem LoadFrom( string filename )
        {
            var file = new FileItem
            {
                FileName = filename,
                OriginalName = Path.GetFileName( filename ),
                NewName = string.Empty
            };

            try
            {
                var metadataDirectories = ImageMetadataReader.ReadMetadata( file.FileName );
                var metadata = metadataDirectories.SelectMany( a => a.Tags )
                    .Where( a => a.HasName )
                    .Where( a =>
                    {
                        bool hasDescription;
                        try
                        {
                            hasDescription = a.Description != null;
                        }
                        catch
                        {
                            hasDescription = false;
                        }

                        return hasDescription;
                    } )
                    .Select( a => new MetadataItem( CleanKeyForLiquid( a.DirectoryName ), CleanKeyForLiquid( a.Name ), a.Description ) )
                    .ToList()
                    .GroupBy( a => a.QualifiedName )
                    .Select( a => a.First() )
                    .ToList();

                if ( !metadata.Any( a => a.Directory == "File" && a.Name == "Extension" ) )
                {
                    metadata.Add( new MetadataItem( "File", "Extension", Path.GetExtension( filename ).Substring( 1 ) ) );
                }

                metadata = metadata.OrderBy( a => a.Directory )
                    .ThenBy( a => a.Name )
                    .ToList();

                file.Metadata = metadata;

                return file;
            }
            catch ( Exception ex )
            {
                System.Diagnostics.Debug.WriteLine( ex.Message );
                return null;
            }
        }

        /// <summary>
        /// Scrub the given key to ensure it's safe to use in Liquid syntax.
        /// </summary>
        /// <param name="key">The property key to be scrubbed.</param>
        /// <returns>A string that contains the Liquid safe key.</returns>
        private static string CleanKeyForLiquid( string key )
        {
            StringBuilder sb = new StringBuilder( key.Length );

            foreach ( var c in key )
            {
                if ( c >= 'a' && c <= 'z' )
                {
                    sb.Append( c );
                }
                else if ( c >= 'A' && c <= 'Z' )
                {
                    sb.Append( c );
                }
                else if ( c >= '0' && c <= '9' )
                {
                    sb.Append( c );
                }
                else if ( c == '_' )
                {
                    sb.Append( c );
                }
            }

            return sb.ToString();
        }

        #endregion
    }
}