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
    /// 2x3 matrix - 2 columns and 3 rows.
    /// </summary>
    public class Matrix23<T> : IEquatable<Matrix23<T>> where T : IEquatable<T>
    {
        public Matrix23()
        {
            _values = new T[NumberOfComponents];
        }

        public Matrix23(T value)
        {
            _values = new T[] 
            { 
                value, value, 
                value, value,
                value, value
            };
        }

        public Matrix23(
            T column0row0, T column1row0,
            T column0row1, T column1row1,
            T column0row2, T column1row2)
        {
            _values = new T[] 
            { 
                column0row0, column0row1, column0row2, 
                column1row0, column1row1, column1row2
            };
        }

        public int NumberOfComponents
        {
            get { return 6; }
        }

        public T Column0Row0 { get { return _values[0]; } }
        public T Column0Row1 { get { return _values[1]; } }
        public T Column0Row2 { get { return _values[2]; } }

        public T Column1Row0 { get { return _values[3]; } }
        public T Column1Row1 { get { return _values[4]; } }
        public T Column1Row2 { get { return _values[5]; } }

        public static Matrix23<float> DoubleToFloat(Matrix23<double> value)
        {
            return new Matrix23<float>(
                (float)value.Column0Row0,
                (float)value.Column1Row0,
                (float)value.Column0Row1,
                (float)value.Column1Row1,
                (float)value.Column0Row2,
                (float)value.Column1Row2);
        }

        public bool Equals(Matrix23<T> other)
        {
            //TODO
            //if (other == null)
            //{
            //    return false;
            //}

            for (int i = 0; i < _values.Length; ++i)
            {
                if (!_values[i].Equals(other._values[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool operator ==(Matrix23<T> left, Matrix23<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Matrix23<T> left, Matrix23<T> right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            if (obj is Matrix23<T>)
            {
                return Equals((Matrix23<T>)obj);
            }
            return false;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture,
                "Rows: \n({0:n}, {1:n}) \n({2:n}, {3:n}) \n({4:n}, {5:n})",
                Column0Row0, Column1Row0,
                Column0Row1, Column1Row1,
                Column0Row2, Column1Row2);
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
