#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;

namespace OpenGlobe.Renderer
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct BlittableBGRA : IEquatable<BlittableBGRA>
    {
        public byte B { get; set; }
        public byte G { get; set; }
        public byte R { get; set; }
        public byte A { get; set; }

        public static readonly ImageFormat Format = ImageFormat.BlueGreenRedAlpha;
        public static readonly ImageDatatype Datatype = ImageDatatype.UnsignedByte;

        public BlittableBGRA(Color color)
            : this()
        {
            B = color.B;
            G = color.G;
            R = color.R;
            A = color.A;
        }

        public static bool operator ==(BlittableBGRA left, BlittableBGRA right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BlittableBGRA left, BlittableBGRA right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "Blue: {0} Green: {1} Red: {2} Alpha: {3}", B, G, R, A);
        }

        public override int GetHashCode()
        {
            return B.GetHashCode() ^ G.GetHashCode() ^ R.GetHashCode() ^ A.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is BlittableBGRA))
                return false;

            return this.Equals((BlittableBGRA)obj);
        }

        #region IEquatable<BlittableBGRA> Members

        public bool Equals(BlittableBGRA other)
        {
            return
                (B == other.B) &&
                (G == other.G) &&
                (R == other.R) &&
                (A == other.A);
        }

        #endregion
    }
}
