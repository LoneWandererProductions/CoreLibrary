/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonControls
 * FILE:        CommonControls/ExtendedGrid.cs
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
        ///     The height. Min is 1
        /// </summary>
        private static int _height = 1;

        /// <summary>
        ///     The length. Min is 1
        /// </summary>
        private static int _length = 1;

        /// <summary>
        ///     Gets or sets the columns.
        /// </summary>
        public static int Columns { get; private set; }

        /// <summary>
        ///     Gets or sets the rows.
        /// </summary>
        public static int Rows { get; private set; }

        /// <summary>
        ///     Gets or sets the cell size.
        /// </summary>
        public static int CellSize { get; set; } = 100;

        /// <summary>
        ///     Generate a Standard Uniform Grid like Grid
        ///     Standard Height == Length == CellSize, is initiated with 100
        /// </summary>
        /// <param name="columns">Height, y, Amount of Columns</param>
        /// <param name="rows">Length, x, amount of rows</param>
        /// <param name="gridLines">Show Grid Lines</param>
        /// <returns>The <see cref="Grid" />. An Uniform like Grid.</returns>
        /// <exception cref="CommonControlsException">Wrong Parameters</exception>
        public static Grid ExtendGrid(int columns, int rows, bool gridLines)
        {
            if (CellSize < 0)
            {
                throw new CommonControlsException(string.Concat(ComCtlResources.ErrorWrongParameters,
                    nameof(CellSize)));
            }

            _height = _length = CellSize;

            if (columns < 0)
            {
                throw new CommonControlsException(string.Concat(ComCtlResources.ErrorWrongParameters, nameof(Columns)));
            }

            if (rows < 0)
            {
                throw new CommonControlsException(string.Concat(ComCtlResources.ErrorWrongParameters, nameof(Rows)));
            }

            Columns = columns;
            Rows = rows;
            return GenerateGrid(gridLines);
        }

        /// <summary>
        ///     Generate a Standard Uniform Grid like Grid
        ///     With specified Height Length
        /// </summary>
        /// <param name="columns">Height, y, Amount of Columns</param>
        /// <param name="rows">Length, x, amount of rows</param>
        /// <param name="length">Pixel length of Cell, or Width</param>
        /// <param name="height">Pixel height of Cell, or Height</param>
        /// <param name="gridLines">Grid Lines</param>
        /// <returns>The <see cref="Grid" />. An Uniform like Grid.</returns>
        public static Grid ExtendGrid(int columns, int rows, int length, int height, bool gridLines)
        {
            if (columns < 0)
            {
                throw new CommonControlsException(string.Concat(ComCtlResources.ErrorWrongParameters, nameof(Columns)));
            }

            if (rows < 0)
            {
                throw new CommonControlsException(string.Concat(ComCtlResources.ErrorWrongParameters, nameof(Rows)));
            }

            if (length < 0)
            {
                throw new CommonControlsException(string.Concat(ComCtlResources.ErrorWrongParameters, nameof(length)));
            }

            if (height < 0)
            {
                throw new CommonControlsException(string.Concat(ComCtlResources.ErrorWrongParameters, nameof(height)));
            }

            Columns = columns;
            Rows = rows;

            _length = length;
            _height = height;

            return GenerateGrid(gridLines);
        }

        /// <summary>
        ///     Generate a Standard Uniform Grid like Grid
        ///     With custom Height Length
        /// </summary>
        /// <param name="grdC">The Grid Column.</param>
        /// <param name="grdR">The Grid Row.</param>
        /// <param name="gridLines">Show Grid Lines</param>
        /// <returns>The <see cref="Grid" />. An Uniform like Grid.</returns>
        public static Grid ExtendGrid(List<int> grdC, List<int> grdR, bool gridLines)
        {
            if (grdC == null || Columns < 0)
            {
                throw new CommonControlsException(string.Concat(ComCtlResources.ErrorWrongParameters, nameof(Columns)));
            }

            if (grdR == null || Rows < 0)
            {
                throw new CommonControlsException(string.Concat(ComCtlResources.ErrorWrongParameters, nameof(Rows)));
            }

            Columns = grdC.Count;
            Rows = grdR.Count;

            return GenerateGrid(grdC, grdR, gridLines);
        }

        /// <summary>
        ///     Creates the grid with the provided Parameters
        /// </summary>
        /// <param name="gridLines">Show Grid Lines</param>
        /// <returns>The <see cref="Grid" />. An Uniform like Grid.</returns>
        private static Grid GenerateGrid(bool gridLines)
        {
            var dynamicGrid = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                ShowGridLines = gridLines,
                Width = _length * Columns,
                Height = _height * Rows,
#if DEBUG
                //for Debugging Purposes, yes it should hurt the eyes!
                Background = Brushes.Pink
#endif
            };

            for (var y = 0; y < Columns; y++)
            {
                dynamicGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(_length) });
            }

            for (var y = 0; y < Rows; y++)
            {
                var rows = new RowDefinition { Height = new GridLength(_height) };

                dynamicGrid.RowDefinitions.Add(rows);
            }

            return dynamicGrid;
        }

        /// <summary>
        ///     Creates the grid with the provided Parameters
        /// </summary>
        /// <param name="grdC">The Grid Column.</param>
        /// <param name="grdR">The Grid Row.</param>
        /// <param name="gridLines">Show Grid Lines</param>
        /// <returns>The <see cref="Grid" />. An Uniform like Grid.</returns>
        /// <exception cref="CommonControlsException"></exception>
        private static Grid GenerateGrid(IEnumerable<int> grdC, IEnumerable<int> grdR, bool gridLines)
        {
            if (grdC == null)
            {
                throw new CommonControlsException(string.Concat(ComCtlResources.ErrorWrongParameters, nameof(grdC)));
            }

            if (grdR == null)
            {
                throw new CommonControlsException(string.Concat(ComCtlResources.ErrorWrongParameters, nameof(grdR)));
            }

            var height = 0;
            var length = 0;

            var dynamicGrid = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                ShowGridLines = gridLines,
#if DEBUG
                //for Debugging Purposes, yes it should hurt the eyes!
                Background = Brushes.Pink
#endif
            };

            foreach (var grid in grdC)
            {
                if (grid < 0)
                {
                    throw new CommonControlsException(string.Concat(ComCtlResources.ErrorWrongParameters,
                        nameof(length)));
                }

                dynamicGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(grid) });
                length += grid;
            }

            foreach (var grid in grdR)
            {
                if (grid < 0)
                {
                    throw new CommonControlsException(string.Concat(ComCtlResources.ErrorWrongParameters,
                        nameof(height)));
                }

                dynamicGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(grid) });
                height += grid;
            }

            dynamicGrid.Height = height;
            dynamicGrid.Width = length;

            return dynamicGrid;
        }
    }
}
