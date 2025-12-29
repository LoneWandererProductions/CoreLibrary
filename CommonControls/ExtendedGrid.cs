/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonControls
 * FILE:        ExtendedGrid.cs
 * PURPOSE:     Extension for Grid Control, not elegant but does the job
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ArrangeBraces_for
// ReSharper disable UnusedMember.Global, basic Dummy for later use

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CommonControls
{
    /// <summary>
    ///     The extended grid class.
    /// </summary>
    public static class ExtendedGrid
    {
        /// <summary>
        ///     The height and width of each cell. Min is 1.
        /// </summary>
        private static int _cellHeight = 1;

        /// <summary>
        ///     The cell width
        /// </summary>
        private static int _cellWidth = 1;

        /// <summary>
        ///     Gets the number of columns.
        /// </summary>
        /// <value>
        ///     The columns.
        /// </value>
        public static int Columns { get; private set; }

        /// <summary>
        ///     Gets the number of rows.
        /// </summary>
        public static int Rows { get; private set; }

        /// <summary>
        ///     Gets or sets the default cell size in pixels.
        /// </summary>
        public static int CellSize { get; set; } = 100;

        /// <summary>
        ///     Generates a grid with uniform cell size.
        /// </summary>
        /// <param name="columns">Number of columns in the grid.</param>
        /// <param name="rows">Number of rows in the grid.</param>
        /// <param name="gridLines">Indicates whether grid lines should be shown.</param>
        /// <returns>A <see cref="Grid" /> with uniform cell dimensions.</returns>
        public static Grid ExtendGrid(int columns, int rows, bool gridLines)
        {
            ValidateParameters(columns, rows, CellSize);

            Columns = columns;
            Rows = rows;
            _cellHeight = _cellWidth = CellSize;

            return InitializeGridBase(gridLines, _cellWidth * Columns, _cellHeight * Rows, null, null);
        }

        /// <summary>
        ///     Generates a grid with specified cell dimensions.
        /// </summary>
        /// <param name="columns">Number of columns in the grid.</param>
        /// <param name="rows">Number of rows in the grid.</param>
        /// <param name="width">Cell width in pixels.</param>
        /// <param name="height">Cell height in pixels.</param>
        /// <param name="gridLines">Indicates whether grid lines should be shown.</param>
        /// <returns>A <see cref="Grid" /> with custom cell dimensions.</returns>
        public static Grid ExtendGrid(int columns, int rows, int width, int height, bool gridLines)
        {
            ValidateParameters(columns, rows, width, height);

            Columns = columns;
            Rows = rows;
            _cellWidth = width;
            _cellHeight = height;

            return InitializeGridBase(gridLines, _cellWidth * Columns, _cellHeight * Rows, null, null);
        }

        /// <summary>
        ///     Generates a grid with custom widths for columns and heights for rows.
        /// </summary>
        /// <param name="columnWidths">List of column widths in pixels.</param>
        /// <param name="rowHeights">List of row heights in pixels.</param>
        /// <param name="gridLines">Indicates whether grid lines should be shown.</param>
        /// <returns>A <see cref="Grid" /> with custom row and column dimensions.</returns>
        public static Grid ExtendGrid(List<int> columnWidths, List<int> rowHeights, bool gridLines)
        {
            if (columnWidths == null || rowHeights == null)
            {
                throw new CommonControlsException("Column and row dimensions cannot be null.");
            }

            Columns = columnWidths.Count;
            Rows = rowHeights.Count;

            return InitializeGridBase(gridLines, CalculateTotalWidth(columnWidths), CalculateTotalHeight(rowHeights),
                columnWidths, rowHeights);
        }

        /// <summary>
        ///     Initializes the grid with the base settings and dimensions.
        /// </summary>
        /// <param name="gridLines">Indicates whether grid lines should be shown.</param>
        /// <param name="width">Total grid width in pixels.</param>
        /// <param name="height">Total grid height in pixels.</param>
        /// <param name="columnWidths">Custom column widths if any, null otherwise.</param>
        /// <param name="rowHeights">Custom row heights if any, null otherwise.</param>
        /// <returns>A <see cref="Grid" /> configured with the specified parameters.</returns>
        private static Grid InitializeGridBase(bool gridLines, int width, int height,
            IReadOnlyCollection<int> columnWidths, IReadOnlyCollection<int> rowHeights)
        {
            var dynamicGrid = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                ShowGridLines = gridLines,
                Width = width,
                Height = height,
#if DEBUG
                Background = Brushes.Gray
#endif
            };

            if (columnWidths != null)
            {
                foreach (var colWidth in columnWidths)
                {
                    dynamicGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(colWidth) });
                }
            }
            else
            {
                for (var i = 0; i < Columns; i++)
                {
                    dynamicGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(_cellWidth) });
                }
            }

            if (rowHeights != null)
            {
                foreach (var rowHeight in rowHeights)
                {
                    dynamicGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(rowHeight) });
                }
            }
            else
            {
                for (var i = 0; i < Rows; i++)
                {
                    dynamicGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(_cellHeight) });
                }
            }

            return dynamicGrid;
        }

        /// <summary>
        ///     Validates the grid parameters to ensure they are non-negative.
        /// </summary>
        private static void ValidateParameters(int columns, int rows, int width = 1, int height = 1)
        {
            if (columns < 0 || rows < 0 || width < 1 || height < 1)
            {
                throw new CommonControlsException(ComCtlResources.GridExceptionValidate);
            }
        }

        /// <summary>
        ///     Calculates the total width of the grid.
        /// </summary>
        private static int CalculateTotalWidth(IEnumerable<int> columnWidths)
        {
            var totalWidth = 0;
            foreach (var width in columnWidths)
            {
                if (width < 0)
                {
                    throw new CommonControlsException(ComCtlResources.GridExceptionColumn);
                }

                totalWidth += width;
            }

            return totalWidth;
        }

        /// <summary>
        ///     Calculates the total height of the grid.
        /// </summary>
        private static int CalculateTotalHeight(IEnumerable<int> rowHeights)
        {
            var totalHeight = 0;
            foreach (var height in rowHeights)
            {
                if (height < 0)
                {
                    throw new CommonControlsException(ComCtlResources.GridExceptionRow);
                }

                totalHeight += height;
            }

            return totalHeight;
        }
    }
}
