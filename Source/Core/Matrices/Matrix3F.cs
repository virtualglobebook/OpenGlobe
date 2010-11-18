#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace OpenGlobe.Core
{
    /// <summary>
    /// 3x3 matrix - 3 columns and 3 rows.
    /// </summary>
    public class Matrix3S
    {
        public Matrix3S()
        {
            _values = new float[NumberOfComponents];
        }

        public Matrix3S(float value)
        {
            _values = new float[] 
            { 
                value, value, value,
                value, value, value,
                value, value, value
            };
        }

        public Matrix3S(
            float column0row0, float column1row0, float column2row0,
            float column0row1, float column1row1, float column2row1,
            float column0row2, float column1row2, float column2row2)
        {
            _values = new float[] 
            { 
                column0row0, column0row1, column0row2,
                column1row0, column1row1, column1row2,
                column2row0, column2row1, column2row2,
            };
        }

        public int NumberOfComponents
        {
            get { return 9; }
        }

        public float Column0Row0 { get { return _values[0]; } }
        public float Column0Row1 { get { return _values[1]; } }
        public float Column0Row2 { get { return _values[2]; } }

        public float Column1Row0 { get { return _values[3]; } }
        public float Column1Row1 { get { return _values[4]; } }
        public float Column1Row2 { get { return _values[5]; } }

        public float Column2Row0 { get { return _values[6]; } }
        public float Column2Row1 { get { return _values[7]; } }
        public float Column2Row2 { get { return _values[8]; } }

        public bool Equals(Matrix3S other)
        {
            if (Matrix3S.ReferenceEquals(other, null))
            {
                return false;
            }

            for (int i = 0; i < _values.Length; ++i)
            {
                if (!_values[i].Equals(other._values[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool operator ==(Matrix3S left, Matrix3S right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Matrix3S left, Matrix3S right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            if (obj is Matrix3S)
            {
                return Equals((Matrix3S)obj);
            }
            return false;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture,
                "Rows: \n({0:n}, {1:n}, {2:n}) \n({3:n}, {4:n}, {5:n}) \n({6:n}, {7:n}, {8:n}) \n({9:n}, {10:n}, {11:n})",
                Column0Row0, Column1Row0, Column2Row0,
                Column0Row1, Column1Row1, Column2Row1,
                Column0Row2, Column1Row2, Column2Row2);
        }

        public override int GetHashCode()
        {
            int hashCode = Convert.ToInt32(_values[0]);

            for (int i = 1; i < _values.Length; ++i)
            {
                hashCode = hashCode ^ Convert.ToInt32(_values[i]);

            }

            return hashCode;
        }

        /// <summary>
        /// Shame on you if you change elements in this array.  This type is supposed to be immutable.
        /// </summary>
        public float[] ReadOnlyColumnMajorValues { get { return _values; } }

        public static readonly Matrix3S Identity = new Matrix3S(
            1.0f, 0.0f, 0.0f,
            0.0f, 1.0f, 0.0f,
            0.0f, 0.0f, 1.0f);

        private readonly float[] _values;       // Column major
    }
}
