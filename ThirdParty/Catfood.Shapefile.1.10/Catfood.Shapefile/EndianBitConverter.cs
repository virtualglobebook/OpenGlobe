/* ------------------------------------------------------------------------
 * (c)copyright 2009 Catfood Software - http://www.catfood.net
 * Provided under the ms-PL license, see LICENSE.txt
 * ------------------------------------------------------------------------ */

using System;
using System.Collections.Generic;
using System.Text;

namespace Catfood.Shapefile
{
    /// <summary>
    /// The order of bytes provided to EndianBitConverter
    /// </summary>
    public enum ProvidedOrder
    {
        /// <summary>
        /// Value is stored as big-endian
        /// </summary>
        Big,

        /// <summary>
        /// Value is stored as little-endian
        /// </summary>
        Little
    }

    /// <summary>
    /// BitConverter methods that allow a different source byte order (only a subset of BitConverter)
    /// </summary>
    public static class EndianBitConverter
    {
        /// <summary>
        /// Returns an integer from four bytes of a byte array
        /// </summary>
        /// <param name="value">bytes to convert</param>
        /// <param name="startIndex">start index in value</param>
        /// <param name="order">byte order of value</param>
        /// <returns>the integer</returns>
        /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
        /// <exception cref="ArgumentException">Thrown if startIndex is invalid</exception>
        public static int ToInt32(byte[] value, int startIndex, ProvidedOrder order)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if ((startIndex + sizeof(int)) > value.Length)
            {
                throw new ArgumentException("startIndex invalid (not enough space in value to extract an integer", "startIndex");
            }

            if (BitConverter.IsLittleEndian && (order == ProvidedOrder.Big))
            {
                byte[] toConvert = new byte[sizeof(int)];
                Array.Copy(value, startIndex, toConvert, 0, sizeof(int));
                Array.Reverse(toConvert);
                return BitConverter.ToInt32(toConvert, 0);
            }
            else
            {
                return BitConverter.ToInt32(value, startIndex);
            }
        }

        /// <summary>
        /// Returns a double from eight bytes of a byte array
        /// </summary>
        /// <param name="value">bytes to convert</param>
        /// <param name="startIndex">start index in value</param>
        /// <param name="order">byte order of value</param>
        /// <returns>the double</returns>
        /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
        /// <exception cref="ArgumentException">Thrown if startIndex is invalid</exception>
        public static double ToDouble(byte[] value, int startIndex, ProvidedOrder order)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if ((startIndex + sizeof(double)) > value.Length)
            {
                throw new ArgumentException("startIndex invalid (not enough space in value to extract a double", "startIndex");
            }

            if (BitConverter.IsLittleEndian && (order == ProvidedOrder.Big))
            {
                byte[] toConvert = new byte[sizeof(double)];
                Array.Copy(value, startIndex, toConvert, 0, sizeof(double));
                Array.Reverse(toConvert);
                return BitConverter.ToDouble(toConvert, 0);
            }
            else
            {
                return BitConverter.ToDouble(value, startIndex);
            }
        }
    }
}
