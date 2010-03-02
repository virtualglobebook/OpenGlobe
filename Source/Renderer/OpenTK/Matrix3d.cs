#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Globalization;
using System.Runtime.InteropServices;
using OpenTK;
using MiniGlobe.Core;

namespace MiniGlobe.Renderer
{
    // TODO:  Belongs in OpenTK namespace
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Matrix3d : IEquatable<Matrix3d>
    {
        /// <summary>
        /// Top row of the matrix
        /// </summary>
        public Vector3D Row0 { get; set; }
        /// <summary>
        /// 2nd row of the matrix
        /// </summary>
        public Vector3D Row1 { get; set; }
        /// <summary>
        /// 3rd row of the matrix
        /// </summary>
        public Vector3D Row2 { get; set; }

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        /// <param name="row0">Top row of the matrix</param>
        /// <param name="row1">Second row of the matrix</param>
        /// <param name="row2">Third row of the matrix</param>
        public Matrix3d(Vector3D row0, Vector3D row1, Vector3D row2)
            : this()
        {
            Row0 = row0;
            Row1 = row1;
            Row2 = row2;
        }

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        /// <param name="m00">First item of the first row of the matrix.</param>
        /// <param name="m01">Second item of the first row of the matrix.</param>
        /// <param name="m02">Third item of the first row of the matrix.</param>
        /// <param name="m10">First item of the second row of the matrix.</param>
        /// <param name="m11">Second item of the second row of the matrix.</param>
        /// <param name="m12">Third item of the second row of the matrix.</param>
        /// <param name="m20">First item of the third row of the matrix.</param>
        /// <param name="m21">Second item of the third row of the matrix.</param>
        /// <param name="m22">Third item of the third row of the matrix.</param>
        public Matrix3d(
            double m00, double m01, double m02,
            double m10, double m11, double m12,
            double m20, double m21, double m22)
            : this()
        {
            Row0 = new Vector3D(m00, m01, m02);
            Row1 = new Vector3D(m10, m11, m12);
            Row2 = new Vector3D(m20, m21, m22);
        }

        /// <summary>
        /// The first column of this matrix
        /// </summary>
        public Vector3D Column0
        {
            get { return new Vector3D(Row0.X, Row1.X, Row2.X); }
        }

        /// <summary>
        /// The second column of this matrix
        /// </summary>
        public Vector3D Column1
        {
            get { return new Vector3D(Row0.Y, Row1.Y, Row2.Y); }
        }

        /// <summary>
        /// The third column of this matrix
        /// </summary>
        public Vector3D Column2
        {
            get { return new Vector3D(Row0.Z, Row1.Z, Row2.Z); }
        }

        /// <summary>
        /// Gets the value at row 1, column 1 of this instance.
        /// </summary>
        public double M11 { get { return Row0.X; } }

        /// <summary>
        /// Gets the value at row 1, column 2 of this instance.
        /// </summary>
        public double M12 { get { return Row0.Y; } }

        /// <summary>
        /// Gets the value at row 1, column 3 of this instance.
        /// </summary>
        public double M13 { get { return Row0.Z; } }

        /// <summary>
        /// Gets the value at row 2, column 1 of this instance.
        /// </summary>
        public double M21 { get { return Row1.X; } }

        /// <summary>
        /// Gets the value at row 2, column 2 of this instance.
        /// </summary>
        public double M22 { get { return Row1.Y; } }

        /// <summary>
        /// Gets the value at row 2, column 3 of this instance.
        /// </summary>
        public double M23 { get { return Row1.Z; } }

        /// <summary>
        /// Gets the value at row 3, column 1 of this instance.
        /// </summary>
        public double M31 { get { return Row2.X; } }

        /// <summary>
        /// Gets the value at row 3, column 2 of this instance.
        /// </summary>
        public double M32 { get { return Row2.Y; } }

        /// <summary>
        /// Gets the value at row 3, column 3 of this instance.
        /// </summary>
        public double M33 { get { return Row2.Z; } }

        public static bool operator ==(Matrix3d left, Matrix3d right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Matrix3d left, Matrix3d right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "{0}\n{1}\n{2}", Row0, Row1, Row2);
        }

        public override int GetHashCode()
        {
            return Row0.GetHashCode() ^ Row1.GetHashCode() ^ Row2.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Matrix3d))
                return false;

            return this.Equals((Matrix3d)obj);
        }

        #region IEquatable<Matrix3d> Members

        public bool Equals(Matrix3d other)
        {
            return
                Row0 == other.Row0 &&
                Row1 == other.Row1 &&
                Row2 == other.Row2;
        }

        #endregion

        public Matrix3d Transpose()
        {
            return new Matrix3d(Column0, Column1, Column2);
        }

        public static Vector3D operator *(Matrix3d left, Vector3D right)
        {
            return new Vector3D(left.Row0.Dot(right),
                                left.Row1.Dot(right),
                                left.Row2.Dot(right));
        }

        public static Matrix3d Identity
        {
            get { return _identity; }
        }

        private static Matrix3d _identity = new Matrix3d(1.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 1.0);
    }
}