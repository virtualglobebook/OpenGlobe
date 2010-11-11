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
    /// 2x2 matrix - 2 columns and 2 rows.
    /// </summary>
    public class Matrix2<T> : IEquatable<Matrix2<T>> where T : IEquatable<T>
    {
        public Matrix2()
        {
            _values = new T[NumberOfComponents];
        }

        public Matrix2(T value)
        {
            _values = new T[] 
            { 
                value, value, 
                value, value,
            };
        }

        public Matrix2(
            T column0row0, T column1row0,
            T column0row1, T column1row1)
        {
            _values = new T[] 
            { 
                column0row0, column0row1, 
                column1row0, column1row1 
            };
        }

        public int NumberOfComponents
        {
            get { return 4; }
        }

        public T Column0Row0 { get { return _values[0]; } }
        public T Column0Row1 { get { return _values[1]; } }

        public T Column1Row0 { get { return _values[2]; } }
        public T Column1Row1 { get { return _values[3]; } }

        public static Matrix2<float> DoubleToFloat(Matrix2<double> value)
        {
            return new Matrix2<float>(
                (float)value.Column0Row0, 
                (float)value.Column1Row0,
                (float)value.Column0Row1, 
                (float)value.Column1Row1);
        }

        public bool Equals(Matrix2<T> other)
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

        public static bool operator ==(Matrix2<T> left, Matrix2<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Matrix2<T> left, Matrix2<T> right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            if (obj is Matrix2<T>)
            {
                return Equals((Matrix2<T>)obj);
            }
            return false;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture,
                "Rows: \n({0:n}, {1:n}) \n({2:n}, {3:n})",
                Column0Row0, Column1Row0,
                Column0Row1, Column1Row1);
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
