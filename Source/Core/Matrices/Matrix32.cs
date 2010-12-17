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
    /// 3x2 matrix - 3 columns and 2 rows.
    /// </summary>
    public class Matrix32<T> : IEquatable<Matrix32<T>> where T : IEquatable<T>
    {
        public Matrix32()
        {
            _values = new T[NumberOfComponents];
        }

        public Matrix32(T value)
        {
            _values = new T[] 
            { 
                value, value, value, 
                value, value, value
            };
        }

        public Matrix32(
            T column0row0, T column1row0, T column2row0, 
            T column0row1, T column1row1, T column2row1)
        {
            _values = new T[] 
            { 
                column0row0, column0row1, 
                column1row0, column1row1,
                column2row0, column2row1, 
            };
        }

        public int NumberOfComponents
        {
            get { return 6; }
        }

        public T Column0Row0 { get { return _values[0]; } }
        public T Column0Row1 { get { return _values[1]; } }

        public T Column1Row0 { get { return _values[2]; } }
        public T Column1Row1 { get { return _values[3]; } }

        public T Column2Row0 { get { return _values[4]; } }
        public T Column2Row1 { get { return _values[5]; } }

        public static Matrix32<float> DoubleToFloat(Matrix32<double> value)
        {
            return new Matrix32<float>(
                (float)value.Column0Row0,
                (float)value.Column1Row0,
                (float)value.Column2Row0,
                (float)value.Column0Row1,
                (float)value.Column1Row1,
                (float)value.Column2Row1);
        }

        public bool Equals(Matrix32<T> other)
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

        public static bool operator ==(Matrix32<T> left, Matrix32<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Matrix32<T> left, Matrix32<T> right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            if (obj is Matrix32<T>)
            {
                return Equals((Matrix32<T>)obj);
            }
            return false;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture,
                "Rows: \n({0:n}, {1:n}, {2:n}) \n({3:n}, {4:n}, {5:n})",
                Column0Row0, Column1Row0, Column2Row0, 
                Column0Row1, Column1Row1, Column2Row1);
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
