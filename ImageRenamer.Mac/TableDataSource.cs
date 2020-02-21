using System;
using System.Collections.Generic;

using AppKit;

using Foundation;

namespace ImageRenamer.Mac
{
    /// <summary>
    /// Provides a data source for <see cref="NSTableView"/> views.
    /// </summary>
    /// <typeparam name="T">The type of object to be displayed.</typeparam>
    public class TableDataSource<T> : NSTableViewDataSource
    {
        #region Properties

        /// <summary>
        /// Gets the items to be used for processing.
        /// </summary>
        protected Func<IReadOnlyList<T>> GetItems { get; private set; }

        /// <summary>
        /// The property names associated with the table columns.
        /// </summary>
        protected string[] ColumnProperties { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="TableDataSource{T}"/> class.
        /// </summary>
        /// <param name="getItems">The function that provides the items to be displayed.</param>
        /// <param name="columnProperties">The list of properties to be used as column values.</param>
        public TableDataSource( Func<IReadOnlyList<T>> getItems, params string[] columnProperties )
        {
            GetItems = getItems;
            ColumnProperties = columnProperties;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="TableDataSource{T}"/> class.
        /// </summary>
        /// <param name="items">The items to be displayed.</param>
        /// <param name="columnProperties">The list of properties to be used as column values.</param>
        public TableDataSource( IReadOnlyList<T> items, params string[] columnProperties )
            : this( () => items, columnProperties )
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the number of rows in the table.
        /// </summary>
        /// <param name="tableView">The <see cref="NSTableView"/> that is requesting the information.</param>
        /// <returns>The number of rows of data.</returns>
        public override nint GetRowCount( NSTableView tableView )
        {
            return GetItems()?.Count ?? 0;
        }

        /// <summary>
        /// Gets the object value for the row and column.
        /// </summary>
        /// <param name="tableView">The <see cref="NSTableView"/> that is requesting the information.</param>
        /// <param name="tableColumn">The column associated with the row.</param>
        /// <param name="row">The row of data to be displayed.</param>
        /// <returns>The value to be displayed in the cell.</returns>
        public override NSObject GetObjectValue( NSTableView tableView, NSTableColumn tableColumn, nint row )
        {
            var items = GetItems();

            if ( items == null || items.Count <= row )
            {
                return null;
            }

            var item = items[( int ) row];
            int columnIndex = Array.IndexOf( tableView.TableColumns(), tableColumn );

            var propertyInfo = item.GetType().GetProperty( ColumnProperties[columnIndex] );
            if ( propertyInfo == null )
            {
                return ( NSString ) string.Empty;
            }

            var value = propertyInfo.GetValue( item );
            if ( value == null )
            {
                return ( NSString ) string.Empty;
            }

            return ( NSString ) value.ToString();
        }

        #endregion
    }
}
