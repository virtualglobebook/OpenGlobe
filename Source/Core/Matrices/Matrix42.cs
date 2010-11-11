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
    /// 4x2 matrix - 4 columns and 2 rows.
    /// </summary>
    public class Matrix42<T> : IEquatable<Matrix42<T>> where T : IEquatable<T>
    {
        public Matrix42()
        {
            _values = new T[NumberOfComponents];
        }

        public Matrix42(T value)
        {
            _values = new T[] 
            { 
                value, value, value, value,
                value, value, value, value
            };
        }

        public Matrix42(
            T column0row0, T column1row0, T column2row0, T column3row0,
            T column0row1, T column1row1, T column2row1, T column3row1)
        {
            _values = new T[] 
            { 
                column0row0, column0row1, column1row0, column1row1,
                column2row0, column2row1, column3row0, column3row1
            };
        }

        public int NumberOfComponents 
        { 
            get { return 8; } 
        }

        public T Column0Row0 { get { return _values[0]; } }
        public T Column1Row0 { get { return _values[2]; } }
        public T Column2Row0 { get { return _values[4]; } }
        public T Column3Row0 { get { return _values[6]; } }

        public T Column0Row1 { get { return _values[1]; } }
        public T Column1Row1 { get { return _values[3]; } }
        public T Column2Row1 { get { return _values[5]; } }
        public T Column3Row1 { get { return _values[7]; } }

/*
        TODO
 
        public Matrix42<Y> To<Y>() where Y : IEquatable<Y>
        {
            return new Matrix42<Y>((Y)Column0Row0);
        }

        public Matrix42<float> ToFloat()
        {
            return new Matrix42<float>(
                (float)Column0Row0, (float)Column1Row0, (float)Column2Row0, (float)Column3Row0,
                (float)Column0Row1, (float)Column1Row1, (float)Column2Row1, (float)Column3Row1);
        }
*/
        public static Matrix42<float> DoubleToFloat(Matrix42<double> value)
        {
            return new Matrix42<float>(
                (float)value.Column0Row0, 
                (float)value.Column1Row0, 
                (float)value.Column2Row0, 
                (float)value.Column3Row0,
                (float)value.Column0Row1, 
                (float)value.Column1Row1, 
                (float)value.Column2Row1, 
                (float)value.Column3Row1);
        }

        public bool Equals(Matrix42<T> other)
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

        public static bool operator ==(Matrix42<T> left, Matrix42<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Matrix42<T> left, Matrix42<T> right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            if (obj is Matrix42<T>)
            {
                return Equals((Matrix42<T>)obj);
            }
            return false;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture,
                "Rows:\n({0:n}, {1:n}, {2:n}, {3:n})\n({4:n}, {5:n}, {6:n}, {7:n})",
                _values[0], _values[2], _values[4], _values[6],
                _values[1], _values[3], _values[5], _values[7]);
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
