using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using ImageRenamer.Common;

namespace ImageRenamer.Wpf
{
    /// <summary>
    /// The view model for the <see cref="MainWindow" />
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public class ViewModel : INotifyPropertyChanged
    {
        #region Events

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the image preview.
        /// </summary>
        /// <value>
        /// The image preview.
        /// </value>
        public ImageSource ImagePreview
        {
            get => _imagePreview;
            set
            {
                _imagePreview = value;
                OnPropertyChanged();
            }
        }
        private ImageSource _imagePreview;

        /// <summary>
        /// Gets or sets the parameter preview.
        /// </summary>
        /// <value>
        /// The parameter preview.
        /// </value>
        public IReadOnlyList<MetadataItem> ParameterPreview
        {
            get => _parameterPreview;
            set
            {
                _parameterPreview = value;
                OnPropertyChanged();
            }
        }
        private IReadOnlyList<MetadataItem> _parameterPreview;

        /// <summary>
        /// Gets the worker.
        /// </summary>
        /// <value>
        /// The worker.
        /// </value>
        public FileWorker Worker { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModel"/> class.
        /// </summary>
        /// <param name="worker">The worker.</param>
        public ViewModel( FileWorker worker )
        {
            Worker = worker;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Called when a property value has changed.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
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
