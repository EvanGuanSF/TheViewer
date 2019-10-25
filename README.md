# The Viewer
<p align="center">
  <img width="256" height="256" src="https://raw.githubusercontent.com/JammyPajamies/TheViewer/dev/The%20Viewer/Viewer.png">
</p>

This is a simple _recursive_ slideshow image viewer inspired by the old built-in Windows Image Viewer from Windows XP and 7. It looks at the directory it is based in and all subdirectories for files with certain image filetypes and adds them to a list for playback. Created using WinForms.
 
### Features and Notes
- Multiple selectable timer options, ranging from 3 to 30 second intervals (toggle with spacebar or button click).
- File deletion checking. The program will remove image paths it can not find from the working list and will attempt to find the next/previous image in the list.
- Fullscreen mode with (toggle with 'F' or 'F11' key).
- Quick hide viewer (activate with escape key).
- Working folder selectable at runtime.
- Image update limiter (default 250ms) to reduce potential CPU load and make the program more usable.
- Has functioning argument capabilities for opening a specific image.
 
### TODO
- Windows file association to make better use of currently CLI-only argument capability.
- Make recursive/top-level only building of image list context-aware by determining number of arguments.
