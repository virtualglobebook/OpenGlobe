#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Globalization;

namespace MiniGlobe.Core
{
    public enum IntervalEndPoint
    {
        Open,
        Closed
    }

    public struct Interval : IEquatable<Interval>
    {
        public Interval(double minimum, double maximum)
            : this(minimum, maximum, IntervalEndPoint.Closed, IntervalEndPoint.Closed)
        {
        }

        public Interval(double minimum, double maximum, IntervalEndPoint minimumEndPoint, IntervalEndPoint maximumEndPoint)
        {
            if (maximum < minimum)
            {
                throw new ArgumentException("maximum < minimum");
            }

            _minimum = minimum;
            _maximum = maximum;
            _minimumEndPoint = minimumEndPoint;
            _maximumEndPoint = maximumEndPoint;
        }

        public double Minimum { get { return _minimum; } }
        public double Maximum { get { return _maximum; } }
        public IntervalEndPoint MinimumEndPoint { get { return _minimumEndPoint; } }
        public IntervalEndPoint MaximumEndPoint { get { return _maximumEndPoint; } }

        public bool Contains(double value)
        {
            bool satisfiesMinimum = (_minimumEndPoint == IntervalEndPoint.Closed) ? (value >= _minimum) : (value > _minimum);
            bool satisfiesMaximum = (_maximumEndPoint == IntervalEndPoint.Closed) ? (value <= _maximum) : (value < _maximum);

            return satisfiesMinimum && satisfiesMaximum;
        }

        public static bool operator ==(Interval left, Interval right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Interval left, Interval right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "{0}{1}, {2},{3}",
                _minimumEndPoint == IntervalEndPoint.Closed ? '[' : '(', _minimum,
                _maximum, _maximumEndPoint == IntervalEndPoint.Closed ? ']' : ')');
        }

        public override int GetHashCode()
        {
            return _minimum.GetHashCode() ^ _maximum.GetHashCode() ^ _minimumEndPoint.GetHashCode() ^ _maximumEndPoint.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Interval))
                return false;

            return this.Equals((Interval)obj);
        }

        #region IEquatable<Interval> Members

        public bool Equals(Interval other)
        {
            return
                (_minimum == other._minimum) &&
                (_maximum == other._maximum) &&
                (_minimumEndPoint == other._minimumEndPoint) &&
                (_maximumEndPoint == other._maximumEndPoint);
        }

        #endregion

        private readonly double _minimum;
        private readonly double _maximum;
        private readonly IntervalEndPoint _minimumEndPoint;
        private readonly IntervalEndPoint _maximumEndPoint;
    }
}
