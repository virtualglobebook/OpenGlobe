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

namespace MiniGlobe.Renderer
{
    // TODO:  Belongs in OpenTK namespace
    public struct Vector3i : IEquatable<Vector3i>
    {
        public Vector3i(int x, int y, int z)
            : this()
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static bool operator ==(Vector3i left, Vector3i right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector3i left, Vector3i right)
        {
            return !left.Equals(right);
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Vector3i))
                return false;

            return this.Equals((Vector3i)obj);
        }

        public bool Equals(Vector3i other)
        {
            return (X == other.X) && (Y == other.Y) && (Z == other.Z);
        }

        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
    }
}