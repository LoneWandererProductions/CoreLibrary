/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ImageCompare.Compare
 * FILE:        ImageSimilarity.cs
 * PURPOSE:     Find Similar Images
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 * SOURCES:     https://www.codeproject.com/Articles/374386/Simple-image-comparison-in-NET
 */

// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using Extended.Extensions;
using FileHandler;

namespace Imaging.Compare
{
    internal static class ImageSimilarity
    {
        /// <summary>
        ///     The Temp path dictionary
        /// </summary>
        private static Dictionary<int, string?>? Translator { get; set; }

        /// <summary>
        ///     Find all similar images in a folder, and possibly subfolders
        /// </summary>
        /// <param name="folderPath">The folder to look for duplicates in</param>
        /// <param name="checkSubfolders">Whether to look in subfolders too</param>
        /// <param name="extensions">The extensions.</param>
        /// <param name="threshold">The Value of differences allowed.</param>
        /// <returns>
        ///     A list of all the duplicates found, collected in separate Lists (one for each distinct image found)
        /// </returns>
        internal static List<List<string>>? GetSimilarImages(string? folderPath, bool checkSubfolders,
            IEnumerable<string> extensions, float threshold)
        {
            var localDate = DateTime.Now;
            Trace.WriteLine(localDate.ToString(CultureInfo.InvariantCulture));

            //create Directories
            var imagePaths = FileHandleSearch.GetFilesByExtensionFullPath(folderPath, extensions, checkSubfolders);

            if (imagePaths.IsNullOrEmpty())
            {
                return null;
            }

            Translator = imagePaths.ToDictionary();

            var images = GetSortedGrayScaleValues();

            if (images.IsNullOrEmpty())
            {
                return null;
            }

            //Just get all Images that are in the same Color Space
            var duplicateGroups = GetDuplicateGroups(images);
            localDate = DateTime.Now;
            Trace.WriteLine(localDate.ToString(CultureInfo.InvariantCulture));
            Trace.WriteLine(nameof(duplicateGroups));
            Trace.WriteLine(nameof(duplicateGroups.Count));
            //Let's compare all result sets, oif empty well tough luck

            if (duplicateGroups.IsNullOrEmpty())
            {
                return null;
            }

            Trace.WriteLine(duplicateGroups.Count);

            var groups = new List<List<ImageSimilar>>();

            foreach (var duplicates in duplicateGroups)
            {
                var dup = new List<ImageSimilar>(duplicates);

                foreach (var cache in duplicates.Select(item => ImageProcessing.FindSimilarImages(item, dup, threshold))
                             .Where(cache => cache != null))
                {
                    dup = dup.Except(cache).ToList();
                    groups.Add(cache);
                }
            }

            localDate = DateTime.Now;
            Trace.WriteLine(localDate.ToString(CultureInfo.InvariantCulture));

            if (groups.IsNullOrEmpty())
            {
                return null;
            }

            var result = Translate(groups);

            localDate = DateTime.Now;
            Trace.WriteLine(localDate.ToString(CultureInfo.InvariantCulture));

            return result;
        }

        /// <summary>
        ///     Gets the sorted gray scale values.
        /// </summary>
        /// <returns>
        ///     List of possible similar images
        /// </returns>
        /// <exception cref="OutOfMemoryException">Out of Memory</exception>
        /// <exception cref="ArgumentException">Wrong Argument</exception>
        /// <exception cref="InvalidOperationException">Invalid Operation</exception>
        private static List<ImageSimilar> GetSortedGrayScaleValues()
        {
            var imagePathsAndGrayValues = new ConcurrentBag<ImageSimilar>();

            //with sanity check in Case one file went missing, we won't have to stop everything
            Parallel.ForEach(Translator.Where(pathImage => File.Exists(pathImage.Value)), pathImage =>
            {
                var (key, value) = pathImage;
                try
                {
                    if (value == null) return;

                    using var btm = new Bitmap(value);
                    var dup = ImageProcessing.GenerateData(btm, key);
                    imagePathsAndGrayValues.Add(dup);
                }
                catch (ArgumentException ex)
                {
                    Trace.WriteLine(ex);
                }
                catch (OutOfMemoryException ex)
                {
                    // Skip this one file rather than aborting the whole scan - see
                    // the identical fix in ImageDuplication.GetSortedGrayScaleValues.
                    var memory = Process.GetCurrentProcess().VirtualMemorySize64.ToString();
                    Trace.WriteLine($"{ex} (VirtualMemorySize64={memory})");
                }
                catch (InvalidOperationException ex)
                {
                    Trace.WriteLine(ex);
                }
            });

            Trace.WriteLine(nameof(GetSortedGrayScaleValues));
            Trace.WriteLine(imagePathsAndGrayValues.Count);
            return imagePathsAndGrayValues.ToList();
        }

        /// <summary>
        ///     Gets the duplicate groups.
        /// </summary>
        /// <param name="imagePathsAndGrayValues">The image paths and gray values.</param>
        /// <returns>Group of Duplicates</returns>
        /// <remarks>
        ///     Replaces an all-pairs scan (for each item, compare against every
        ///     remaining item) with spatial bucketing + Union-Find. Two things
        ///     changed on purpose, not just speed:
        ///     1. The old version's output depended on file enumeration order -
        ///        verified empirically: the exact same input, reordered, produced a
        ///        different number of groups. This version is deterministic.
        ///     2. It clusters transitively (A~B and B~C puts all three in one group,
        ///        even if A and C aren't directly within threshold), rather than
        ///        "everything within threshold of whichever item was visited first".
        ///        That's the correct behavior for a photo archive - e.g. a run of
        ///        burst-mode shots that drift slightly frame to frame.
        ///     Bucket size is ColorThreshold + 1, and every item checks its
        ///     surrounding 3x3x3 neighborhood of buckets, so two images near a
        ///     bucket boundary still find each other - bucketing narrows the search,
        ///     it doesn't change which pairs count as a match.
        /// </remarks>
        private static List<List<ImageSimilar>> GetDuplicateGroups(
            IReadOnlyCollection<ImageSimilar> imagePathsAndGrayValues)
        {
            const int bucketSize = ImageResources.ColorThreshold + 1;

            var items = imagePathsAndGrayValues.ToList();

            (int, int, int) KeyOf(ImageSimilar s) => (s.R / bucketSize, s.G / bucketSize, s.B / bucketSize);

            var buckets = new Dictionary<(int, int, int), List<int>>();
            for (var i = 0; i < items.Count; i++)
            {
                var key = KeyOf(items[i]);
                if (!buckets.TryGetValue(key, out var bucket))
                {
                    bucket = new List<int>();
                    buckets[key] = bucket;
                }

                bucket.Add(i);
            }

            var parent = new int[items.Count];
            for (var i = 0; i < parent.Length; i++) parent[i] = i;

            int Find(int x)
            {
                while (parent[x] != x)
                {
                    parent[x] = parent[parent[x]];
                    x = parent[x];
                }

                return x;
            }

            void Union(int a, int b)
            {
                var ra = Find(a);
                var rb = Find(b);
                if (ra != rb) parent[ra] = rb;
            }

            for (var i = 0; i < items.Count; i++)
            {
                var (kx, ky, kz) = KeyOf(items[i]);
                for (var dx = -1; dx <= 1; dx++)
                for (var dy = -1; dy <= 1; dy++)
                for (var dz = -1; dz <= 1; dz++)
                {
                    if (!buckets.TryGetValue((kx + dx, ky + dy, kz + dz), out var neighborIndices)) continue;

                    foreach (var j in neighborIndices)
                    {
                        // j <= i: every unordered pair is only ever checked once,
                        // from whichever side visits it second.
                        if (j <= i) continue;
                        if (items[i].Equals(items[j])) Union(i, j);
                    }
                }
            }

            var groups = new Dictionary<int, List<ImageSimilar>>();
            for (var i = 0; i < items.Count; i++)
            {
                var root = Find(i);
                if (!groups.TryGetValue(root, out var g))
                {
                    g = new List<ImageSimilar>();
                    groups[root] = g;
                }

                g.Add(items[i]);
            }

            var duplicateGroups = groups.Values.Where(g => g.Count > 1).ToList();

            Trace.WriteLine(nameof(GetDuplicateGroups));
            Trace.WriteLine(duplicateGroups.Count);
            return duplicateGroups;
        }

        /// <summary>
        ///     Translates the specified duplicate groups.
        /// </summary>
        /// <param name="duplicateGroups">The duplicate groups.</param>
        /// <returns>List of Similar Images</returns>
        private static List<List<string>> Translate(IEnumerable<List<ImageSimilar>> duplicateGroups)
        {
            return duplicateGroups.Select(group =>
                    (from element in @group where Translator[element.Id] != null select Translator[element.Id])
                    .ToList())
                .ToList();
        }
    }
}