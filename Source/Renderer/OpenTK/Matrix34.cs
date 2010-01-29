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

namespace MiniGlobe.Renderer
{
    // TODO:  Belongs in OpenTK namespace
    /// <summary>
    /// 3x2 matrix - 3 columns and 4 rows
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Matrix34 : IEquatable<Matrix34>
    {
        /// <summary>
        /// Top row of the matrix
        /// </summary>
        public Vector3 Row0 { get; set; }
        /// <summary>
        /// 2nd row of the matrix
        /// </summary>
        public Vector3 Row1 { get; set; }
        /// <summary>
        /// 3rd row of the matrix
        /// </summary>
        public Vector3 Row2 { get; set; }
        /// <summary>
        /// 4th row of the matrix
        /// </summary>
        public Vector3 Row3 { get; set; }

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        /// <param name="row0">Top row of the matrix</param>
        /// <param name="row1">Second row of the matrix</param>
        /// <param name="row2">Third row of the matrix</param>
        /// <param name="row3">Fourth row of the matrix</param>
        public Matrix34(Vector3 row0, Vector3 row1, Vector3 row2, Vector3 row3)
            : this()
        {
            Row0 = row0;
            Row1 = row1;
            Row2 = row2;
            Row3 = row3;
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
        /// <param name="m30">First item of the fourth row of the matrix.</param>
        /// <param name="m31">Second item of the fourth row of the matrix.</param>
        /// <param name="m32">Third item of the fourth row of the matrix.</param>
        public Matrix34(
            float m00, float m01, float m02,
            float m10, float m11, float m12,
            float m20, float m21, float m22,
            float m30, float m31, float m32)
            : this()
        {
            Row0 = new Vector3(m00, m01, m02);
            Row1 = new Vector3(m10, m11, m12);
            Row2 = new Vector3(m20, m21, m22);
            Row3 = new Vector3(m30, m31, m32);
        }

        /// <summary>
        /// The first column of this matrix
        /// </summary>
        public Vector4 Column0
        {
            get { return new Vector4(Row0.X, Row1.X, Row2.X, Row3.X); }
        }

        /// <summary>
        /// The second column of this matrix
        /// </summary>
        public Vector4 Column1
        {
            get { return new Vector4(Row0.Y, Row1.Y, Row2.Y, Row2.Y); }
        }

        /// <summary>
        /// The third column of this matrix
        /// </summary>
        public Vector4 Column2
        {
            get { return new Vector4(Row0.Z, Row1.Z, Row2.Z, Row3.Z); }
        }

        /// <summary>
        /// Gets or sets the value at row 1, column 1 of this instance.
        /// </summary>
        public float M11 { get { return Row0.X; } }

        /// <summary>
        /// Gets or sets the value at row 1, column 2 of this instance.
        /// </summary>
        public float M12 { get { return Row0.Y; } }

        /// <summary>
        /// Gets or sets the value at row 1, column 3 of this instance.
        /// </summary>
        public float M13 { get { return Row0.Z; } }

        /// <summary>
        /// Gets or sets the value at row 2, column 1 of this instance.
        /// </summary>
        public float M21 { get { return Row1.X; } }

        /// <summary>
        /// Gets or sets the value at row 2, column 2 of this instance.
        /// </summary>
        public float M22 { get { return Row1.Y; } }

        /// <summary>
        /// Gets or sets the value at row 2, column 3 of this instance.
        /// </summary>
        public float M23 { get { return Row1.Z; } }

        /// <summary>
        /// Gets or sets the value at row 3, column 1 of this instance.
        /// </summary>
        public float M31 { get { return Row2.X; } }

        /// <summary>
        /// Gets or sets the value at row 3, column 2 of this instance.
        /// </summary>
        public float M32 { get { return Row2.Y; } }

        /// <summary>
        /// Gets or sets the value at row 3, column 3 of this instance.
        /// </summary>
        public float M33 { get { return Row2.Z; } }

        /// <summary>
        /// Gets or sets the value at row 4, column 1 of this instance.
        /// </summary>
        public float M41 { get { return Row3.X; } }

        /// <summary>
        /// Gets or sets the value at row 4, column 2 of this instance.
        /// </summary>
        public float M42 { get { return Row3.Y; } }

        /// <summary>
        /// Gets or sets the value at row 4, column 3 of this instance.
        /// </summary>
        public float M43 { get { return Row3.Z; } }

        public static bool operator ==(Matrix34 left, Matrix34 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Matrix34 left, Matrix34 right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "{0}\n{1}\n{2}\n{3}", Row0, Row1, Row2, Row3);
        }

        public override int GetHashCode()
        {
            return Row0.GetHashCode() ^ Row1.GetHashCode() ^ Row2.GetHashCode() ^ Row3.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Matrix34))
                return false;

            return this.Equals((Matrix34)obj);
        }

        #region IEquatable<Matrix34> Members

        public bool Equals(Matrix34 other)
        {
            return
                Row0 == other.Row0 &&
                Row1 == other.Row1 &&
                Row2 == other.Row2 &&
                Row3 == other.Row3;
        }

        #endregion
    }
}