using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace The_Viewer
{
    public static class ImageOperations
    {
        /// <summary>
        /// Holds miscellaneous fields. Options, working directory, worker fields.
        /// </summary>
        private struct EnvironmentInfo
        {
            public static string workingDirectory;
        }

        /// <summary>
        /// Holds image file paths in a list and the working index of said list.
        /// </summary>
        private struct Images
        {
            public static int curFilePathIndex = 0;
            public static List<string> filePaths;
        }

        /// <summary>
        /// Gets next image.
        /// </summary>
        /// <returns>Next image</returns>
        // Returns image given options set.
        public static Image GetNextImage()
        {
            // Try to return the next valid image in the list.
            bool imageFound = false;
            while (!imageFound && Images.filePaths.Count > 0)
            {
                // Calculate the new index.
                if (Images.filePaths.Count > 0 && (Images.curFilePathIndex + 1) < (Images.filePaths.Count))
                {
                    // Not at last index and we have more items in the list. 
                    Images.curFilePathIndex++;
                }
                else
                {
                    // At last index or we have no more items in the list, set working index to 0.
                    Images.curFilePathIndex = 0;
                }

                imageFound = CheckFileExists();
            }

            // Return the current image.
            return GetCurrentImage();
        }

        /// <summary>
        /// Gets previous image path from the Images list.
        /// </summary>
        /// <returns>Previous image.</returns>
        public static Image GetPreviousImage()
        {
            // Try to return the previous valid image in the list.
            bool imageFound = false;
            int loopCount = 0;
            while (!imageFound && Images.filePaths.Count > 0)
            {
                // Calculate the new index.
                if (Images.filePaths.Count > 0 && (Images.curFilePathIndex - 1) >= 0)
                {
                    // Not at index 0 and we have more items in the list. 
                    Images.curFilePathIndex--;
                }
                else
                {
                    // At last index or we have no more items in the list, set working index to the max index.
                    Images.curFilePathIndex = Images.filePaths.Count - 1;
                }

                imageFound = CheckFileExists();

                loopCount++;
            }

            // Return the current image.
            return GetCurrentImage();
        }

        private static bool CheckFileExists()
        {
            // Check the path at the new index to see if it exists.
            if (Images.curFilePathIndex >= 0 && Images.filePaths[Images.curFilePathIndex].Length >= 3 && File.Exists(Images.filePaths[Images.curFilePathIndex]))
            {
                // If the file given the path is ok/exists, then break out of the loop and handle it.
                return true;
            }

            if (Images.filePaths.Count > 0)
            {
                // If it does not, remove the path at the index and try again for the next index by repeating the while loop.
                Images.filePaths.RemoveAt(Images.curFilePathIndex);
                return false;
            }

            return false;
        }

        /// <summary>
        /// Get current image via Images.curFilePathIndex.
        /// </summary>
        /// <returns>Image at current working index</returns>
        public static Image GetCurrentImage()
        {
            // Return...
            // ... A blank image if there are no valid images found.
            if (Images.filePaths.Count == 0)
            {
                return new Bitmap(64, 64, PixelFormat.Format32bppRgb);
            }

            // Try to set up the image.
            if (File.Exists(Images.filePaths[Images.curFilePathIndex]))
            {
                Image tempImage = Image.FromFile(Images.filePaths[Images.curFilePathIndex]);

                // ... A .gif file as-is.
                if (Convert.ToString(Images.filePaths[Images.curFilePathIndex]).EndsWith(".gif"))
                {
                    return tempImage;
                }

                // ... A resized image if a valid image is found.
                return GetResizedImage(ViewerDimensions.panelWidth, ViewerDimensions.panelHeight, tempImage);
            }
            else
            {
                return new Bitmap(64, 64, PixelFormat.Format32bppRgb);
            }
        }

        /// <summary>
        /// Gets a random image from Images.filePaths. NOT YET IMPLEMENTED.
        /// </summary>
        /// <returns>Random Image</returns>
        private static Image RandomImageFetch()
        {
            Random rand = new Random();
            int randNum = Images.curFilePathIndex = rand.Next(0, Images.filePaths.Count());

            Image tempImage = Image.FromFile(Images.filePaths[randNum]);
            return GetResizedImage(ViewerDimensions.panelWidth, ViewerDimensions.panelHeight, tempImage);
        }

        /// <summary>
        /// Returns a list of images with valid extensions in a directory.
        /// </summary>
        /// <param name="directory"></param>
        /// <returns>List of paths</returns>
        private static List<string> GetAllImages(string directory)
        {
            List<string> newList = new List<string>(); ;
            if (!Directory.Exists(directory))
            {
                return newList;
            }

            var listOfStuff = Directory.EnumerateFiles(directory, "*.*", SearchOption.AllDirectories)
            .Where(s => s.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                s.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                s.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase) ||
                s.EndsWith(".gif", StringComparison.OrdinalIgnoreCase));

            /*/ Inefficient given current implementaion.
            var listOfStuff = Directory.EnumerateFiles(directory, "*.*", SearchOption.AllDirectories)
            .Where(s => s.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                || s.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                || s.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase));
            //*/

            int count = listOfStuff.Count();
            string[] arrayOfStuff;

            if (count > 0)
            {
                arrayOfStuff = new string[count - 1];
                arrayOfStuff = listOfStuff.ToArray();
            }
            else
            {
                arrayOfStuff = new string[0];
            }

            Array.Sort(arrayOfStuff, new AlphanumComparatorFast());
            newList = arrayOfStuff.ToList();
            return newList;
        }

        /// <summary>
        /// Seeds the Locations.filePaths list with file paths.
        /// </summary>
        public static void SeedList()
        {
            string dir = EnvironmentInfo.workingDirectory;
            Images.filePaths = new List<string>();
            Images.filePaths.Clear();
            Images.curFilePathIndex = 0;
            Images.filePaths = GetAllImages(dir);
        }

        /// <summary>
        /// Assembles parts then returns a fixed aspect image within set boundaries.
        /// </summary>
        /// <param name="maxWidth"></param>
        /// <param name="maxHeight"></param>
        /// <param name="source"></param>
        /// <returns>Scaled Image</returns>
        public static Image GetResizedImage(int? maxWidth, int? maxHeight, Image source)
        {
            Size newSize = new Size();
            newSize = CalculateImageElementDimensions(maxWidth, maxHeight, source.Size);
            int width = newSize.Width;
            int height = newSize.Height;


            //return ResizedImage(width, height, source);
            // Resize the image.
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(source.HorizontalResolution, source.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(source, destRect, 0, 0, source.Width, source.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        /// <summary>
        /// Calculates and returns a bounded Size given source Size. Maintains aspect ratio.
        /// </summary>
        /// <param name="maxWidth"></param>
        /// <param name="maxHeight"></param>
        /// <param name="source"></param>
        /// <returns>Scaled Size</returns>
        private static Size CalculateImageElementDimensions(int? maxWidth, int? maxHeight, Size source)
        {
            // Check to see if the dimensions received are valid values.
            if (!maxWidth.HasValue && !maxHeight.HasValue)
                throw new ArgumentException("At least one scale factor (toWidth or toHeight) must not be null.");
            if (source.Height == 0 || source.Width == 0)
                throw new ArgumentException("Cannot scale size from zero.");

            double? widthScale = null;
            double? heightScale = null;

            // Calculate the new dimensions.
            if (maxWidth.HasValue)
            {
                widthScale = maxWidth.Value / (double)source.Width;
            }
            if (maxHeight.HasValue)
            {
                heightScale = maxHeight.Value / (double)source.Height;
            }

            double scale = Math.Min((double)(widthScale ?? heightScale),
                                     (double)(heightScale ?? widthScale));

            // Send the dimensions of the new image.
            return new Size((int)Math.Floor(source.Width * scale), (int)Math.Ceiling(source.Height * scale));
        }

        /// <summary>
        /// Small functions for getting and setting ImageStuff fields.
        /// </summary>
        /// <returns></returns>
        #region Getters and Setters
        // Get working directory.
        public static string GetWorkingDir()
        {
            return EnvironmentInfo.workingDirectory;
        }

        // Set working directory. Reseed list of files.
        public static void SetWorkingDir(string newDir)
        {
            EnvironmentInfo.workingDirectory = newDir;
            SeedList();
        }

        // Find and set the current index (for when the program is launched with a file association).
        public static void PathToCurrentIndex(string file)
        {
            for (int i = 0; i < Images.filePaths.Count; i++)
            {
                // Case sensitive search.
                if (Images.filePaths[i] == file)
                {
                    Images.curFilePathIndex = i;

                    break;
                }
            }
        }

        // Get path of current list index.
        public static string GetCurrentIndexPath()
        {
            return Convert.ToString(Images.filePaths[Images.curFilePathIndex]);
        }

        // Get current image path.
        public static string GetCurrentImagePath()
        {
            if (Images.filePaths.Count == 0)
            {
                return @"No images in selected folder.";
            }
            else
                return Convert.ToString(Images.filePaths[Images.curFilePathIndex]);
        }

        // Get index of current image.
        private static int GetCurrentImageIndex()
        {
            return Images.curFilePathIndex;
        }

        #endregion
    }
}
