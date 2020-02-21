using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Fluid;
using MetadataExtractor;
using Microsoft.Win32;

namespace ImageRenamer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        protected ViewModel ViewModel { get; private set; }

        protected CancellationTokenSource UpdateNewNamesCTS;

        public MainWindow()
        {
            InitializeComponent();

            DataContext = ViewModel = new ViewModel( this );
        }

        private void ResizeListViewColumns( ListView listView )
        {
            GridView gv = ( listView.View as GridView );

            foreach ( var column in gv.Columns )
            {
                if ( double.IsNaN( column.Width ) )
                {
                    column.Width = 1;
                }
                column.Width = double.NaN;
            }
        }

        private List<FileItem> GetFileList( IEnumerable<string> filenames )
        {
            return filenames
                .Select( a => new
                 {
                       Extension = Path.GetExtension( a ).ToUpper(),
                       Filename = a
                 } )
                .Where( a => a.Extension == ".JPG" || a.Extension == ".JPEG" || a.Extension == ".PNG" || a.Extension == ".TIF" || a.Extension == ".TIFF" )
                .Select( a => FileItem.LoadFrom( a.Filename ) )
                .Where( a => a != null )
                .ToList();
        }

        #region Event Handlers

        /// <summary>
        /// Handles the Click event of the OpenDirectory control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OpenDirectory_Click( object sender, RoutedEventArgs e )
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();

            if ( dialog.ShowDialog() ?? false )
            {
                ViewModel.FileList = GetFileList( System.IO.Directory.EnumerateFiles( dialog.SelectedPath ) );
            }

            UpdateNewNames();
        }

        /// <summary>
        /// Handles the Click event of the OpenFiles control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OpenFiles_Click( object sender, RoutedEventArgs e )
        {
            var dialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "Images (PNG, JPG, TIF)|*.png;*.jpg;*.jpeg;*.tif;*.tiff"
            };

            if ( dialog.ShowDialog() == true )
            {
                ViewModel.FileList = GetFileList( dialog.FileNames );
            }

            UpdateNewNames();
        }

        /// <summary>
        /// Handles the SelectionChanged event of the ListView control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void ListView_SelectionChanged( object sender, SelectionChangedEventArgs e )
        {
            var fileItem = ( FileItem ) lvFileList.SelectedItem;

            ViewModel.ImagePreview = fileItem?.FileName;

            if ( fileItem != null )
            {
                ViewModel.ParameterPreview = fileItem.Metadata
                    .Values
                    .SelectMany( a => a )
                    .OrderBy( a => a.Directory )
                    .ToList();
            }
            else
            {
                ViewModel.ParameterPreview = new List<MetadataItem>();
            }

            ResizeListViewColumns( lvMetadataPreview );
        }

        /// <summary>
        /// Handles the SizeChanged event of the lvFileList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SizeChangedEventArgs"/> instance containing the event data.</param>
        private void lvFileList_SizeChanged( object sender, SizeChangedEventArgs e )
        {
            ResizeListViewColumns( lvFileList );
        }

        /// <summary>
        /// Handles the SizeChanged event of the lvMetadataPreview control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SizeChangedEventArgs"/> instance containing the event data.</param>
        private void lvMetadataPreview_SizeChanged( object sender, SizeChangedEventArgs e )
        {
            ResizeListViewColumns( lvMetadataPreview );
        }

        #endregion

        private void btnUpdate_Click( object sender, RoutedEventArgs e )
        {
            UpdateNewNames();
        }

        private void tbFileNameTemplate_TextChanged( object sender, TextChangedEventArgs e )
        {
            UpdateNewNames();
        }
    }

    public class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region Properties

        public string ImagePreview
        {
            get => _imagePreview;
            set
            {
                _imagePreview = value;
                OnPropertyChanged();
            }
        }
        private string _imagePreview;

        public IList<MetadataItem> ParameterPreview
        {
            get => _parameterPreview;
            set
            {
                _parameterPreview = value;
                OnPropertyChanged();
            }
        }
        private IList<MetadataItem> _parameterPreview;

        public IList<FileItem> FileList
        {
            get => _fileList;
            set
            {
                _fileList = value;
                OnPropertyChanged();
            }
        }
        private IList<FileItem> _fileList;

        #endregion

        public ViewModel( MainWindow mainWindow )
        {
        }

        protected void OnPropertyChanged( [CallerMemberName] string propertyName = null )
        {
            if ( propertyName != null )
            {
                PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
            }
        }
    }

    public class MetadataItem
    {
        public string Directory { get; set; }

        public string Name { get; set; }

        public string QualifiedName => $"{Directory}.{Name}";

        public string Value { get; set; }

        public MetadataItem( string directory, string name, string value )
        {
            Directory = directory;
            Name = name;
            Value = value;
        }
    }

    public class FileItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string FileName { get; set; }

        public string OriginalName { get; set; }

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

        public IReadOnlyDictionary<string, IReadOnlyList<MetadataItem>> Metadata { get; set; }

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
                    .Where( a => a.HasName && a.Description != null )
                    .Select( a => new
                    {
                        Directory = CleanKeyForLiquid( a.DirectoryName ),
                        Name = CleanKeyForLiquid( a.Name ),
                        Value = a.Description
                    } )
                    .GroupBy( a => a.Directory )
                    .ToDictionary( a => a.Key,
                        a => ( IReadOnlyList<MetadataItem> ) a.GroupBy( b => b.Name )
                            .Select( b => b.First() )
                            .Select( b => new MetadataItem( a.Key, b.Name, b.Value ) )
                            .ToList() );

                if ( metadata.ContainsKey( "File" ) )
                {
                    ( ( List<MetadataItem> ) metadata["File"] ).Add( new MetadataItem( "File", "Extension", Path.GetExtension( filename ).Substring( 1 ) ) );
                }

                file.Metadata = metadata;

                return file;
            }
            catch
            {
                return null;
            }
        }

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
    }
}
