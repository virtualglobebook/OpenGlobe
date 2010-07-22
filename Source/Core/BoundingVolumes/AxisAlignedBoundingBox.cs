#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Collections.Generic;

namespace OpenGlobe.Core
{
    public struct AxisAlignedBoundingBox
    {
        public AxisAlignedBoundingBox(IEnumerable<Vector3D> positions)
        {
            if (positions == null)
            {
                throw new ArgumentNullException("positions");
            }

            double minimumX = double.MaxValue;
            double minimumY = double.MaxValue;
            double minimumZ = double.MaxValue;

            double maximumX = -double.MaxValue;
            double maximumY = -double.MaxValue;
            double maximumZ = -double.MaxValue;
            
            foreach (Vector3D position in positions)
            {
                if (position.X < minimumX)
                {
                    minimumX = position.X;
                }

                if (position.X > maximumX)
                {
                    maximumX = position.X;
                }

                if (position.Y < minimumY)
                {
                    minimumY = position.Y;
                }

                if (position.Y > maximumY)
                {
                    maximumY = position.Y;
                }

                if (position.Z < minimumZ)
                {
                    minimumZ = position.Z;
                }

                if (position.Z > maximumZ)
                {
                    maximumZ = position.Z;
                }
            }

            Vector3D minimum = new Vector3D(minimumX, minimumY, minimumZ);
            Vector3D maximum = new Vector3D(maximumX, maximumY, maximumZ);

            if (minimum > maximum)
            {
                Utility.Swap(ref minimum, ref maximum);
            }

            _minimum = minimum;
            _maximum = maximum;
        }

        public Vector3D Minimum
        {
            get { return _minimum; }
        }

        public Vector3D Maximum
        {
            get { return _maximum; }
        }

        public Vector3D Center
        {
            get { return (Minimum + Maximum) * 0.5; }
        }

        private readonly Vector3D _minimum;
        private readonly Vector3D _maximum;
    }
}