using System.Drawing;

namespace The_Viewer
{
    /// <summary>
    /// Houses a struct that holds the width and height of the form and panels.
    /// </summary>
    public static class ViewerDimensions
    {
        #region Control size fields
        public static int mainWidth;
        public static int mainHeight;
        public static int panelWidth;
        public static int panelHeight;
        #endregion

        /// <summary>
        /// Programmatically sets width and height of "windowed" mode panels and window.
        /// </summary>
        /// <param name="curScreenSize"></param>
        public static void SetRegularSizes(Size curScreenSize)
        {
            mainWidth = curScreenSize.Width;
            mainHeight = curScreenSize.Height;

            panelWidth = curScreenSize.Width;
            panelHeight = curScreenSize.Height - 24;
        }

        /// <summary>
        /// Programmatically sets width and height of "fullscreen" mode panels and window.
        /// </summary>
        /// <param name="curScreenSize"></param>
        public static void SetFullScreenSizes(Size curScreenSize)
        {
            mainWidth = curScreenSize.Width;
            mainHeight = curScreenSize.Height;

            panelWidth = curScreenSize.Width;
            panelHeight = curScreenSize.Height;
        }
    }
}
