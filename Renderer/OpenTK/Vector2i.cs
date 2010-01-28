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
    public struct Vector2i : IEquatable<Vector2i>
    {
        public Vector2i(int x, int y)
            : this()
        {
            X = x;
            Y = y;
        }

        public static bool operator ==(Vector2i left, Vector2i right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector2i left, Vector2i right)
        {
            return !left.Equals(right);
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Vector2i))
                return false;

            return this.Equals((Vector2i)obj);
        }

        public bool Equals(Vector2i other)
        {
            return (X == other.X) && (Y == other.Y);
        }

        public int X { get; set; }
        public int Y { get; set; }
    }
}
