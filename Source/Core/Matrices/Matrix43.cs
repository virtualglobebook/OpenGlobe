#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace OpenGlobe.Core
{
    /// <summary>
    /// 4x3 matrix - 4 columns and 3 rows.
    /// </summary>
    public class Matrix43<T> : IEquatable<Matrix43<T>> where T : IEquatable<T>
    {
        public Matrix43()
        {
            _values = new T[NumberOfComponents];
        }

        public Matrix43(T value)
        {
            _values = new T[] 
            { 
                value, value, value, value,
                value, value, value, value,
                value, value, value, value
            };
        }

        public Matrix43(
            T column0row0, T column1row0, T column2row0, T column3row0,
            T column0row1, T column1row1, T column2row1, T column3row1,
            T column0row2, T column1row2, T column2row2, T column3row2)
        {
            _values = new T[] 
            { 
                column0row0, column0row1, column0row2,
                column1row0, column1row1, column1row2,
                column2row0, column2row1, column2row2,
                column3row0, column3row1, column3row2
            };
        }

        public int NumberOfComponents
        {
            get { return 12; }
        }

        public T Column0Row0 { get { return _values[0]; } }
        public T Column0Row1 { get { return _values[1]; } }
        public T Column0Row2 { get { return _values[2]; } }

        public T Column1Row0 { get { return _values[3]; } }
        public T Column1Row1 { get { return _values[4]; } }
        public T Column1Row2 { get { return _values[5]; } }

        public T Column2Row0 { get { return _values[6]; } }
        public T Column2Row1 { get { return _values[7]; } }
        public T Column2Row2 { get { return _values[8]; } }

        public T Column3Row0 { get { return _values[9]; } }
        public T Column3Row1 { get { return _values[10]; } }
        public T Column3Row2 { get { return _values[11]; } }

        public static Matrix43<float> DoubleToFloat(Matrix43<double> value)
        {
            return new Matrix43<float>(
                (float)value.Column0Row0, 
                (float)value.Column1Row0, 
                (float)value.Column2Row0, 
                (float)value.Column3Row0,
                (float)value.Column0Row1, 
                (float)value.Column1Row1, 
                (float)value.Column2Row1, 
                (float)value.Column3Row1,
                (float)value.Column0Row2, 
                (float)value.Column1Row2, 
                (float)value.Column2Row2, 
                (float)value.Column3Row2);
        }

        public bool Equals(Matrix43<T> other)
        {
            if (Matrix4F.ReferenceEquals(other, null))
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

        public static bool operator ==(Matrix43<T> left, Matrix43<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Matrix43<T> left, Matrix43<T> right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            if (obj is Matrix43<T>)
            {
                return Equals((Matrix43<T>)obj);
            }
            return false;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture,
                "Rows: \n({0:n}, {1:n}, {2:n}, {3:n}) \n({4:n}, {5:n}, {6:n}, {7:n}) \n({8:n}, {9:n}, {10:n}, {11:n})",
                Column0Row0, Column1Row0, Column2Row0, Column3Row0,
                Column0Row1, Column1Row1, Column2Row1, Column3Row1,
                Column0Row2, Column1Row2, Column2Row2, Column3Row2);
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
        public T[] ReadOnlyColumnMajorValues { get { return _values; } }

        private readonly T[] _values;       // Column major
    }
}
