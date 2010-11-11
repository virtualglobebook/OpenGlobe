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

        public Matrix4F ToMatrix4F()
        {
            return new Matrix4F(
                (float)Column0Row0,
                (float)Column1Row0,
                (float)Column2Row0,
                (float)Column3Row0,
                (float)Column0Row1,
                (float)Column1Row1,
                (float)Column2Row1,
                (float)Column3Row1,
                (float)Column0Row2,
                (float)Column1Row2,
                (float)Column2Row2,
                (float)Column3Row2,
                (float)Column0Row3,
                (float)Column1Row3,
                (float)Column2Row3,
                (float)Column3Row3);
        }

        public Matrix4D Transpose()
        {
            return new Matrix4D(
                Column0Row0, Column0Row1, Column0Row2, Column0Row3,
                Column1Row0, Column1Row1, Column1Row2, Column1Row3,
                Column2Row0, Column2Row1, Column2Row2, Column2Row3,
                Column3Row0, Column3Row1, Column3Row2, Column3Row3);
        }

        public static Matrix4D CreatePerspectiveFieldOfView(double fovy, double aspect, double zNear, double zFar)
        {
            if (fovy <= 0.0 || fovy > Math.PI)
            {
                throw new ArgumentOutOfRangeException("fovy", "fovy must be in [0, PI).");
            }

            if (aspect <= 0.0)
            {
                throw new ArgumentOutOfRangeException("aspect", "aspect must be greater than zero.");
            }

            if (zNear <= 0.0)
            {
                throw new ArgumentOutOfRangeException("zNear", "zNear must be greater than zero.");
            }

            if (zFar <= 0.0)
            {
                throw new ArgumentOutOfRangeException("zFar", "zFar must be greater than zero.");
            }

            double bottom = Math.Tan(fovy * 0.5);
            double f = 1.0 / bottom;

            return new Matrix4D(
                f / aspect, 0.0,                             0.0, 0.0,
                0.0,          f,                             0.0, 0.0,
                0.0,        0.0, (zFar + zNear) / (zNear - zFar), (2.0 * zFar * zNear) / (zNear - zFar),
                0.0,        0.0,                            -1.0, 0.0);
        }

        public static Matrix4D CreateOrthographicOffCenter(double left, double right, double bottom, double top, double zNear, double zFar)
        {
            double a = 1.0 / (right - left);
            double b = 1.0 / (top - bottom);
            double c = 1.0 / (zFar - zNear);

            double tx = -(right + left) * a;
            double ty = -(top + bottom) * b;
            double tz = -(zFar + zNear) * c;

            return new Matrix4D(
                2.0 * a, 0.0,      0.0,     tx,
                0.0,     2.0 * b,  0.0,     ty,
                0.0,     0.0,     -2.0 * c, tz,
                0.0,     0.0,      0.0,     1.0);
        }

        public static Matrix4D LookAt(Vector3D eye, Vector3D target, Vector3D up)
        {
            Vector3D f = (target - eye).Normalize();
            Vector3D s = f.Cross(up).Normalize();
            Vector3D u = s.Cross(f).Normalize();

            Matrix4D rotation = new Matrix4D(
                 s.X,  s.Y,  s.Z, 0.0,
                 u.X,  u.Y,  u.Z, 0.0,
                -f.X, -f.Y, -f.Z, 0.0,
                 0.0,  0.0,  0.0, 1.0);
            Matrix4D translation = new Matrix4D(
                1.0, 0.0, 0.0, -eye.X,
                0.0, 1.0, 0.0, -eye.Y,
                0.0, 0.0, 1.0, -eye.Z,
                0.0, 0.0, 0.0, 1.0);
            return rotation * translation;
        }

        public static Matrix4D operator *(Matrix4D left, Matrix4D right)
        {
            double[] leftValues = left.ReadOnlyColumnMajorValues;
            double[] rightValues = right.ReadOnlyColumnMajorValues;

            double col0row0 = 
                leftValues[0] * rightValues[0] + 
                leftValues[4] * rightValues[1] + 
                leftValues[8] * rightValues[2] + 
                leftValues[12] * rightValues[3];
            double col0row1 = 
                leftValues[1] * rightValues[0] + 
                leftValues[5] * rightValues[1] + 
                leftValues[9] * rightValues[2] + 
                leftValues[13] * rightValues[3];
            double col0row2 = 
                leftValues[2] * rightValues[0] + 
                leftValues[6] * rightValues[1] + 
                leftValues[10] * rightValues[2] + 
                leftValues[14] * rightValues[3];
            double col0row3 = 
                leftValues[3] * rightValues[0] + 
                leftValues[7] * rightValues[1] + 
                leftValues[11] * rightValues[2] + 
                leftValues[15] * rightValues[3];

            double col1row0 = 
                leftValues[0] * rightValues[4] + 
                leftValues[4] * rightValues[5] + 
                leftValues[8] * rightValues[6] + 
                leftValues[12] * rightValues[7];
            double col1row1 = 
                leftValues[1] * rightValues[4] + 
                leftValues[5] * rightValues[5] + 
                leftValues[9] * rightValues[6] + 
                leftValues[13] * rightValues[7];
            double col1row2 = 
                leftValues[2] * rightValues[4] + 
                leftValues[6] * rightValues[5] + 
                leftValues[10] * rightValues[6] + 
                leftValues[14] * rightValues[7];
            double col1row3 = 
                leftValues[3] * rightValues[4] + 
                leftValues[7] * rightValues[5] + 
                leftValues[11] * rightValues[6] + 
                leftValues[15] * rightValues[7];

            double col2row0 = 
                leftValues[0] * rightValues[8] + 
                leftValues[4] * rightValues[9] + 
                leftValues[8] * rightValues[10] + 
                leftValues[12] * rightValues[11];
            double col2row1 = 
                leftValues[1] * rightValues[8] + 
                leftValues[5] * rightValues[9] + 
                leftValues[9] * rightValues[10] + 
                leftValues[13] * rightValues[11];
            double col2row2 = 
                leftValues[2] * rightValues[8] + 
                leftValues[6] * rightValues[9] + 
                leftValues[10] * rightValues[10] + 
                leftValues[14] * rightValues[11];
            double col2row3 = 
                leftValues[3] * rightValues[8] + 
                leftValues[7] * rightValues[9] + 
                leftValues[11] * rightValues[10] + 
                leftValues[15] * rightValues[11];

            double col3row0 = 
                leftValues[0] * rightValues[12] + 
                leftValues[4] * rightValues[13] + 
                leftValues[8] * rightValues[14] + 
                leftValues[12] * rightValues[15];
            double col3row1 = 
                leftValues[1] * rightValues[12] + 
                leftValues[5] * rightValues[13] + 
                leftValues[9] * rightValues[14] + 
                leftValues[13] * rightValues[15];
            double col3row2 = 
                leftValues[2] * rightValues[12] + 
                leftValues[6] * rightValues[13] + 
                leftValues[10] * rightValues[14] + 
                leftValues[14] * rightValues[15];
            double col3row3 = 
                leftValues[3] * rightValues[12] + 
                leftValues[7] * rightValues[13] + 
                leftValues[11] * rightValues[14] + 
                leftValues[15] * rightValues[15];

            return new Matrix4D(
                col0row0, col1row0, col2row0, col3row0,
                col0row1, col1row1, col2row1, col3row1,
                col0row2, col1row2, col2row2, col3row2,
                col0row3, col1row3, col2row3, col3row3);
        }

        public static Vector4D operator *(Matrix4D matrix, Vector4D vector)
        {
            double[] values = matrix.ReadOnlyColumnMajorValues;

            double x = 
                values[0] * vector.X + 
                values[4] * vector.Y + 
                values[8] * vector.Z + 
                values[12] * vector.W;
            double y = 
                values[1] * vector.X + 
                values[5] * vector.Y + 
                values[9] * vector.Z + 
                values[13] * vector.W;
            double z = 
                values[2] * vector.X + 
                values[6] * vector.Y + 
                values[10] * vector.Z + 
                values[14] * vector.W;
            double w = 
                values[3] * vector.X + 
                values[7] * vector.Y + 
                values[11] * vector.Z + 
                values[15] * vector.W;

            return new Vector4D(x, y, z, w);
        }

        public bool Equals(Matrix4D other)
        {
            if (Matrix4D.ReferenceEquals(other, null))
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
        
        public static readonly Matrix4D Identity = new Matrix4D(
            1.0, 0.0, 0.0, 0.0,
            0.0, 1.0, 0.0, 0.0,
            0.0, 0.0, 1.0, 0.0,
            0.0, 0.0, 0.0, 1.0);

        private readonly double[] _values;       // Column major
    }
}
