#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System.Drawing;

namespace OpenGlobe.Scene
{
    public class ShapefileAppearance
    {
        public ShapefileAppearance()
	    {
            PolylineColor = Color.Yellow;
            PolylineOutlineColor = Color.Black;
            PolylineWidth = 3.0;
            PolylineOutlineWidth = 2.0;
	    }

        public Bitmap Bitmap { get; set; }
        public Color PolylineColor { get; set; }
        public Color PolylineOutlineColor { get; set; }
        public double PolylineWidth { get; set; }
        public double PolylineOutlineWidth { get; set; }
    }
}
