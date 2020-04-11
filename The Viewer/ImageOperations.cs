using System;
using System.Collections.Generic;
using System.Drawing;
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
            if (Images.filePaths.Count > 0 && File.Exists(Images.filePaths[Images.curFilePathIndex]))
            {
                Image tempImage = Image.FromFile(Images.filePaths[Images.curFilePathIndex]);
                return tempImage;
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
            return tempImage;
        }

        /// <summary>
        /// Returns a list of images with valid extensions in a directory.
        /// </summary>
        /// <param name="directory"></param>
        /// <returns>List of paths</returns>
        private static List<string> GetAllImages(string directory)
        {
            List<string> newListOfImagePaths = new List<string>(); ;
            if (!Directory.Exists(directory))
            {
                return newListOfImagePaths;
            }

            var listOfImagePaths = Directory.EnumerateFiles(directory, "*.*", SearchOption.AllDirectories)
            .Where(s => s.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                s.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                s.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase) ||
                s.EndsWith(".gif", StringComparison.OrdinalIgnoreCase));

            int count = listOfImagePaths.Count();
            string[] arrayOfImagePaths;

            if (count > 0)
            {
                arrayOfImagePaths = new string[count - 1];
                arrayOfImagePaths = listOfImagePaths.ToArray();
            }
            else
            {
                arrayOfImagePaths = new string[0];
            }

            Array.Sort(arrayOfImagePaths, new AlphanumComparatorFast());
            newListOfImagePaths = arrayOfImagePaths.ToList();
            return newListOfImagePaths;
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
