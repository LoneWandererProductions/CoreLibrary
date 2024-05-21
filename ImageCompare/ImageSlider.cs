using System.Drawing;
using Imaging;

namespace ImageCompare
{
    internal static class ImageSlider
    {
        internal static bool IsPartOf(Bitmap bigImage, Bitmap smallImage)
        {
            int bigHeight = bigImage.Height;
            int bigWidth = bigImage.Width;
            int smallHeight = smallImage.Height;
            int smallWidth = smallImage.Width;

            var dbmBig = new DirectBitmap(bigImage);
            var dbmSmall = new DirectBitmap(smallImage);

            var smallImageBottomEdge = new DirectBitmap(smallWidth, 1);

            for (int x = 0; x < smallWidth; x++)
            {
                smallImageBottomEdge.SetPixel(x, 0, dbmSmall.GetPixel(x, smallHeight - 1));
            }

            for (int i = 0; i <= bigHeight - smallHeight; i++)
            {
                for (int j = 0; j <= bigWidth - smallWidth; j++)
                {
                    if (CheckEdges(dbmBig, dbmSmall, i, j, smallImageBottomEdge) && CheckFull(dbmBig, dbmSmall, i, j))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool CheckEdges(DirectBitmap bigImage, DirectBitmap smallImage, int startRow, int startCol, DirectBitmap smallImageBottomEdge)
        {
            int smallHeight = smallImage.Height;
            int smallWidth = smallImage.Width;

            // Check top edge
            for (int x = 0; x < smallWidth; x++)
            {
                if (bigImage.GetPixel(startCol + x, startRow) != smallImage.GetPixel(x, 0))
                {
                    return false;
                }
            }

            // Check bottom edge
            for (int x = 0; x < smallWidth; x++)
            {
                if (bigImage.GetPixel(startCol + x, startRow + smallHeight - 1) != smallImageBottomEdge.GetPixel(x, 0))
                {
                    return false;
                }
            }

            // Check left edge
            for (int y = 0; y < smallHeight; y++)
            {
                if (bigImage.GetPixel(startCol, startRow + y) != smallImage.GetPixel(0, y))
                {
                    return false;
                }
            }

            // Check right edge
            for (int y = 0; y < smallHeight; y++)
            {
                if (bigImage.GetPixel(startCol + smallWidth - 1, startRow + y) != smallImage.GetPixel(smallWidth - 1, y))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool CheckFull(DirectBitmap bigImage, DirectBitmap smallImage, int startRow, int startCol)
        {
            int smallHeight = smallImage.Height;
            int smallWidth = smallImage.Width;

            for (int y = 0; y < smallHeight; y++)
            {
                for (int x = 0; x < smallWidth; x++)
                {
                    if (bigImage.GetPixel(startCol + x, startRow + y) != smallImage.GetPixel(x, y))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
