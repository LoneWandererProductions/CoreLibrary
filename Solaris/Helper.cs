/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Solaris
 * FILE:        Helper.cs
 * PURPOSE:     Helper class for image processing and map rendering.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using ExtendedSystemObjects;
using Imaging;
using Brushes = System.Drawing.Brushes;

namespace Solaris;

/// <summary>
///     Helper class that manages image generation tasks.
/// </summary>
internal static partial class Helper
{
    /// <summary>
    ///     The render
    /// </summary>
    private static readonly ImageRender Render = new();

    /// <summary>
    ///     Cache for thread-safe bitmap loading
    /// </summary>
    private static readonly ConcurrentDictionary<string, Bitmap> ImageCache = new();

    /// <summary>
    ///     Generates the final image based on map and textures.
    /// </summary>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    /// <param name="textureSize">Size of the texture.</param>
    /// <param name="textures">The textures.</param>
    /// <param name="map">The map.</param>
    /// <returns>Generated Board as Imaage</returns>
    internal static Bitmap GenerateImage(
        int width, int height, int textureSize,
        Dictionary<int, Texture> textures,
        Dictionary<int, List<int>>? map)
    {
        var background = new Bitmap(width * textureSize, height * textureSize);

        if (map == null)
        {
            return background;
        }

        var tiles = new List<Box>();

        Parallel.ForEach(map, tile =>
        {
            if (tile.Value is not { Count: > 0 })
            {
                return;
            }

            var x = tile.Key % width * textureSize;
            var y = tile.Key / width * textureSize;

            var boxes = tile.Value.Select(textureId =>
            {
                var texture = textures[textureId];
                var image = ImageCache.GetOrAdd(texture.Path, path => Render.GetBitmapFile(path));
                return new Box { X = x, Y = y, Layer = texture.Layer, Image = image };
            });

            lock (tiles)
            {
                tiles.AddRange(boxes);
            }
        });

        tiles.Sort((a, b) => a.Layer.CompareTo(b.Layer));

        foreach (var slice in tiles)
        {
            Render.CombineBitmap(background, slice.Image, slice.X, slice.Y);
        }

        return background;
    }

    /// <summary>
    ///     Generates a grid overlay.
    /// </summary>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    /// <param name="textureSize">Size of the texture.</param>
    /// <returns></returns>
    internal static ImageSource GenerateGrid(int width, int height, int textureSize)
    {
        using var bitmap = new Bitmap(width * textureSize, height * textureSize);
        using var graphics = Graphics.FromImage(bitmap);

        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            graphics.DrawRectangle(Pens.Black, x * textureSize, y * textureSize, textureSize, textureSize);
        }

        return bitmap.ToBitmapImage();
    }

    /// <summary>
    ///     Generates a number overlay.
    /// </summary>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    /// <param name="textureSize">Size of the texture.</param>
    /// <param name="padding">The padding.</param>
    /// <returns>Image with Numbers overlayed.</returns>
    internal static ImageSource GenerateNumbers(int width, int height, int textureSize, int padding = 2)
    {
        using var bitmap = new Bitmap(width * textureSize, height * textureSize);
        using var graphics = Graphics.FromImage(bitmap);
        using var font = new Font(Resources.Font, 8);
        var brush = Brushes.Black;

        var count = 0;

        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++, count++)
        {
            var rect = new RectangleF(
                (x * textureSize) + padding,
                (y * textureSize) + padding,
                textureSize - padding,
                textureSize - padding);

            graphics.DrawString(count.ToString(), font, brush, rect);
        }

        return bitmap.ToBitmapImage();
    }

    /// <summary>
    ///     Adds a tile to the map.
    /// </summary>
    /// <param name="map">The map.</param>
    /// <param name="idTexture">The identifier texture.</param>
    /// <returns>Generate a new Bitmap for the new Map</returns>
    internal static MapChangeResult AddTile(
        Dictionary<int, List<int>>? map, KeyValuePair<int, int> idTexture)
    {
        map ??= new Dictionary<int, List<int>>();
        var (key, value) = idTexture;
        var added = map.AddDistinct(key, value);
        return new MapChangeResult(added, map);
    }

    /// <summary>
    ///     Removes a tile from the map.
    /// </summary>
    /// <param name="map">The map.</param>
    /// <param name="textures">The textures.</param>
    /// <param name="idLayer">The identifier layer.</param>
    /// <returns>Remove a Tile from the map.</returns>
    internal static MapChangeResult RemoveTile(
        Dictionary<int, List<int>>? map, Dictionary<int, Texture> textures, KeyValuePair<int, int> idLayer)
    {
        if (map == null || !map.TryGetValue(idLayer.Key, out var tileList))
        {
            return new MapChangeResult(false, map);
        }

        var updatedList = tileList
            .Where(tile => textures[tile].Layer != idLayer.Value)
            .ToList();

        if (updatedList.Count == tileList.Count)
        {
            return new MapChangeResult(false, map);
        }

        if (updatedList.Count == 0)
        {
            map.Remove(idLayer.Key);
        }
        else
        {
            map[idLayer.Key] = updatedList;
        }

        return new MapChangeResult(true, map);
    }

    /// <summary>
    ///     Adds a tile image to the display layer.
    /// </summary>
    /// <param name="width">The width.</param>
    /// <param name="textureSize">Size of the texture.</param>
    /// <param name="textures">The textures.</param>
    /// <param name="layer">The layer.</param>
    /// <param name="idTile">The identifier tile.</param>
    /// <returns>Add an Image to the image Layer</returns>
    public static Bitmap? AddDisplay(
        int width, int textureSize, Dictionary<int, Texture> textures, Bitmap? layer,
        KeyValuePair<int, int> idTile)
    {
        var (position, tileId) = idTile;
        var x = position % width * textureSize;
        var y = position / width * textureSize;

        var image = ImageCache.GetOrAdd(textures[tileId].Path, path => Render.GetBitmapFile(path));
        return Render.CombineBitmap(layer, image, x, y);
    }

    /// <summary>
    /// Removes a tile image from the display layer.
    /// </summary>
    /// <param name="width">The width.</param>
    /// <param name="textureSize">Size of the texture.</param>
    /// <param name="layer">The layer.</param>
    /// <param name="position">The position.</param>
    /// <returns>Remove areal from Image</returns>
    public static Bitmap RemoveDisplay(int width, int textureSize, Bitmap? layer, int position)
    {
        var x = position % width * textureSize;
        var y = position / width * textureSize;

        return Render.EraseRectangle(layer, x, y, textureSize, textureSize);
    }

    /// <summary>
    /// Displays movement animation.
    /// </summary>
    /// <param name="aurora">The aurora.</param>
    /// <param name="steps">The steps.</param>
    /// <param name="avatar">The avatar.</param>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    /// <param name="textureSize">Size of the texture.</param>
    /// <returns>Status of the animation.</returns>
    internal static async Task DisplayMovement(Aurora aurora, IEnumerable<int> steps, Bitmap? avatar,
        int width, int height, int textureSize)
    {
        aurora.IsEnabled = false;
        using var background = new Bitmap(width * textureSize, height * textureSize);

        foreach (var step in steps)
        {
            var x = step % width * textureSize;
            var y = step / width * textureSize;

            await MoveAvatar(x, y, background, avatar, 100);
        }

        aurora.IsEnabled = true;
        aurora.LayerThree.Source = background.ToBitmapImage();
    }

    /// <summary>
    /// Moves the avatar to a new position with animation.
    /// </summary>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <param name="background">The background.</param>
    /// <param name="avatar">The avatar.</param>
    /// <param name="sleep">The sleep.</param>
    /// <returns>Status of the animation.</returns>
    private static async Task<bool> MoveAvatar(int x, int y, Bitmap? background, Bitmap? avatar, int sleep)
    {
        Render.CombineBitmap(background, avatar, x, y);
        await Task.Delay(sleep);
        return true;
    }
}
