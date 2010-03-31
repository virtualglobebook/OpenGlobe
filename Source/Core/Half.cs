#region License
//
// (C) Copyright 2010 Patrick Cozzi, Deron Ohlarik, and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//

// This type is based on OpenTK.Half in the Open Toolkit Library and is
// governed by the following license:

// Copyright (c) 2006 - 2008 The Open Toolkit library.

// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
// of the Software, and to permit persons to whom the Software is furnished to do
// so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

// The conversion functions are derived from OpenEXR's implementation and are
// governed by the following license:

// Copyright (c) 2002, Industrial Light & Magic, a division of Lucas
// Digital Ltd. LLC

// All rights reserved.

// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are
// met:
// *       Redistributions of source code must retain the above copyright
// notice, this list of conditions and the following disclaimer.
// *       Redistributions in binary form must reproduce the above
// copyright notice, this list of conditions and the following disclaimer
// in the documentation and/or other materials provided with the
// distribution.
// *       Neither the name of Industrial Light & Magic nor the names of
// its contributors may be used to endorse or promote products derived
// from this software without specific prior written permission. 

// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
// THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion

using System;
using System.Globalization;

namespace MiniGlobe.Core
{
    /// <summary>
    /// A half-precision (16-bit) floating-point number.  This format has
    /// 1 sign bit, 5 exponent bits, and 11 significand bits (10 explicitly
    /// stored).  It conforms to the IEEE 754-2008 binary16 format. 
    /// </summary>
    [Serializable]
    public struct Half : IEquatable<Half>, IComparable<Half>
    {
        /// <summary>
        /// Represents a value that is not-a-number (NaN).
        /// </summary>
        public static readonly Half NaN = new Half(0x7C01);

        /// <summary>
        /// Represents positive infinity.
        /// </summary>
        public static readonly Half PositiveInfinity = new Half(31744);

        /// <summary>
        /// Represents negative infinity.
        /// </summary>
        public static readonly Half NegativeInfinity = new Half(64512);

        /// <summary>
        /// Determines if this instance is zero.  It returns <see langword="true" />
        /// if this instance is either positive or negative zero.
        /// </summary>
        public bool IsZero { get { return (_bits == 0) || (_bits == 0x8000); } }

        /// <summary>
        /// Determines if this instance is Not-A-Number (NaN).
        /// </summary>
        public bool IsNaN { get { return (((_bits & 0x7C00) == 0x7C00) && (_bits & 0x03FF) != 0x0000); } }

        /// <summary>
        /// Determines if this instance represents positive infinity.
        /// </summary>
        public bool IsPositiveInfinity { get { return (_bits == 31744); } }

        /// <summary>
        /// Determines if this instance represents negative infinity.
        /// </summary>
        public bool IsNegativeInfinity { get { return (_bits == 64512); } }

        /// <summary>
        /// Determines if this instance represents either positive or negative infinity.
        /// </summary>
        public bool IsInfinity { get { return (_bits & 31744) == 31744; } }

        /// <summary>
        /// Initializes a new instance from a 32-bit single-precision floating-point number.
        /// </summary>
        /// <param name="value">A 32-bit, single-precision floating-point number.</param>
        public Half(float value) : this((double)value)
        {
        }

        /// <summary>
        /// Initializes a new instance from a 64-bit double-precision floating-point number.
        /// </summary>
        /// <param name="value">A 64-bit, double-precision floating-point number.</param>
        public Half(double value)
        {
            _bits = DoubleToHalf(BitConverter.DoubleToInt64Bits(value));
        }

        private Half(ushort value)
        {
            _bits = value;
        }

        /// <summary>
        /// Converts this instance to a 32-bit, single-precision floating-point number.</summary>
        /// <returns>The 32-bit, single-precision floating-point number.</returns>
        public float ToSingle()
        {
            // If it's problematic, this heap allocation can be eliminated by replacing
            // HalfToFloat with HalfToDouble and using BitConverter.Int64BitsToDouble().
            byte[] bytes = BitConverter.GetBytes(HalfToFloat(_bits));
            return BitConverter.ToSingle(bytes, 0);
        }

        /// <summary>
        /// Converts this instance to a 64-bit, double-precision floating-point number.</summary>
        /// <returns>The 64-bit, double-precision floating-point number.</returns>
        public double ToDouble()
        {
            return ToSingle();
        }

        /// <summary>
        /// Converts a 32-bit, single-precision, floating-point number to a 16-bit,
        /// half-precision, floating-point number.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator Half(float value)
        {
            return new Half(value);
        }

        /// <summary>
        /// Converts a 64-bit, double-precision, floating-point number to a 16-bit,
        /// half-precision, floating-point number.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator Half(double value)
        {
            return new Half(value);
        }

        /// <summary>
        /// Converts a 16-bit, half-precision, floating-point number to a 
        /// 32-bit, single-precision, floating-point number.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator float(Half value)
        {
            return value.ToSingle();
        }

        /// <summary>
        /// Converts a 16-bit, half-precision, floating-point number to a
        /// 64-bit, double-precision, floating-point number.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator double(Half value)
        {
            return value.ToDouble();
        }

        private static ushort DoubleToHalf(long bits)
        {
            // Our double-precision floating point number, F, is represented by the bit pattern in long i.
            // Disassemble that bit pattern into the sign, S, the exponent, E, and the significand, M.
            // Shift S into the position where it will go in in the resulting half number.
            // Adjust E, accounting for the different exponent bias of double and half (1023 versus 15).

            int sign = (int)((bits >> 48) & 0x00008000);
            int exponent = (int)(((bits >> 52) & 0x7FF) - (1023 - 15));
            long mantissa = bits & 0xFFFFFFFFFFFFF;

            // Now reassemble S, E and M into a half:

            if (exponent <= 0)
            {
                if (exponent < -10)
                {
                    // E is less than -10. The absolute value of F is less than Half.MinValue
                    // (F may be a small normalized float, a denormalized float or a zero).
                    //
                    // We convert F to a half zero with the same sign as F.

                    return (UInt16)sign;
                }

                // E is between -10 and 0. F is a normalized double whose magnitude is less than Half.MinNormalizedValue.
                //
                // We convert F to a denormalized half.

                // Add an explicit leading 1 to the significand.

                mantissa = mantissa | 0x10000000000000;

                // Round to M to the nearest (10+E)-bit value (with E between -10 and 0); in case of a tie, round to the nearest even value.
                //
                // Rounding may cause the significand to overflow and make our number normalized. Because of the way a half's bits
                // are laid out, we don't have to treat this case separately; the code below will handle it correctly.

                int t = 43 - exponent;
                long a = (1L << (t - 1)) - 1;
                long b = (mantissa >> t) & 1;

                mantissa = (mantissa + a + b) >> t;

                // Assemble the half from S, E (==zero) and M.

                return (ushort)(sign | (int)mantissa);
            }
            else if (exponent == 0x7ff - (1023 - 15))
            {
                if (mantissa == 0)
                {
                    // F is an infinity; convert F to a half infinity with the same sign as F.

                    return (ushort)(sign | 0x7c00);
                }
                else
                {
                    // F is a NAN; we produce a half NAN that preserves the sign bit and the 10 leftmost bits of the
                    // significand of F, with one exception: If the 10 leftmost bits are all zero, the NAN would turn 
                    // into an infinity, so we have to set at least one bit in the significand.

                    int mantissa32 = (int)(mantissa >> 42);
                    return (ushort)(sign | 0x7c00 | mantissa32 | ((mantissa == 0) ? 1 : 0));
                }
            }
            else
            {
                // E is greater than zero.  F is a normalized double. We try to convert F to a normalized half.

                // Round to M to the nearest 10-bit value. In case of a tie, round to the nearest even value.

                mantissa = mantissa + 0x1FFFFFFFFFF + ((mantissa >> 42) & 1);

                if ((mantissa & 0x10000000000000) != 0)
                {
                    mantissa = 0;        // overflow in significand,
                    exponent += 1;        // adjust exponent
                }

                // exponent overflow
                if (exponent > 30) throw new ArithmeticException("Half: Hardware floating-point overflow.");

                // Assemble the half from S, E and M.

                return (ushort)(sign | (exponent << 10) | (int)(mantissa >> 42));
            }
        }

        /// <summary>Ported from OpenEXR's IlmBase 1.0.1</summary>
        private static int HalfToFloat(ushort ui16)
        {
            int sign = (ui16 >> 15) & 0x00000001;
            int exponent = (ui16 >> 10) & 0x0000001f;
            int mantissa = ui16 & 0x000003ff;

            if (exponent == 0)
            {
                if (mantissa == 0)
                {
                    // Plus or minus zero

                    return sign << 31;
                }
                else
                {
                    // Denormalized number -- renormalize it

                    while ((mantissa & 0x00000400) == 0)
                    {
                        mantissa <<= 1;
                        exponent -= 1;
                    }

                    exponent += 1;
                    mantissa &= ~0x00000400;
                }
            }
            else if (exponent == 31)
            {
                if (mantissa == 0)
                {
                    // Positive or negative infinity

                    return (sign << 31) | 0x7f800000;
                }
                else
                {
                    // Nan -- preserve sign and significand bits

                    return (sign << 31) | 0x7f800000 | (mantissa << 13);
                }
            }

            // Normalized number

            exponent = exponent + (127 - 15);
            mantissa = mantissa << 13;

            // Assemble S, E and M.

            return (sign << 31) | (exponent << 23) | mantissa;
        }

        /// <summary>
        /// The smallest positive <see cref="Half"/>.
        /// </summary>
        public static readonly float MinValue = 5.96046448e-08f;

        /// <summary>
        /// The smallest positive normalized <see cref="Half"/>.
        /// </summary>
        public static readonly float MinNormalizedValue = 6.10351562e-05f;

        /// <summary>
        /// The largest positive <see cref="Half"/>.
        /// </summary>
        public static readonly float MaxValue = 65504.0f;

        /// <summary>
        /// Smallest positive value e for which <see cref="Half"/> (1.0 + e) != <see cref="Half"/> (1.0).
        /// </summary>
        public static readonly float Epsilon = 0.00097656f;

        /// <summary>
        /// Returns a value indicating whether this instance is equal to another
        /// instance.
        /// </summary>
        /// <param name="other">The other instance to which to compare this instance.</param>
        /// <returns>
        /// <see langword="true" /> if this instance exactly equals another; otherwise <see langword="false" />.</returns>
        public bool Equals(Half other)
        {
            return ToSingle().Equals(other.ToSingle());
        }

        public override bool Equals(object obj)
        {
            if (obj is Half)
            {
                return Equals((Half)obj);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return _bits.GetHashCode();
        }

        public static bool operator ==(Half left, Half right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Half left, Half right)
        {
            return !left.Equals(right);
        }

        public static bool operator <(Half left, Half right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator <=(Half left, Half right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >(Half left, Half right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator >=(Half left, Half right)
        {
            return left.CompareTo(right) >= 0;
        }

        /// <summary>
        /// Compares this instance to a specified half-precision floating-point number
        /// and returns an integer that indicates whether the value of this instance
        /// is less than, equal to, or greater than the value of the specified half-precision
        /// floating-point number. 
        /// </summary>
        /// <param name="other">A half-precision floating-point number to compare.</param>
        /// <returns>
        /// A signed number indicating the relative values of this instance and value. If the number is:
        /// <para>
        /// Less than zero, then this instance is less than other, or this instance is not a number
        /// (<see cref="NaN"/>) and other is a number.
        /// </para>
        /// <para>
        /// Zero: this instance is equal to value, or both this instance and other
        /// are not a number (<see cref="NaN"/>), <see cref="PositiveInfinity"/>, or
        /// <see cref="NegativeInfinity"/>.
        /// </para>
        /// <para>
        /// Greater than zero: this instance is greater than othrs, or this instance is a number
        /// and other is not a number (<see cref="NaN"/>).
        /// </para>
        /// </returns>
        public int CompareTo(Half other)
        {
            return ToSingle().CompareTo(other.ToSingle());
        }

        /// <summary>Converts this instance into a human-legible string representation.</summary>
        /// <returns>The string representation of this instance.</returns>
        public override string ToString()
        {
            return ToSingle().ToString();
        }

        /// <summary>Converts this instance into a human-legible string representation.</summary>
        /// <param name="format">Formatting for the output string.</param>
        /// <param name="formatProvider">Culture-specific formatting information.</param>
        /// <returns>The string representation of this instance.</returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            return ToSingle().ToString(format, formatProvider);
        }

        /// <summary>Converts the string representation of a number to a half-precision floating-point equivalent.</summary>
        /// <param name="s">String representation of the number to convert.</param>
        /// <returns>A new <see cref="Half"/> instance.</returns>
        public static Half Parse(string s)
        {
            return new Half(Double.Parse(s));
        }

        /// <summary>Converts the string representation of a number to a half-precision floating-point equivalent.</summary>
        /// <param name="s">String representation of the number to convert.</param>
        /// <param name="style">Specifies the format of <paramref name="s"/>.</param>
        /// <param name="provider">Culture-specific formatting information.</param>
        /// <returns>A new <see cref="Half"/> instance.</returns>
        public static Half Parse(string s, NumberStyles style, IFormatProvider provider)
        {
            return new Half(Double.Parse(s, style, provider));
        }

        /// <summary>Converts the string representation of a number to a half-precision floating-point equivalent.</summary>
        /// <param name="s">String representation of the number to convert.</param>
        /// <param name="result">The <see cref="Half"/> instance to write to.</param>
        /// <returns><see langword="true" /> true if parsing succeeded; otherwise <see langword="false"/>.</returns>
        public static bool TryParse(string s, out Half result)
        {
            double f;
            bool b = Double.TryParse(s, out f);
            result = new Half(f);
            return b;
        }

        /// <summary>Converts the string representation of a number to a half-precision floating-point equivalent.</summary>
        /// <param name="s">String representation of the number to convert.</param>
        /// <param name="style">Specifies the format of <paramref name="s"/>.</param>
        /// <param name="provider">Culture-specific formatting information.</param>
        /// <param name="result">The Half instance to write to.</param>
        /// <returns><see langword="true" /> true if parsing succeeded; otherwise <see langword="false"/>.</returns>
        public static bool TryParse(string s, System.Globalization.NumberStyles style, IFormatProvider provider, out Half result)
        {
            double f;
            bool b = Double.TryParse(s, style, provider, out f);
            result = new Half(f);
            return b;
        }

        private ushort _bits;
    }
}
