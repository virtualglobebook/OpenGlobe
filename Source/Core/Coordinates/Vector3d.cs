#region License
//
// (C) Copyright 2010 Patrick Cozzi, Deron Ohlarik, and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Collections.Generic;
using System.Text;

namespace MiniGlobe.Core.Coordinates
{
    /// <summary>
    /// A set of 3-dimensional cartesian coordinates where the three components,
    /// <see cref="X"/>, <see cref="Y"/>, and <see cref="Z"/>, are represented as
    /// double-precision (64-bit) floating point numbers.
    /// </summary>
    public struct Vector3d
    {
        public Vector3d(double x, double y, double z)
        {
            _x = x;
            _y = y;
            _z = z;
        }

        public double X
        {
            get { return _x; }
        }

        public double Y
        {
            get { return _y; }
        }

        public double Z
        {
            get { return _z; }
        }

        public double MagnitudeSquared
        {
            get { return _x * _x + _y * _y + _z * _z; }
        }

        public double Magnitude
        {
            get { return Math.Sqrt(MagnitudeSquared); }
        }

        public Vector3d Normalize(out double magnitude)
        {
            magnitude = Magnitude;
            return this / magnitude;
        }

        public Vector3d Normalize()
        {
            double magnitude;
            return Normalize(out magnitude);
        }

        public Vector3d Add(Vector3d addend)
        {
            return this + addend;
        }

        public Vector3d Subtract(Vector3d subtrahend)
        {
            return this - subtrahend;
        }

        public Vector3d Multiply(double scalar)
        {
            return this * scalar;
        }

        public Vector3d Divide(double scalar)
        {
            return this / scalar;
        }

        public static Vector3d operator +(Vector3d left, Vector3d right)
        {
            return new Vector3d(left._x + right._x, left._y + right._y, left._z + right._z);
        }

        public static Vector3d operator -(Vector3d left, Vector3d right)
        {
            return new Vector3d(left._x - right._x, left._y - right._y, left._z - right._z);
        }

        public static Vector3d operator *(Vector3d left, double right)
        {
            return new Vector3d(left._x * right, left._y * right, left._z * right);
        }

        public static Vector3d operator *(double left, Vector3d right)
        {
            return right * left;
        }

        public static Vector3d operator /(Vector3d left, double right)
        {
            return new Vector3d(left._x / right, left._y / right, left._z / right);
        }

        private readonly double _x;
        private readonly double _y;
        private readonly double _z;
    }
}
