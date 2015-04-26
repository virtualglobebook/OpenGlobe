OpenGlobe
=========

<center>
![Screen shot of Half Dome as rendered by OpenGlobe](http://www.virtualglobebook.com/halfdome.jpg)
</center>

**Note: OpenGlobe is great for learning along with our book, 3D Engine Design for Virtual Globes.  But for a production quality, open source, virtual globe by the same authors (plus more), check out [Cesium](https://cesiumjs.org)!**

OpenGlobe is a 3D engine for virtual globes (think [Google Earth](http://earth.google.com) or [NASA World Wind](http://worldwind.arc.nasa.gov)) designed to illustrate the engine design and rendering techniques described in our book, [3D Engine Design for Virtual Globes](http://www.virtualglobebook.com).  It is written in C# (with full support for running on Linux using [Mono](http://www.mono-project.com)) and uses the OpenGL 3.3 core profile via [OpenTK](http://www.opentk.com).  It is not a complete virtual globe application, but is rather a core engine and a number of runnable examples.

OpenGlobe has the following features and capabilities:

- A well designed (and pragmatic) renderer abstraction making it easier and less error prone to interface with OpenGL.
- WGS84 (and other ellipsoid) globe rendering using tessellation or GPU ray casting.
- Techniques for avoiding depth buffer errors when rendered objects are found at widely varying distances from the camera.
- High-precision vertex rendering techniques to avoid jittering problems.
- Vector data rendering, including reading vector data from shapefiles.
- Multithreaded resource preparation.
- Terrain patch rendering using CPU triangulation, GPU displacement mapping, and GPU ray casting.
- Terrain shading using procedural techniques.
- Whole-world terrain and imagery rendering on an accurate WGS84 globe using geometry clipmapping.

The code (and the book) draw from the authors' real-world experience working on [STK](http://www.agi.com), one of the earliest commercial virtual globes.  Since it is intended as a learning aid, the code is written in a style that we hope is easy to understand and follow, especially when accompanied by the book.

For information on getting up and running with the code on both Windows and Linux, see the instructions [here](http://www.virtualglobebook.com/code.html).

