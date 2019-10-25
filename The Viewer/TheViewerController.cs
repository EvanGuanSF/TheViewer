using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace The_Viewer
{
    partial class Form1
    {
        #region Classwide fields
        // Toggle limiter for key hold-down signaling rate.
        private static bool inputsAllowed = false;
        // Max/restore window.
        private FormState formState = new FormState();
        private bool isMaximized = false;
        #endregion

        /// <summary>
        /// Setup. Sets up hooks, screen size, check boxes.
        /// </summary>
        private void LoadTheViewer()
        {
            // Set screen resolution
            ViewerDimensions.SetRegularSizes(Screen.PrimaryScreen.WorkingArea.Size);
            this.MaximumSize = Screen.PrimaryScreen.WorkingArea.Size;

            // Check if opened with file association. Check file, set path and index.
            if (Environment.GetCommandLineArgs().Length > 1 && Environment.GetCommandLineArgs() != null && File.Exists(Environment.GetCommandLineArgs()[1]))
            {
                //* Make sure it's actually a picture file.
                //if (args != null && args.Length > 0 && (Path.GetExtension(args[0]) == ".jpg" || Path.GetExtension(args[0]) == ".png" || Path.GetExtension(args[0]) == ".bmp"))
                ImageOperations.SetWorkingDir(Path.GetDirectoryName(Environment.GetCommandLineArgs()[1]));
                ImageOperations.PathToCurrentIndex(Environment.GetCommandLineArgs()[1]);

                mainViewerBox.Image = null;
                mainViewerBox.Image = ImageOperations.GetCurrentImage();
                workingPathDisplay.Text = ImageOperations.GetCurrentImagePath();

                GC.Collect();
            }
            // Otherwise, open in the default way.
            else
            {
                ImageOperations.SetWorkingDir(Directory.GetCurrentDirectory());

                mainViewerBox.Image = null;
                mainViewerBox.Image = ImageOperations.GetCurrentImage();
                workingPathDisplay.Text = ImageOperations.GetCurrentImagePath();
            }

            // Sets up check boxes, enablers, etc.
            inputsAllowed = true;

            timerToggleButton.BackColor = Color.FromArgb(128, 0, 0);
            timerSelectionBox.SelectedIndex = 1;
        }

        /// <summary>
        /// Checks timers and whether or not inputs are allowed.
        /// If everything is ok, get the next image and update the main viewer box.
        /// </summary>
        private void GetNextPicture()
        {
            if (inputsAllowed)
            {
                bool thatTimerDetector = false;

                if (autoplayTimer.Enabled)
                {
                    thatTimerDetector = true;
                    autoplayTimer.Enabled = false;
                }
                if (thatTimerDetector)
                    autoplayTimer.Enabled = true;


                inputsAllowed = false;
                mainViewerBox.Image = null;
                mainViewerBox.Image = ImageOperations.GetNextImage();
                workingPathDisplay.Text = ImageOperations.GetCurrentImagePath();

                GC.Collect();

                if (thatTimerDetector)
                    autoplayTimer.Enabled = true;
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
                bool thatTimerDetector = false;

                if (autoplayTimer.Enabled)
                {
                    thatTimerDetector = true;
                    autoplayTimer.Enabled = false;
                }
                if (thatTimerDetector)
                    autoplayTimer.Enabled = true;

                inputsAllowed = false;
                mainViewerBox.Image = null;
                mainViewerBox.Image = ImageOperations.GetPreviousImage();
                workingPathDisplay.Text = ImageOperations.GetCurrentImagePath();

                GC.Collect();

                if (thatTimerDetector)
                    autoplayTimer.Enabled = true;
            }
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
                autoplayTimer.Enabled = true;
            }
            else
            {
                timerToggleButton.Text = "Timer: Off";
                timerToggleButton.BackColor = Color.FromArgb(128, 0, 0);
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
                    autoplayTimer.Interval = baseInterval * 2;
                    break;
                case 1:
                    autoplayTimer.Interval = baseInterval * 3;
                    break;
                case 2:
                    autoplayTimer.Interval = baseInterval * 5;
                    break;
                case 3:
                    autoplayTimer.Interval = baseInterval * 8;
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
            if (!inputsAllowed)
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
                GetNextPicture();
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
                mainViewerBox.Image = ImageOperations.GetCurrentImage();

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
                mainViewerBox.Image = ImageOperations.GetCurrentImage();

                Cursor.Show();

                isMaximized = false;
            }
        }
    }
}
