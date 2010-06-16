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
    public struct Vector4b : IEquatable<Vector4b>
    {
        public Vector4b(bool x, bool y, bool z, bool w)
            : this()
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public static bool operator ==(Vector4b left, Vector4b right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector4b left, Vector4b right)
        {
            return !left.Equals(right);
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode() ^ W.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Vector4b))
                return false;

            return this.Equals((Vector4b)obj);
        }

        public bool Equals(Vector4b other)
        {
            return (X == other.X) && (Y == other.Y) && (Z == other.Z) && (W == other.W);
        }

        public bool X { get; set; }
        public bool Y { get; set; }
        public bool Z { get; set; }
        public bool W { get; set; }
    }
}