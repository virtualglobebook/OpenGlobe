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
    public class Matrix3D
    {
        public Matrix3D()
        {
            _values = new double[NumberOfComponents];
        }

        public Matrix3D(double value)
        {
            _values = new double[] 
            { 
                value, value, value,
                value, value, value,
                value, value, value
            };
        }

        public Matrix3D(
            double column0row0, double column1row0, double column2row0,
            double column0row1, double column1row1, double column2row1,
            double column0row2, double column1row2, double column2row2)
        {
            _values = new double[] 
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

        public double Column0Row0 { get { return _values[0]; } }
        public double Column0Row1 { get { return _values[1]; } }
        public double Column0Row2 { get { return _values[2]; } }

        public double Column1Row0 { get { return _values[3]; } }
        public double Column1Row1 { get { return _values[4]; } }
        public double Column1Row2 { get { return _values[5]; } }

        public double Column2Row0 { get { return _values[6]; } }
        public double Column2Row1 { get { return _values[7]; } }
        public double Column2Row2 { get { return _values[8]; } }

        public Matrix3S ToMatrix4S()
        {
            return new Matrix3S(
                (float)Column0Row0,
                (float)Column1Row0,
                (float)Column2Row0,
                (float)Column0Row1,
                (float)Column1Row1,
                (float)Column2Row1,
                (float)Column0Row2,
                (float)Column1Row2,
                (float)Column2Row2);
        }

        public Matrix3D Transpose()
        {
            return new Matrix3D(
                Column0Row0, Column0Row1, Column0Row2, 
                Column1Row0, Column1Row1, Column1Row2, 
                Column2Row0, Column2Row1, Column2Row2);
        }

        public static Vector3D operator *(Matrix3D matrix, Vector3D vector)
        {
            if (Matrix4D.ReferenceEquals(matrix, null))
            {
                throw new ArgumentNullException("matrix");
            }

            double[] values = matrix.ReadOnlyColumnMajorValues;

            double x =
                values[0] * vector.X +
                values[3] * vector.Y +
                values[6] * vector.Z;
            double y =
                values[1] * vector.X +
                values[4] * vector.Y +
                values[7] * vector.Z;
            double z =
                values[2] * vector.X +
                values[5] * vector.Y +
                values[8] * vector.Z;

            return new Vector3D(x, y, z);
        }

        public bool Equals(Matrix3D other)
        {
            if (Matrix3D.ReferenceEquals(other, null))
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

        public static bool operator ==(Matrix3D left, Matrix3D right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Matrix3D left, Matrix3D right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            if (obj is Matrix3D)
            {
                return Equals((Matrix3D)obj);
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
        public double[] ReadOnlyColumnMajorValues { get { return _values; } }

        public static readonly Matrix3D Identity = new Matrix3D(
            1.0, 0.0, 0.0,
            0.0, 1.0, 0.0,
            0.0, 0.0, 1.0);

        private readonly double[] _values;       // Column major
    }
}
