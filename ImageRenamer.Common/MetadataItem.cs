namespace ImageRenamer.Common
{
    /// <summary>
    /// A single item of metadata in a <see cref="FileItem"/> object.
    /// </summary>
    public class MetadataItem
    {
        #region Properties

        /// <summary>
        /// Gets or sets the metadata directory that contains this metadata item.
        /// </summary>
        public string Directory { get; set; }

        /// <summary>
        /// Gets or sets the name of this metadata item.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the qualified name of this metadata item that can be used
        /// in Liquid.
        /// </summary>
        public string QualifiedName => $"{Directory}.{Name}";

        /// <summary>
        /// Gets or sets the value of this metadata item.
        /// </summary>
        public string Value { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="MetadataItem"/> class.
        /// </summary>
        /// <param name="directory">The name of the directory containing this item.</param>
        /// <param name="name">The name of this item.</param>
        /// <param name="value">The value of this item.</param>
        public MetadataItem( string directory, string name, string value )
        {
            Directory = directory;
            Name = name;
            Value = value;
        }

        #endregion
    }
}