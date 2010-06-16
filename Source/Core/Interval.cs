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

namespace OpenGlobe.Core
{
    public enum IntervalEndpoint
    {
        Open,
        Closed
    }

    public struct Interval : IEquatable<Interval>
    {
        public Interval(double minimum, double maximum)
            : this(minimum, maximum, IntervalEndpoint.Closed, IntervalEndpoint.Closed)
        {
        }

        public Interval(double minimum, double maximum, IntervalEndpoint minimumEndpoint, IntervalEndpoint maximumEndpoint)
        {
            if (maximum < minimum)
            {
                throw new ArgumentException("maximum < minimum");
            }

            _minimum = minimum;
            _maximum = maximum;
            _minimumEndpoint = minimumEndpoint;
            _maximumEndpoint = maximumEndpoint;
        }

        public double Minimum { get { return _minimum; } }
        public double Maximum { get { return _maximum; } }
        public IntervalEndpoint MinimumEndpoint { get { return _minimumEndpoint; } }
        public IntervalEndpoint MaximumEndpoint { get { return _maximumEndpoint; } }

        public bool Contains(double value)
        {
            bool satisfiesMinimum = (_minimumEndpoint == IntervalEndpoint.Closed) ? (value >= _minimum) : (value > _minimum);
            bool satisfiesMaximum = (_maximumEndpoint == IntervalEndpoint.Closed) ? (value <= _maximum) : (value < _maximum);

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
                _minimumEndpoint == IntervalEndpoint.Closed ? '[' : '(', _minimum,
                _maximum, _maximumEndpoint == IntervalEndpoint.Closed ? ']' : ')');
        }

        public override int GetHashCode()
        {
            return _minimum.GetHashCode() ^ _maximum.GetHashCode() ^ _minimumEndpoint.GetHashCode() ^ _maximumEndpoint.GetHashCode();
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
                (_minimumEndpoint == other._minimumEndpoint) &&
                (_maximumEndpoint == other._maximumEndpoint);
        }

        #endregion

        private readonly double _minimum;
        private readonly double _maximum;
        private readonly IntervalEndpoint _minimumEndpoint;
        private readonly IntervalEndpoint _maximumEndpoint;
    }
}
