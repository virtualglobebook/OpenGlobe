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
    public struct Vector2b : IEquatable<Vector2b>
    {
        public Vector2b(bool x, bool y)
            : this()
        {
            X = x;
            Y = y;
        }

        public static bool operator ==(Vector2b left, Vector2b right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector2b left, Vector2b right)
        {
            return !left.Equals(right);
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Vector2b))
                return false;

            return this.Equals((Vector2b)obj);
        }

        public bool Equals(Vector2b other)
        {
            return (X == other.X) && (Y == other.Y);
        }

        public bool X { get; set; }
        public bool Y { get; set; }
    }
}
