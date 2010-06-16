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

namespace OpenGlobe.Renderer
{
    // TODO:  Belongs in OpenTK namespace
    /// <summary>
    /// 4x2 matrix - 4 columns and 2 rows
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Matrix42 : IEquatable<Matrix42>
    {
        /// <summary>
        /// Top row of the matrix
        /// </summary>
        public Vector4 Row0 { get; set; }
        /// <summary>
        /// 2nd row of the matrix
        /// </summary>
        public Vector4 Row1 { get; set; }

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        /// <param name="row0">Top row of the matrix</param>
        /// <param name="row1">Second row of the matrix</param>
        public Matrix42(Vector4 row0, Vector4 row1)
            : this()
        {
            Row0 = row0;
            Row1 = row1;
        }

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        /// <param name="m00">First item of the first row of the matrix.</param>
        /// <param name="m01">Second item of the first row of the matrix.</param>
        /// <param name="m02">Third item of the first row of the matrix.</param>
        /// <param name="m03">Fourth item of the first row of the matrix.</param>
        /// <param name="m10">First item of the second row of the matrix.</param>
        /// <param name="m11">Second item of the second row of the matrix.</param>
        /// <param name="m12">Third item of the second row of the matrix.</param>
        /// <param name="m13">Fourth item of the second row of the matrix.</param>
        public Matrix42(
            float m00, float m01, float m02, float m03,
            float m10, float m11, float m12, float m13)
            : this()
        {
            Row0 = new Vector4(m00, m01, m02, m03);
            Row1 = new Vector4(m10, m11, m12, m13);
        }

        /// <summary>
        /// The first column of this matrix
        /// </summary>
        public Vector2 Column0
        {
            get { return new Vector2(Row0.X, Row1.X); }
        }

        /// <summary>
        /// The second column of this matrix
        /// </summary>
        public Vector2 Column1
        {
            get { return new Vector2(Row0.Y, Row1.Y); }
        }

        /// <summary>
        /// The third column of this matrix
        /// </summary>
        public Vector2 Column2
        {
            get { return new Vector2(Row0.Z, Row1.Z); }
        }

        /// <summary>
        /// The fourth column of this matrix
        /// </summary>
        public Vector2 Column3
        {
            get { return new Vector2(Row0.W, Row1.W); }
        }

        /// <summary>
        /// Gets the value at row 1, column 1 of this instance.
        /// </summary>
        public float M11 { get { return Row0.X; } }

        /// <summary>
        /// Gets the value at row 1, column 2 of this instance.
        /// </summary>
        public float M12 { get { return Row0.Y; } }

        /// <summary>
        /// Gets the value at row 1, column 3 of this instance.
        /// </summary>
        public float M13 { get { return Row0.Z; } }

        /// <summary>
        /// Gets the value at row 1, column 4 of this instance.
        /// </summary>
        public float M14 { get { return Row0.W; } }

        /// <summary>
        /// Gets the value at row 2, column 1 of this instance.
        /// </summary>
        public float M21 { get { return Row1.X; } }

        /// <summary>
        /// Gets the value at row 2, column 2 of this instance.
        /// </summary>
        public float M22 { get { return Row1.Y; } }

        /// <summary>
        /// Gets the value at row 2, column 3 of this instance.
        /// </summary>
        public float M23 { get { return Row1.Z; } }

        /// <summary>
        /// Gets the value at row 2, column 4 of this instance.
        /// </summary>
        public float M24 { get { return Row1.W; } }

        public static bool operator ==(Matrix42 left, Matrix42 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Matrix42 left, Matrix42 right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "{0}\n{1}", Row0, Row1);
        }

        public override int GetHashCode()
        {
            return Row0.GetHashCode() ^ Row1.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Matrix42))
                return false;

            return this.Equals((Matrix42)obj);
        }

        #region IEquatable<Matrix42> Members

        public bool Equals(Matrix42 other)
        {
            return
                Row0 == other.Row0 &&
                Row1 == other.Row1;
        }

        #endregion
    }
}