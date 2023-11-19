# TemplateMatching
Visual Studio 2015 projet to find template image in source image. Uses [EmguCV](https://github.com/emgucv/emgucv) (OpenCV .Net wrapper) 4.1.1, 64bit dlls.

In this example, you first capture frames (using my [ScreenCapture](https://github.com/samilkorkmaz/ScreenCapture) repo) of [floppy bird](https://github.com/samilkorkmaz/floppybird) game and save them. Set Form1.cs, sourceFileName according to your saved frames folder and file names. The program tries to find matches of images in Resources folder in the frames you captured and saved. 
