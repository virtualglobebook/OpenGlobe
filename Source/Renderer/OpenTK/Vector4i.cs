#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using OpenTK;

namespace OpenGlobe.Renderer
{
    // TODO:  Belongs in OpenTK namespace
    public struct Vector4i : IEquatable<Vector4i>
    {
        public Vector4i(int x, int y, int z, int w)
            : this()
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public static bool operator ==(Vector4i left, Vector4i right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector4i left, Vector4i right)
        {
            return !left.Equals(right);
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode() ^ W.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Vector4i))
                return false;

            return this.Equals((Vector4i)obj);
        }

        public bool Equals(Vector4i other)
        {
            return (X == other.X) && (Y == other.Y) && (Z == other.Z) && (W == other.W);
        }

        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public int W { get; set; }
    }
}