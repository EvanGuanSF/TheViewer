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
        /// Holds ordered image file paths in a list and the working index of said list.
        /// Sorted alphanumerically.
        /// </summary>
        private struct OrderedImages
        {
            public static int curFilePathIndex = 0;
            public static List<string> filePaths;
        }

        /// <summary>
        /// Holds randomized image file paths in a list and the working index of said list.
        /// Randomized.
        /// </summary>
        private struct RandomizedImages
        {
            public static int curFilePathIndex = 0;
            public static List<string> filePaths;
        }

        /// <summary>
        /// Gets next image from the appropriate Images struct.
        /// </summary>
        public static void GetNextImage(bool randomizedImage = false)
        {
            // Try to return the next valid image in the list.
            bool imageFound = false;
            if(!randomizedImage)
            {
                while (!imageFound && OrderedImages.filePaths.Count > 0)
                {
                    // Calculate the new index.
                    if (OrderedImages.filePaths.Count > 0 && (OrderedImages.curFilePathIndex + 1) < (OrderedImages.filePaths.Count))
                    {
                        // Not at last index and we have more items in the list. 
                        OrderedImages.curFilePathIndex++;
                    }
                    else
                    {
                        // At last index or we have no more items in the list, set working index to 0.
                        OrderedImages.curFilePathIndex = 0;
                    }

                    imageFound = CheckFileExists();
                }
            } else if (randomizedImage)
            {
                while (!imageFound && RandomizedImages.filePaths.Count > 0)
                {
                    // Calculate the new index.
                    if (RandomizedImages.filePaths.Count > 0 && (RandomizedImages.curFilePathIndex + 1) < (RandomizedImages.filePaths.Count))
                    {
                        // Not at last index and we have more items in the list. 
                        RandomizedImages.curFilePathIndex++;
                    }
                    else
                    {
                        // At last index or we have no more items in the list, set working index to 0.
                        RandomizedImages.curFilePathIndex = 0;
                    }

                    imageFound = CheckFileExists(randomizedImage: true);
                }
            }
        }

        /// <summary>
        /// Gets previous image path from the appropriate Images struct.
        /// </summary>
        public static void GetPreviousImage(bool randomizedImage = false)
        {
            // Try to return the previous valid image in the list.
            bool imageFound = false;
            if (!randomizedImage)
            {
                while (!imageFound && OrderedImages.filePaths.Count > 0)
                {
                    // Calculate the new index.
                    if (OrderedImages.filePaths.Count > 0 && (OrderedImages.curFilePathIndex - 1) >= 0)
                    {
                        // Not at index 0 and we have more items in the list. 
                        OrderedImages.curFilePathIndex--;
                    }
                    else
                    {
                        // At last index or we have no more items in the list, set working index to the max index.
                        OrderedImages.curFilePathIndex = OrderedImages.filePaths.Count - 1;
                    }

                    imageFound = CheckFileExists();
                }
            }
            else if (randomizedImage)
            {
                while (!imageFound && RandomizedImages.filePaths.Count > 0)
                {
                    // Calculate the new index.
                    if (RandomizedImages.filePaths.Count > 0 && (RandomizedImages.curFilePathIndex - 1) >= 0)
                    {
                        // Not at index 0 and we have more items in the list. 
                        RandomizedImages.curFilePathIndex--;
                    }
                    else
                    {
                        // At last index or we have no more items in the list, set working index to the max index.
                        RandomizedImages.curFilePathIndex = RandomizedImages.filePaths.Count - 1;
                    }

                    imageFound = CheckFileExists(randomizedImage: true) ;
                }
            }
        }

        /// <summary>
        /// Checks to see if the file at the current filePaths index exists.
        /// </summary>
        /// <returns>bool representing the existence of a valid file at the current filePaths index.</returns>
        private static bool CheckFileExists(bool randomizedImage = false)
        {
            if(!randomizedImage)
            {
                // Check the path at the new index to see if it exists.
                if (OrderedImages.curFilePathIndex >= 0 && OrderedImages.filePaths[OrderedImages.curFilePathIndex].Length >= 3 && File.Exists(OrderedImages.filePaths[OrderedImages.curFilePathIndex]))
                {
                    // If the file given the path is ok/exists, then break out of the loop and handle it.
                    return true;
                }

                if (OrderedImages.filePaths.Count > 0)
                {
                    // If it does not, remove the path at the index and try again for the next index by repeating the while loop.
                    OrderedImages.filePaths.RemoveAt(OrderedImages.curFilePathIndex);
                    return false;
                }
            } else if (randomizedImage)
            {
                // Check the path at the new index to see if it exists.
                if (RandomizedImages.curFilePathIndex >= 0 && RandomizedImages.filePaths[RandomizedImages.curFilePathIndex].Length >= 3 && File.Exists(RandomizedImages.filePaths[RandomizedImages.curFilePathIndex]))
                {
                    // If the file given the path is ok/exists, then break out of the loop and handle it.
                    return true;
                }

                if (RandomizedImages.filePaths.Count > 0)
                {
                    // If it does not, remove the path at the index and try again for the next index by repeating the while loop.
                    RandomizedImages.filePaths.RemoveAt(RandomizedImages.curFilePathIndex);
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Get current image via Images.curFilePathIndex.
        /// </summary>
        /// <returns>Image at current working index</returns>
        public static Image GetCurrentImage(bool randomizedImage = false)
        {
            if(!randomizedImage)
            {
                if (OrderedImages.filePaths.Count > 0 && File.Exists(OrderedImages.filePaths[OrderedImages.curFilePathIndex]))
                {
                    Image tempImage = Image.FromFile(OrderedImages.filePaths[OrderedImages.curFilePathIndex]);
                    return tempImage;
                }
                else
                {
                    return new Bitmap(64, 64, PixelFormat.Format32bppRgb);
                }
            } else
            {
                if (RandomizedImages.filePaths.Count > 0 && File.Exists(RandomizedImages.filePaths[RandomizedImages.curFilePathIndex]))
                {
                    Image tempImage = Image.FromFile(RandomizedImages.filePaths[RandomizedImages.curFilePathIndex]);
                    return tempImage;
                }
                else
                {
                    return new Bitmap(64, 64, PixelFormat.Format32bppRgb);
                }
            }
        }

        /// <summary>
        /// Populate RandomImages with randomly ordered paths from OrderedImages.
        /// </summary>
        public static void InitRandomizedImages()
        {
            // Make sure we have cleared the RandomizedImages filepaths list.
            RandomizedImages.filePaths = new List<string>();
            RandomizedImages.filePaths.Clear();
            RandomizedImages.curFilePathIndex = 0;

            // Copy the contents of OrderedImages into RandomizedImages.
            for (int i = 0; i < OrderedImages.filePaths.Count; i++)
            {
                RandomizedImages.filePaths.Add(OrderedImages.filePaths[i]);
            }

            // Quick and dirty shuffle via guid.
            RandomizedImages.filePaths = RandomizedImages.filePaths.OrderBy(a => Guid.NewGuid()).ToList();

            // Display file paths.
            //for (int i = 0; i < OrderedImages.filePaths.Count; i++)
            //{
            //    Console.WriteLine(RandomizedImages.filePaths[i]);
            //}
        }

        public static void StopRandomizedImages()
        {
            // Set the current random path to be the current path in the ordered paths.
            if(RandomizedImages.filePaths.Count > 0)
                SetIndexFromPathOrdered(RandomizedImages.filePaths[RandomizedImages.curFilePathIndex]);

            // Make sure we have cleared the RandomizedImages filepaths list.
            RandomizedImages.filePaths = new List<string>();
            RandomizedImages.filePaths.Clear();
            RandomizedImages.curFilePathIndex = 0;
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
            OrderedImages.filePaths = new List<string>();
            OrderedImages.filePaths.Clear();
            OrderedImages.curFilePathIndex = 0;
            OrderedImages.filePaths = GetAllImages(dir);
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
        public static void SetIndexFromPathOrdered(string file)
        {
            for (int i = 0; i < OrderedImages.filePaths.Count; i++)
            {
                // Case sensitive search.
                if (OrderedImages.filePaths[i] == file)
                {
                    OrderedImages.curFilePathIndex = i;

                    break;
                }
            }
        }

        // Get path of current list index.
        public static string GetCurrentIndexPath(bool randomizedImage = false)
        {
            if(!randomizedImage)
                return Convert.ToString(OrderedImages.filePaths[OrderedImages.curFilePathIndex]);
            else
                return Convert.ToString(RandomizedImages.filePaths[RandomizedImages.curFilePathIndex]);
        }

        // Get current image path.
        public static string GetCurrentImagePath(bool randomizedImage = false)
        {
            if(!randomizedImage)
            {
                if (OrderedImages.filePaths.Count == 0)
                {
                    return @"No images in selected folder.";
                }
                else
                    return Convert.ToString(OrderedImages.filePaths[OrderedImages.curFilePathIndex]);
            } else
            {
                if (RandomizedImages.filePaths.Count == 0)
                {
                    return @"No images in selected folder.";
                }
                else
                    return Convert.ToString(RandomizedImages.filePaths[RandomizedImages.curFilePathIndex]);
            }
        }

        #endregion
    }
}
