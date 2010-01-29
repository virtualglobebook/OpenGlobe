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
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Matrix2 : IEquatable<Matrix2>
    {
        /// <summary>
        /// Top row of the matrix
        /// </summary>
        public Vector2 Row0 { get; set; }
        /// <summary>
        /// 2nd row of the matrix
        /// </summary>
        public Vector2 Row1 { get; set; }

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        /// <param name="row0">Top row of the matrix</param>
        /// <param name="row1">Second row of the matrix</param>
        public Matrix2(Vector2 row0, Vector2 row1)
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
        /// <param name="m10">First item of the second row of the matrix.</param>
        /// <param name="m11">Second item of the second row of the matrix.</param>
        public Matrix2(
            float m00, float m01,
            float m10, float m11)
            : this()
        {
            Row0 = new Vector2(m00, m01);
            Row1 = new Vector2(m10, m11);
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
        /// Gets the value at row 1, column 1 of this instance.
        /// </summary>
        public float M11 { get { return Row0.X; } }

        /// <summary>
        /// Gets the value at row 1, column 2 of this instance.
        /// </summary>
        public float M12 { get { return Row0.Y; } }

        /// <summary>
        /// Gets the value at row 2, column 1 of this instance.
        /// </summary>
        public float M21 { get { return Row1.X; } }

        /// <summary>
        /// Gets the value at row 2, column 2 of this instance.
        /// </summary>
        public float M22 { get { return Row1.Y; } }

        public static bool operator ==(Matrix2 left, Matrix2 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Matrix2 left, Matrix2 right)
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
            if (!(obj is Matrix2))
                return false;

            return this.Equals((Matrix2)obj);
        }

        #region IEquatable<Matrix2> Members

        public bool Equals(Matrix2 other)
        {
            return
                Row0 == other.Row0 &&
                Row1 == other.Row1;
        }

        #endregion
    }
}