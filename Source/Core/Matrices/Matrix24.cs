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
    /// 2x4 matrix - 2 columns and 4 rows.
    /// </summary>
    public class Matrix24<T> : IEquatable<Matrix24<T>> where T : IEquatable<T>
    {
        public Matrix24()
        {
            _values = new T[NumberOfComponents];
        }

        public Matrix24(T value)
        {
            _values = new T[] 
            { 
                value, value, 
                value, value,
                value, value, 
                value, value
            };
        }

        public Matrix24(
            T column0row0, T column1row0, 
            T column0row1, T column1row1,
            T column0row2, T column1row2, 
            T column0row3, T column1row3)
        {
            _values = new T[] 
            { 
                column0row0, column0row1, column0row2, column0row3,
                column1row0, column1row1, column1row2, column1row3
            };
        }

        public int NumberOfComponents
        {
            get { return 8; }
        }

        public T Column0Row0 { get { return _values[0]; } }
        public T Column0Row1 { get { return _values[1]; } }
        public T Column0Row2 { get { return _values[2]; } }
        public T Column0Row3 { get { return _values[3]; } }

        public T Column1Row0 { get { return _values[4]; } }
        public T Column1Row1 { get { return _values[5]; } }
        public T Column1Row2 { get { return _values[6]; } }
        public T Column1Row3 { get { return _values[7]; } }

        public static Matrix24<float> DoubleToFloat(Matrix24<double> value)
        {
            return new Matrix24<float>(
                (float)value.Column0Row0,
                (float)value.Column1Row0,
                (float)value.Column0Row1,
                (float)value.Column1Row1,
                (float)value.Column0Row2,
                (float)value.Column1Row2,
                (float)value.Column0Row3,
                (float)value.Column1Row3);
        }

        public bool Equals(Matrix24<T> other)
        {
            if (Matrix4S.ReferenceEquals(other, null))
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

        public static bool operator ==(Matrix24<T> left, Matrix24<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Matrix24<T> left, Matrix24<T> right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            if (obj is Matrix24<T>)
            {
                return Equals((Matrix24<T>)obj);
            }
            return false;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture,
                "Rows: \n({0:n}, {1:n}) \n({2:n}, {3:n}) \n({4:n}, {5:n}) \n({6:n}, {7:n})",
                Column0Row0, Column1Row0,
                Column0Row1, Column1Row1,
                Column0Row2, Column1Row2,
                Column0Row3, Column1Row3);
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
