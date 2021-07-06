using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace The_Viewer
{
    partial class Form1
    {
        #region Classwide fields
        // Toggle limiter for key hold-down signaling rate.
        private static bool inputsAllowed = false;
        // Max/restore window.
        private FormState formState = new FormState();
        private bool isAutoPlaying = false;
        private bool isMaximized = false;
        private bool isStartingUp = true;
        private List<Tuple<PictureBox, bool, bool>> pictureBoxes = new List<Tuple<PictureBox, bool, bool>>();

        private struct PictureBoxManager
        {
            public static List<PictureBox> pictureBoxes = new List<PictureBox>();
            public static List<bool> pictureBoxIsLoading = new List<bool>();
            public static bool preloadNextImage = false;
        }
        #endregion

        /// <summary>
        /// Setup. Sets up hooks, screen size, check boxes.
        /// </summary>
        private void LoadTheViewer()
        {
            // Set screen resolution
            ViewerDimensions.SetRegularSizes(Screen.PrimaryScreen.WorkingArea.Size);
            this.MaximumSize = Screen.PrimaryScreen.WorkingArea.Size;
            this.Size = Screen.PrimaryScreen.WorkingArea.Size;

            Console.WriteLine(Screen.PrimaryScreen.WorkingArea.Size);

            // Setup the PBM
            PictureBoxManager.pictureBoxes.Add(pictureBoxOne);
            PictureBoxManager.pictureBoxes.Add(pictureBoxTwo);
            PictureBoxManager.pictureBoxIsLoading.Add(false);
            PictureBoxManager.pictureBoxIsLoading.Add(false);

            // Check if opened with file association. Check file, set path and index.
            if (Environment.GetCommandLineArgs().Length > 1 && Environment.GetCommandLineArgs() != null && File.Exists(Environment.GetCommandLineArgs()[1]))
            {
                // Make sure it's actually a picture file.
                //if (args != null && args.Length > 0 && (Path.GetExtension(args[0]) == ".jpg" || Path.GetExtension(args[0]) == ".png" || Path.GetExtension(args[0]) == ".bmp"))
                ImageOperations.SetWorkingDir(Path.GetDirectoryName(Environment.GetCommandLineArgs()[1]));
                ImageOperations.SetIndexFromPathOrdered(Environment.GetCommandLineArgs()[1]);

                PictureBoxManager.pictureBoxes[0].Image = null;
                GetCurrentImage();

                GC.Collect();
            }
            // Otherwise, open in the default way.
            else
            {
                ImageOperations.SetWorkingDir(Directory.GetCurrentDirectory());

                PictureBoxManager.pictureBoxes[0].Image = null;
                GetCurrentImage();
            }

            // Sets up check boxes, enablers, etc.
            inputsAllowed = true;

            timerToggleButton.BackColor = Color.FromArgb(128, 0, 0);
            timerSelectionBox.SelectedIndex = 2;
        }

        /// <summary>
        /// Checks timers and whether or not inputs are allowed.
        /// If everything is ok, get the next image and update the main viewer box.
        /// </summary>
        private void GetNextPicture()
        {
            if (inputsAllowed)
            {
                repeatInputTimer.Enabled = false;
                inputsAllowed = false;

                if (autoplayTimer.Enabled)
                    autoplayTimer.Enabled = false;
                
                ImageOperations.IncrementCurrentImageIndex(randomizedImage: randomOrderCheckBox.Checked);

                // Check to see if we have a preload available.
                if(!PictureBoxManager.pictureBoxIsLoading[1] && PictureBoxManager.pictureBoxes[1].ImageLocation == ImageOperations.GetCurrentImagePath(randomizedImage: randomOrderCheckBox.Checked))
                {
                    // If we have a preload ready, use it instead.
                    PictureBoxManager.pictureBoxes[0].Image = PictureBoxManager.pictureBoxes[1].Image;
                    PictureBoxManager.pictureBoxes[0].ImageLocation = PictureBoxManager.pictureBoxes[1].ImageLocation;

                    // After setting that, preload the next image.
                    PictureBoxManager.pictureBoxIsLoading[1] = true;
                    PictureBoxManager.pictureBoxes[1].LoadAsync(ImageOperations.GetNextImagePath(randomizedImage: randomOrderCheckBox.Checked));
                }
                else
                {
                    // Cancel any loading operations in progress.
                    PictureBoxManager.pictureBoxes[0].CancelAsync();
                    PictureBoxManager.pictureBoxes[1].CancelAsync();

                    // Otherwise, cancel any image loading in progress and reload images for both picture boxes.
                    PictureBoxManager.pictureBoxes[0].LoadAsync(ImageOperations.GetCurrentImagePath(randomizedImage: randomOrderCheckBox.Checked));
                    // Set flag for preloading of a new picture.
                    PictureBoxManager.preloadNextImage = true;
                    Console.WriteLine("Manually loading.");
                }
            }
        }

        /// <summary>
        /// Checks timers and whether or not inputs are allowed.
        /// If everything is ok, get the next image and update the main viewer box.
        /// </summary>
        private void GetPreviousPicture()
        {
            if (inputsAllowed)
            {
                repeatInputTimer.Enabled = false;
                inputsAllowed = false;

                ImageOperations.DecrementCurrentImageIndex(randomizedImage: randomOrderCheckBox.Checked);

                // Check to see if we can use the current image in pictureboxtwo as a buffer.
                if (!PictureBoxManager.pictureBoxIsLoading[0] && PictureBoxManager.pictureBoxes[0].ImageLocation == ImageOperations.GetCurrentImagePath(randomizedImage: randomOrderCheckBox.Checked))
                {
                    PictureBoxManager.pictureBoxes[1].Image = PictureBoxManager.pictureBoxes[0].Image;
                    PictureBoxManager.pictureBoxes[1].ImageLocation = PictureBoxManager.pictureBoxes[0].ImageLocation;
                }
                else
                {
                    // Cancel any loading operations in progress.
                    PictureBoxManager.pictureBoxes[0].CancelAsync();
                    PictureBoxManager.pictureBoxes[1].CancelAsync();

                    PictureBoxManager.pictureBoxes[0].LoadAsync(ImageOperations.GetCurrentImagePath(randomizedImage: randomOrderCheckBox.Checked));
                    PictureBoxManager.preloadNextImage = true;

                    Console.WriteLine("Manually loading and then preloading.");
                }
            }
        }

        private void GetCurrentImage()
        {
            PictureBoxManager.pictureBoxes[0].LoadAsync(ImageOperations.GetCurrentImagePath(randomizedImage: randomOrderCheckBox.Checked));
            if (isStartingUp)
            {
                PictureBoxManager.preloadNextImage = true;
                Console.WriteLine("Initiating startup preload.");
                isStartingUp = !isStartingUp;
            }
        }

        private void mainViewerBox_LoadCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            GC.Collect();

            workingPathDisplay.Text = ImageOperations.GetCurrentImagePath(randomizedImage: randomOrderCheckBox.Checked);

            if (isAutoPlaying)
                autoplayTimer.Enabled = true;

            repeatInputTimer.Enabled = true;
        }

        /// <summary>
        /// Listens for timer button click.
        /// Calls the ToggleTimer method to engage/disengage the timer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timerToggleButton_Click(object sender, EventArgs e)
        {
            ToggleTimer();
        }

        /// <summary>
        /// Engages/disengages the slideshow timer.
        /// Changes the button's color accordingly.
        /// </summary>
        private void ToggleTimer()
        {
            if (!autoplayTimer.Enabled)
            {
                timerToggleButton.Text = "Timer: On";
                timerToggleButton.BackColor = Color.FromArgb(0, 128, 0);
                isAutoPlaying = true;
                autoplayTimer.Enabled = true;
            }
            else
            {
                timerToggleButton.Text = "Timer: Off";
                timerToggleButton.BackColor = Color.FromArgb(128, 0, 0);
                isAutoPlaying = false;
                autoplayTimer.Enabled = false;
            }
        }

        /// <summary>
        /// Listens for timer speed change.
        /// Changes the timers tick duration based on the option selected in the combobox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timerSelectionBox_SelectionChangeCommitted(object sender, EventArgs e)
        {
            int baseInterval = 1000;
            switch (timerSelectionBox.SelectedIndex)
            {
                case 0:
                    autoplayTimer.Interval = baseInterval * 1;
                    break;
                case 1:
                    autoplayTimer.Interval = baseInterval * 2;
                    break;
                case 2:
                    autoplayTimer.Interval = baseInterval * 3;
                    break;
                case 3:
                    autoplayTimer.Interval = baseInterval * 5;
                    break;
                case 4:
                    autoplayTimer.Interval = baseInterval * 10;
                    break;
                case 5:
                    autoplayTimer.Interval = baseInterval * 15;
                    break;
                case 6:
                    autoplayTimer.Interval = baseInterval * 20;
                    break;
                case 7:
                    autoplayTimer.Interval = baseInterval * 25;
                    break;
                case 8:
                    autoplayTimer.Interval = baseInterval * 30;
                    break;
                case 9:
                    autoplayTimer.Interval = baseInterval * 40;
                    break;
                case 10:
                    autoplayTimer.Interval = baseInterval * 50;
                    break;
                case 11:
                    autoplayTimer.Interval = baseInterval * 60;
                    break;
                default:
                    autoplayTimer.Interval = baseInterval * 3;
                    break;
            }
        }

        /// <summary>
        /// Listens for input.
        /// Limits the amount of image switching via bool usage.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void repeatInputTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            inputsAllowed = true;
        }

        /// <summary>
        /// Listens for click, then gets the next picture.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void nextPictureButton_Click(object sender, EventArgs e)
        {
            GetNextPicture();
        }

        /// <summary>
        /// Listens for click, then gets the previous picture.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void previousPictureButton_Click(object sender, EventArgs e)
        {
            GetPreviousPicture();
        }

        /// <summary>
        /// Listens for timer tick.
        /// Gets the next picture.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void autoplayTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            GetNextPicture();
        }

        /// <summary>
        /// CheckBox event for toggling randomized picture ordering.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void randomOrderCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if(randomOrderCheckBox.Checked)
            {
                ImageOperations.InitRandomizedImages();
                GetCurrentImage();
            }
            if (!randomOrderCheckBox.Checked)
            {
                ImageOperations.StopRandomizedImages();
                GetCurrentImage();
            }
        }

        /// <summary>
        /// Listens for working directory browsing button click.
        /// Opens dialog to find a new browsing folder.
        /// Checks if folder exists, calls ImageStuff to reset and reseed image lists.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void browseForWorkingDirButton_Click(object sender, EventArgs e)
        {
            DialogResult result = selectFolderDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                ImageOperations.SetWorkingDir(selectFolderDialog.SelectedPath);

                PictureBoxManager.pictureBoxes[0].Image = null;
                GetCurrentImage();
            }
        }

        private void pictureBoxOne_LoadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            GC.Collect();

            workingPathDisplay.Text = ImageOperations.GetCurrentImagePath(randomizedImage: randomOrderCheckBox.Checked);

            if (isAutoPlaying)
                autoplayTimer.Enabled = true;

            repeatInputTimer.Enabled = true;

            PictureBoxManager.pictureBoxIsLoading[0] = false;
            Console.WriteLine("Primary image loaded.");

            // If the flag has been set to preload the next image, do it here.
            if (PictureBoxManager.preloadNextImage)
            {
                PictureBoxManager.pictureBoxIsLoading[1] = true;
                PictureBoxManager.pictureBoxes[1].LoadAsync(ImageOperations.GetNextImagePath(randomizedImage: randomOrderCheckBox.Checked));

                Console.WriteLine("Initiating preload.");
            }
        }

        private void pictureBoxTwo_LoadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            PictureBoxManager.pictureBoxIsLoading[1] = false;
            Console.WriteLine("Secondary image loaded.");

            if (PictureBoxManager.preloadNextImage)
            {
                PictureBoxManager.preloadNextImage = false;
                Console.WriteLine("Preload done.");
            }
        }

        /// <summary>
        /// Listens for and captures key presses.
        /// Valid key and key comboes result in actions and nullify the events to other methods.
        /// Invalid key and key comboes result in the release of the event to other methods.
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="keyData"></param>
        /// <returns>Boolean for used/unused key press</returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Left:
                case Keys.A:
                    {
                        GetPreviousPicture();
                        return true;
                    }
                case Keys.Right:
                case Keys.D:
                    {
                        GetNextPicture();
                        return true;
                    }
                case Keys.Up:
                    {
                        if (timerSelectionBox.SelectedIndex > 0)
                        {
                            timerSelectionBox.SelectedIndex--;
                            timerSelectionBox_SelectionChangeCommitted(this, null);
                        }
                        return true;
                    }
                case Keys.Down:
                    {
                        if (timerSelectionBox.SelectedIndex < timerSelectionBox.Items.Count - 1)
                        {
                            timerSelectionBox.SelectedIndex++;
                            timerSelectionBox_SelectionChangeCommitted(this, null);
                        }
                        return true;
                    }
                case Keys.Space:
                    {
                        ToggleTimer();
                        return true;
                    }
                case Keys.Escape:
                    {
                        WindowState = FormWindowState.Minimized;
                        return true;
                    }
                case Keys.F:
                case Keys.F11:
                    {
                        ToggleFullscreen();
                        return true;
                    }
                default:
                    {
                        return false;
                    }
            }
        }

        private void Form1_MouseWheel(object sender, MouseEventArgs e)
        {
            if(e.Delta > 0)
            {
                // Scrolled up.
                if (timerSelectionBox.SelectedIndex > 0)
                {
                    timerSelectionBox.SelectedIndex--;
                    timerSelectionBox_SelectionChangeCommitted(this, null);
                }
            }
            else
            {
                // Scrolled down.
                if (timerSelectionBox.SelectedIndex < timerSelectionBox.Items.Count - 1)
                {
                    timerSelectionBox.SelectedIndex++;
                    timerSelectionBox_SelectionChangeCommitted(this, null);
                }
            }
        }

        private void mainViewerBox_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                ToggleFullscreen();
            }

            if (e.Button == MouseButtons.Middle)
            {
                ToggleTimer();
            }
        }

        /// <summary>
        /// Toggles fullscreen mode. Sets/resets element/window sizes, positions, and image.
        /// </summary>
        private void ToggleFullscreen()
        {
            if (!isMaximized)
            {
                viewerControlsPanel.Visible = false;
                viewerControlsPanel.BringToFront();
                mainViewerPanel.AutoScroll = true;

                mainViewerPanel.Size = new Size(ViewerDimensions.mainWidth, ViewerDimensions.mainHeight);
                this.MaximumSize = Screen.PrimaryScreen.Bounds.Size;
                ViewerDimensions.SetFullScreenSizes(Screen.PrimaryScreen.Bounds.Size);
                GetCurrentImage();

                Cursor.Hide();

                formState.Maximize(this);
                isMaximized = true;
            }
            else
            {
                formState.Restore(this);

                viewerControlsPanel.Visible = true;
                mainViewerPanel.AutoScroll = false;

                this.MaximumSize = Screen.PrimaryScreen.WorkingArea.Size;
                ViewerDimensions.SetRegularSizes(Screen.PrimaryScreen.WorkingArea.Size);
                mainViewerPanel.Size = new Size(ViewerDimensions.panelWidth, ViewerDimensions.panelHeight);
                GetCurrentImage();

                Cursor.Show();

                isMaximized = false;
            }
        }
    }
}
