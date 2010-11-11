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
    /// 4x4 matrix - 4 columns and 4 rows.
    /// </summary>
    public class Matrix4D
    {
        public Matrix4D()
        {
            _values = new double[NumberOfComponents];
        }

        public Matrix4D(double value)
        {
            _values = new double[] 
            { 
                value, value, value, value,
                value, value, value, value,
                value, value, value, value,
                value, value, value, value
            };
        }

        public Matrix4D(
            double column0row0, double column1row0, double column2row0, double column3row0,
            double column0row1, double column1row1, double column2row1, double column3row1,
            double column0row2, double column1row2, double column2row2, double column3row2,
            double column0row3, double column1row3, double column2row3, double column3row3)
        {
            _values = new double[] 
            { 
                column0row0, column0row1, column0row2, column0row3,
                column1row0, column1row1, column1row2, column1row3,
                column2row0, column2row1, column2row2, column2row3,
                column3row0, column3row1, column3row2, column3row3
            };
        }

        public int NumberOfComponents
        {
            get { return 16; }
        }

        public double Column0Row0 { get { return _values[0]; } }
        public double Column0Row1 { get { return _values[1]; } }
        public double Column0Row2 { get { return _values[2]; } }
        public double Column0Row3 { get { return _values[3]; } }

        public double Column1Row0 { get { return _values[4]; } }
        public double Column1Row1 { get { return _values[5]; } }
        public double Column1Row2 { get { return _values[6]; } }
        public double Column1Row3 { get { return _values[7]; } }

        public double Column2Row0 { get { return _values[8]; } }
        public double Column2Row1 { get { return _values[9]; } }
        public double Column2Row2 { get { return _values[10]; } }
        public double Column2Row3 { get { return _values[11]; } }

        public double Column3Row0 { get { return _values[12]; } }
        public double Column3Row1 { get { return _values[13]; } }
        public double Column3Row2 { get { return _values[14]; } }
        public double Column3Row3 { get { return _values[15]; } }

/*
        public static Matrix4F DoubleToFloat(Matrix4D<double> value)
        {
            return new Matrix4F(
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
                (float)value.Column3Row2,
                (float)value.Column0Row3,
                (float)value.Column1Row3,
                (float)value.Column2Row3,
                (float)value.Column3Row3);
        }
*/

        public bool Equals(Matrix4D other)
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

        public static bool operator ==(Matrix4D left, Matrix4D right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Matrix4D left, Matrix4D right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            if (obj is Matrix4D)
            {
                return Equals((Matrix4D)obj);
            }
            return false;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture,
                "Rows: \n({0:n}, {1:n}, {2:n}, {3:n}) \n({4:n}, {5:n}, {6:n}, {7:n}) \n({8:n}, {9:n}, {10:n}, {11:n}) \n({12:n}, {13:n}, {14:n}, {15:n})",
                Column0Row0, Column1Row0, Column2Row0, Column3Row0,
                Column0Row1, Column1Row1, Column2Row1, Column3Row1,
                Column0Row2, Column1Row2, Column2Row2, Column3Row2,
                Column0Row3, Column1Row3, Column2Row3, Column3Row3);
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
        public double[] ReadOnlyColumnMajorValues { get { return _values; } }

        private readonly double[] _values;       // Column major
    }
}
