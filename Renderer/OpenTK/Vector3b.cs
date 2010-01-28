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
    public struct Vector3b : IEquatable<Vector3b>
    {
        public Vector3b(bool x, bool y, bool z)
            : this()
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static bool operator ==(Vector3b left, Vector3b right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector3b left, Vector3b right)
        {
            return !left.Equals(right);
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Vector3b))
                return false;

            return this.Equals((Vector3b)obj);
        }

        public bool Equals(Vector3b other)
        {
            return (X == other.X) && (Y == other.Y) && (Z == other.Z);
        }

        public bool X { get; set; }
        public bool Y { get; set; }
        public bool Z { get; set; }
    }
}