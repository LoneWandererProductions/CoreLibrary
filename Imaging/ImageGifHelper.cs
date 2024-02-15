using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;

namespace Imaging
{
    public static class ImageGifHelper
    {
        //TODO needs to be set
        public static readonly Dictionary<string, Color> ColorTable = new();

        /// <summary>
        ///     https://debugandrelease.blogspot.com/2018/12/creating-gifs-in-c.html
        ///     https://learn.microsoft.com/en-us/dotnet/api/system.io.binarywriter.write?view=net-8.0#system-io-binarywriter-write(system-byte)
        ///     GIFs must start with a header block;
        ///     The first 3 bytes must be "GIF",
        ///     the next 3 bytes must be the version, that we are encoding the .GIF in ("87a" or "89a")
        /// </summary>
        /// <returns>gif Header</returns>
        internal static List<byte> WriteHeader()
        {
            var lst = new List<byte>
            {
                (byte)'G',
                (byte)'I',
                (byte)'F',
                (byte)'8',
                (byte)'9',
                (byte)'a'
            };
            return lst;
        }

        /// <summary>
        ///     https://learn.microsoft.com/en-us/dotnet/api/system.io.binarywriter.write?view=net-8.0#system-io-binarywriter-write(system-byte)
        ///     The logical screen descriptor holds global values
        ///     for our .GIF file
        /// </summary>
        /// <returns>Logical Screen Descriptor</returns>
        internal static List<byte> WriteLogicalScreenDescriptor(int width, int height)
        {
            var lst = new List<byte>
            {
                // The second 2 bytes are the height of the canvas
                // The first 2 bytes are the width of the canvas (the entire .GIF)
                (byte)width, (byte)height
            };

            // The first 2 bytes are the width of the canvas (the entire .GIF)
            // The second 2 bytes are the height of the canvas
            // The next byte is a packed byte:
            //      The first 3 bits are the size of the global color table
            //          (ex.  if these 3 bits equal "1" in binary, the global color table holds 2^(n+1) colors)
            //          (aka. if these 3 bits equal "1" in binary, we can use a total of 4 colors in our .GIF)
            //      The next bit is if the colors in the global color table are sorted in decreasing frequency in the image
            //          (this helps speed up decoding the .GIF; can stay at "0")
            //      The next 3 bits are the color resolution of the image
            //          (color resolution == number of bits per primary color available to the original image, minus 1)
            //      The last bit is if we have a global color table (1 = yes, 0 = no)

            // Packed field
            var array = new BitArray(new byte[1]);
            var gcts = LogicalScreenDescriptorGlobalColorTableSize();
            array[2] = gcts[2];
            array[1] = gcts[1];
            array[0] = gcts[0]; // Global color table size
            array[3] = false; // Sort flag
            array[6] = true;
            array[5] = true;
            array[4] = true; // Color resolution (64 colors)
            array[7] = true; // Global color table flag ("1" since we are using a global color table)

            var bytes = new byte[1];
            array.CopyTo(bytes, 0);

            lst.AddRange(bytes);

            // The next byte is the background color index (the index of the color from the global color table)
            // The last byte is the pixel aspect ratio

            lst.Add(0); // Background color index
            lst.Add(0); // Pixel aspect ratio

            return lst;
        }

        //TODO change return value
        internal static List<Color> WriteGlobalColorTable()
        {
            //TODO Wrong
            //RGB values in hexadecimal
            var lst = new List<Color>();

            //      The global color table includes all colors the .GIF
            //      will use, in this format (where "C" means color):
            //      C1 red value, C1 green value, C1 blue value,
            //      C2 red value, C2 green value, C2 blue value,
            //      ...

            for (var i = 0; i < ColorTable.Count; i++)
            {
                lst.Add(Color.Red);
                lst.Add(Color.Green);
                lst.Add(Color.Blue);
            }

            // Add additional placeholders if we do not have the same
            // number of colors that match the GCTS (global color table size)
            // 
            // Please see the LogicalScreenDescriptorGlobalColorTableSize
            // to see the number of colors we require for a given GCTS

            var gcts = LogicalScreenDescriptorGlobalColorTableSize();
            var bytes = new byte[1];
            gcts.CopyTo(bytes, 0);

            //todo error here
            var placeholder = new Color();
            //here we will use the ColorHsv Object and Image Analysis to get the color Table.
            //Example: ff ff ff 00 00 00 ff 00 00 00 ff 00
            // #0000FF -> From  HSV
            for (var i = ColorTable.Count; i < (int)Math.Pow(2.0, bytes[0] + 1); i++)
            {
                lst.Add(Color.Red);
                lst.Add(Color.Green);
                lst.Add(Color.Blue);
            }

            return lst;
        }

        /// <summary>
        ///     Writes the application extension section for the .GIF file.
        /// </summary>
        /// <param name="shouldRepeat">if set to <c>true</c> [should repeat].</param>
        /// <returns></returns>
        internal static List<byte> WriteApplicationExtension(bool shouldRepeat)
        {
            // This section allows us to animate our .GIF file.
            // Without this section, our .GIF would play through
            // once and stop.
            //
            // The first byte is the extension code, this "extension" code
            //      (extension codes are specific to the 89a .GIF specification, as opposed to the 87a specification)
            // The second byte says this extension is an application extension "0xff"
            // The third byte is the length of the application extension (fixed value, "0x0b")
            // The next 8 bytes are "NETSCAPE" - this is the application identifier
            // The next 3 bytes are "2.0" - this is the application authentication code
            // The next byte is a "3" - the length of the bytes to follow
            // The next byte is a "1"
            // The next 2 bytes are how many times the .GIF file should loop in the animation
            // The last byte is a termination character

            var lst = new List<byte>
            {
                0x21, // GIF extension code
                0xff, // Application extension label
                0x0b, // Length of application block (to follow)
                (byte)'N',
                (byte)'E',
                (byte)'T',
                (byte)'S',
                (byte)'C',
                (byte)'A',
                (byte)'P',
                (byte)'E',
                (byte)'2',
                (byte)'.',
                (byte)'0',
                3, // Length of data sub-block (3 bytes of data to follow)
                1
            };

            // GIF extension code
            // Application extension label
            // Length of application block (to follow)

            // Length of data sub-block (3 bytes of data to follow)

            if (shouldRepeat)
            {
                lst.Add(0xff); // Times the GIF will loop
                lst.Add(0xff);
            }
            else
            {
                lst.Add(0);
                lst.Add(0);
            }

            lst.Add(0); // Data sub-block terminator

            return lst;
        }

        /// <summary>
        ///     Writes the graphic control extension section for the .GIF file.
        ///     This section is optional for each frame of your .GIF file if you
        ///     are not animating your .GIF file with multiple frames.
        /// </summary>
        /// <param name="delay">The delay.</param>
        /// <returns></returns>
        internal static List<byte> WriteGraphicControlExtension(ushort delay)
        {
            // The graphic control extension allows us to control
            // aspects related to our .GIF's animation.
            //
            // The first byte is the extension code, this "extension" code
            // The second byte says this extension is a graphics control extension "0xf9"
            // The third byte is the byte size (this should stay at 4)
            // The fourth byte is a packed byte:
            //      The first bit is if we want to make our transparent color index, transparent
            //          (the transparent color index is an index of the global color table we want to make transparent)
            //      The second bit is if we require user input before moving to the next frame in an animated .GIF
            //      The next three bits are the disposal method - which defines what happens to the current image before we move onto the next
            //          (a value of "1" indicates the image should remain in place, and draw the next image over it)
            //          (a value of "2" indicates the canvas should be restored to the background color (as indicated in the logical screen descriptor))
            //          (a value of "3" indicates the canvas should restore state to the previous state before the current image was drawn)
            //      The next three bits are reserved and not used as of today
            // The next 2 bytes are the value in centiseconds should wait before moving onto the next frame
            //      (100 centiseconds = 1 second)
            // The next byte is the transparent color index (that matches a color in the global color table)
            // The last byte is a termination character

            var lst = new List<byte>
            {
                0x21, // GIF extension code
                0xf9, // Graphic control extension label
                4 // Byte size
            };

            // Packed field
            var array = new BitArray(new byte[1])
            {
                [0] = false, // Transparent color flag
                [1] = false, // User input flag
                [4] = false, // Disposal method
                [3] = false,
                [2] = true
            };

            var bytes = new byte[1];

            array.CopyTo(bytes, 0);

            lst.AddRange(bytes);

            lst.Add((byte)delay); // Delay time, in centiseconds
            lst.Add(0); // Transparent color index
            lst.Add(0); // Block terminator

            return lst;
        }

        private static BitArray LogicalScreenDescriptorGlobalColorTableSize(int? size = null)
        {
            var total = size ?? ColorTable.Count; // Optionally pass in a size to get the GCTS

            // Global color table size is directly related to the number
            // of colors we can have in our .GIF. A .GIF can hold anywhere
            // between 2-256 colors, and has this relationship between the
            // global color table size
            //
            //      GCTS | Total colors available
            //      0      2
            //      1      4
            //      2      8
            //      3      16
            //      4      32
            //      5      64
            //      6      128
            //      7      256
            //
            // Where:
            //      total colors available = 2^(GCTS+1)
            //
            // We will perform the inverse since we know the
            // colors we want to use

            // log2(total colors) - 1
            var ret = (byte)Math.Ceiling(Math.Log(total, 2.0) - 1.0);

            return new BitArray(new[] { ret });
        }
    }
}
