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

        /// <summary>
        /// Initializes a 4x4 transformation matrix composed of a
        /// 3x3 rotation matrix in the upper left and a 3D
        /// translation in the upper right.  The bottom row is
        /// [0, 0, 0, 1].
        /// </summary>
        /// <param name="rotation">
        /// The rotation matrix to initialize the upper left 3x3 elements.
        /// </param>
        /// <param name="translation">
        /// The translation to initialize the upper right 3D elements.
        /// </param>
        public Matrix4D(Matrix3D rotation, Vector3D translation)
        {
            _values = new double[] 
            { 
                rotation.Column0Row0, rotation.Column0Row1, rotation.Column0Row2, 0.0,
                rotation.Column1Row0, rotation.Column1Row1, rotation.Column1Row2, 0.0,
                rotation.Column2Row0, rotation.Column2Row1, rotation.Column2Row2, 0.0,
                translation.X, translation.Y, translation.Z, 1.0
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

        /// <summary>
        /// Returns the inverse of the matrix assuming it is a
        /// transformation matrix, where the upper left 3x3 elements
        /// are a rotation matrix, and the upper three elements in
        /// the fourth column are the translation.  The bottom row
        /// is assumed to be [0, 0, 0, 1].
        /// </summary>
        /// <remarks>
        /// This method is faster than computing the inverse for a 
        /// general 4x4 matrix using <cref = "Inverse" />.
        /// <p />
        /// The matrix is not verified to be in the proper form.
        /// </remarks>
        /// <returns>The inverse of the matrix</returns>
        /// <seealso cref="Matrix4D(Matrix3D,Vector3D)" />
        /// <seealso cref="Inverse" />
        public Matrix4D InverseTransformation()
        {
            // r = rotaton, rT = r^-1
            Matrix3D rT = RotationTranspose();

            // T = translation, rTT = (-rT)(T)
            Vector3D rTT = -rT * Translation;

            // [ rT, rTT ]
            // [  0,  1  ]
            return new Matrix4D(
                rT.Column0Row0, rT.Column1Row0, rT.Column2Row0, rTT.X,
                rT.Column0Row1, rT.Column1Row1, rT.Column2Row1, rTT.Y,
                rT.Column0Row2, rT.Column1Row2, rT.Column2Row2, rTT.Z,
                0.0, 0.0, 0.0, 1.0);
        }

        /// <summary>
        /// Returns the inverse of a general 4x4 matrix.
        /// </summary>
        /// <remarks>
        /// The matrix is inverted using Cramer's Rule.  If the 
        /// determinant is zero, the matrix can not be inverted, and
        /// an exception is thrown.
        /// <p />
        /// If the matrix is a transformation matrix of the form
        /// initialized with <see cref="Matrix4D(Matrix3D,Vector3D)" />,
        /// it is more efficient to invert it with 
        /// <see cref="InverseTransformation" />.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// This matrix is not invertible because its determinate is zero.
        /// </exception>
        /// <returns>The inverse of the matrix</returns>
        /// <seealso cref="Matrix4D(Matrix3D,Vector3D)" />
        /// <seealso cref="InverseTransformation" />
        public Matrix4D Inverse()
        {
            //
            // Ported from:
            //   ftp://download.intel.com/design/PentiumIII/sml/24504301.pdf
            //

            Matrix4D inverse = new Matrix4D();
            double[] dst = inverse._values;

            double[] tmp = new double[12];
            double[] src = new double[16];
            double det;

            // transpose matrix
            for (int i = 0; i < 4; i++) 
            {
                src[i]      = _values[i * 4];
                src[i + 4]  = _values[i * 4 + 1];
                src[i + 8]  = _values[i * 4 + 2];
                src[i + 12] = _values[i * 4 + 3];
            }

            // calculate pairs for first 8 elements (cofactors)
            tmp[0]  = src[10] * src[15];
            tmp[1]  = src[11] * src[14];
            tmp[2]  = src[9]  * src[15];
            tmp[3]  = src[11] * src[13];
            tmp[4]  = src[9]  * src[14];
            tmp[5]  = src[10] * src[13];
            tmp[6]  = src[8]  * src[15];
            tmp[7]  = src[11] * src[12];
            tmp[8]  = src[8]  * src[14];
            tmp[9]  = src[10] * src[12];
            tmp[10] = src[8]  * src[13];
            tmp[11] = src[9]  * src[12];

            // calculate first 8 elements (cofactors)
            dst[0]  = tmp[0] * src[5] + tmp[3] * src[6] + tmp[4]  * src[7];
            dst[0] -= tmp[1] * src[5] + tmp[2] * src[6] + tmp[5]  * src[7];
            dst[1]  = tmp[1] * src[4] + tmp[6] * src[6] + tmp[9]  * src[7];
            dst[1] -= tmp[0] * src[4] + tmp[7] * src[6] + tmp[8]  * src[7];
            dst[2]  = tmp[2] * src[4] + tmp[7] * src[5] + tmp[10] * src[7];
            dst[2] -= tmp[3] * src[4] + tmp[6] * src[5] + tmp[11] * src[7];
            dst[3]  = tmp[5] * src[4] + tmp[8] * src[5] + tmp[11] * src[6];
            dst[3] -= tmp[4] * src[4] + tmp[9] * src[5] + tmp[10] * src[6];
            dst[4]  = tmp[1] * src[1] + tmp[2] * src[2] + tmp[5]  * src[3];
            dst[4] -= tmp[0] * src[1] + tmp[3] * src[2] + tmp[4]  * src[3];
            dst[5]  = tmp[0] * src[0] + tmp[7] * src[2] + tmp[8]  * src[3];
            dst[5] -= tmp[1] * src[0] + tmp[6] * src[2] + tmp[9]  * src[3];
            dst[6]  = tmp[3] * src[0] + tmp[6] * src[1] + tmp[11] * src[3];
            dst[6] -= tmp[2] * src[0] + tmp[7] * src[1] + tmp[10] * src[3];
            dst[7]  = tmp[4] * src[0] + tmp[9] * src[1] + tmp[10] * src[2];
            dst[7] -= tmp[5] * src[0] + tmp[8] * src[1] + tmp[11] * src[2];

            // calculate pairs for second 8 elements (cofactors)
            tmp[0]  = src[2] * src[7];
            tmp[1]  = src[3] * src[6];
            tmp[2]  = src[1] * src[7];
            tmp[3]  = src[3] * src[5];
            tmp[4]  = src[1] * src[6];
            tmp[5]  = src[2] * src[5];
            tmp[6]  = src[0] * src[7];
            tmp[7]  = src[3] * src[4];
            tmp[8]  = src[0] * src[6];
            tmp[9]  = src[2] * src[4];
            tmp[10] = src[0] * src[5];
            tmp[11] = src[1] * src[4];

            // calculate second 8 elements (cofactors)
            dst[8]   = tmp[0]  * src[13] + tmp[3]  * src[14] + tmp[4]  * src[15];
            dst[8]  -= tmp[1]  * src[13] + tmp[2]  * src[14] + tmp[5]  * src[15];
            dst[9]   = tmp[1]  * src[12] + tmp[6]  * src[14] + tmp[9]  * src[15];
            dst[9]  -= tmp[0]  * src[12] + tmp[7]  * src[14] + tmp[8]  * src[15];
            dst[10]  = tmp[2]  * src[12] + tmp[7]  * src[13] + tmp[10] * src[15];
            dst[10] -= tmp[3]  * src[12] + tmp[6]  * src[13] + tmp[11] * src[15];
            dst[11]  = tmp[5]  * src[12] + tmp[8]  * src[13] + tmp[11] * src[14];
            dst[11] -= tmp[4]  * src[12] + tmp[9]  * src[13] + tmp[10] * src[14];
            dst[12]  = tmp[2]  * src[10] + tmp[5]  * src[11] + tmp[1]  * src[9];
            dst[12] -= tmp[4]  * src[11] + tmp[0]  * src[9] +  tmp[3]  * src[10];
            dst[13]  = tmp[8]  * src[11] + tmp[0]  * src[8] +  tmp[7]  * src[10];
            dst[13] -= tmp[6]  * src[10] + tmp[9]  * src[11] + tmp[1]  * src[8];
            dst[14]  = tmp[6]  * src[9] +  tmp[11] * src[11] + tmp[3]  * src[8];
            dst[14] -= tmp[10] * src[11] + tmp[2]  * src[8] +  tmp[7]  * src[9];
            dst[15]  = tmp[10] * src[10] + tmp[4]  * src[8] +  tmp[9]  * src[9];
            dst[15] -= tmp[8]  * src[9] +  tmp[11] * src[10] + tmp[5]  * src[8];

            // calculate determinant
            det = src[0] * dst[0] + src[1] * dst[1] + src[2] * dst[2] + src[3] * dst[3];

            if (Math.Abs(det) < 0.00000000000000000001)
            {
                throw new InvalidOperationException("This matrix is not invertible because its determinate is zero.");
            }

            // calculate matrix inverse
            det = 1.0 / det;
            for (int j = 0; j < 16; j++)
            {
                dst[j] *= det;
            }

            return inverse;
        }

        /// <summary>
        /// Gets the upper left 3x3 rotation matrix, assuming
        /// the matrix is a transformation matrix of the form
        /// initialized with <see cref="Matrix4D(Matrix3D,Vector3D)" />.
        /// </summary>
        /// <seealso cref="Matrix4D(Matrix3D,Vector3D)" />
        public Matrix3D Rotation
        {
            get
            {
                return new Matrix3D(
                    Column0Row0, Column1Row0, Column2Row0,
                    Column0Row1, Column1Row1, Column2Row1,
                    Column0Row2, Column1Row2, Column2Row2);
            }
        }

        /// <summary>
        /// Returns the transpose of the upper left 3x3 rotation 
        /// matrix, assuming the matrix is a transformation matrix 
        /// of the form initialized with 
        /// <see cref="Matrix4D(Matrix3D,Vector3D)" />.
        /// </summary>
        /// <remarks>
        /// The tranpose of a rotation matrix is also its inverse.
        /// </remarks>
        /// <returns>
        /// The transpose of the upper left 3x3 rotation matrix.
        /// </returns>
        /// <seealso cref="Matrix4D(Matrix3D,Vector3D)" />
        public Matrix3D RotationTranspose()
        {
            return new Matrix3D(
                Column0Row0, Column0Row1, Column0Row2,
                Column1Row0, Column1Row1, Column1Row2,
                Column2Row0, Column2Row1, Column2Row2);
        }

        /// <summary>
        /// Gets the upper right 3D translation, assuming the 
        /// matrix is a transformation matrix of the form initialized with 
        /// <see cref="Matrix4D(Matrix3D,Vector3D)" />.
        /// </summary>
        /// <seealso cref="Matrix4D(Matrix3D,Vector3D)" />
        public Vector3D Translation
        {
            get
            {
                return new Vector3D(Column3Row0, Column3Row1, Column3Row2);
            }
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
            if (Matrix4D.ReferenceEquals(left, null))
            {
                throw new ArgumentNullException("left");
            }

            if (Matrix4D.ReferenceEquals(right, null))
            {
                throw new ArgumentNullException("right");
            }

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
            if (Matrix4D.ReferenceEquals(matrix, null))
            {
                throw new ArgumentNullException("matrix");
            }

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

        public bool EqualsEpsilon(Matrix4D other, double epsilon)
        {
            if (Matrix4D.ReferenceEquals(other, null))
            {
                return false;
            }

            for (int i = 0; i < _values.Length; ++i)
            {
                if (Math.Abs(_values[i] - other._values[i]) > epsilon)
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
