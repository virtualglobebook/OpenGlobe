#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.IO;
using System.Collections.Generic;
using System.Collections;

namespace OpenGlobe.Core
{
    public class Shapefile
    {
        public Shapefile(string filename)
        {
            //
            // ESRI Shapefile Technical Description:
            //    http://www.esri.com/library/whitepapers/pdfs/shapefile.pdf
            //
            using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                //
                // File Header:
                //
                // Position  Field         Value        Type     Byte Order
                // --------  -----         -----        ----     ----------
                // Byte 0    File Code     9994         Integer  Big
                // Byte 4    Unused        0            Integer  Big
                // Byte 8    Unused        0            Integer  Big
                // Byte 12   Unused        0            Integer  Big
                // Byte 16   Unused        0            Integer  Big
                // Byte 20   Unused        0            Integer  Big
                // Byte 24   File Length   File Length  Integer  Big
                // Byte 28   Version       1000         Integer  Little
                // Byte 32   Shape Type    Shape Type   Integer  Little
                // Byte 36   Bounding Box  Xmin         Double   Little
                // Byte 44   Bounding Box  Ymin         Double   Little
                // Byte 52   Bounding Box  Xmax         Double   Little
                // Byte 60   Bounding Box  Ymax         Double   Little
                // Byte 68*  Bounding Box  Zmin         Double   Little
                // Byte 76*  Bounding Box  Zmax         Double   Little
                // Byte 84*  Bounding Box  Mmin         Double   Little
                // Byte 92*  Bounding Box  Mmax         Double   Little
                //
                // * Unused, with value 0.0, if not Measured or Z type
                //
                byte[] fileHeader = Read(fs, _fileHeaderLength);
                if (fileHeader == null)
                {
                    throw new InvalidDataException("Could not read shapefile header.");
                }

                int fileCode = ToInteger(fileHeader, 0, ByteOrder.BigEndian);
                if (fileCode != _fileCode)
                {
                    //
                    // Swap byte order and try again.
                    //
                    _byteOrder = ByteOrder.BigEndian;

                    fileCode = ToInteger(fileHeader, 0, ByteOrder.BigEndian);

                    if (fileCode != _fileCode)
                    {
                        throw new InvalidDataException("Could not read shapefile file code.  Is this a valid shapefile?");
                    }
                }

                int fileLengthInBytes = ToInteger(fileHeader, 24, ByteOrder.BigEndian) * 2;

                int version = ToInteger(fileHeader, 28, ByteOrder.LittleEndian);
                if (version != _version)
                {
                    throw new InvalidDataException("Shapefile version " + version + " is not supported.  Only version " + _version + " is supported.");
                }

                _shapeType = (ShapeType)ToInteger(fileHeader, 32, ByteOrder.LittleEndian);

                double xMin = ToDouble(fileHeader, 36, ByteOrder.LittleEndian);
                double yMin = ToDouble(fileHeader, 44, ByteOrder.LittleEndian);
                double xMax = ToDouble(fileHeader, 52, ByteOrder.LittleEndian);
                double yMax = ToDouble(fileHeader, 60, ByteOrder.LittleEndian);
                // TODO: use these next four
                double zMin = NoDataToZero(ToDouble(fileHeader, 68, ByteOrder.LittleEndian));
                double zMax = NoDataToZero(ToDouble(fileHeader, 76, ByteOrder.LittleEndian));
                double mMin = NoDataToZero(ToDouble(fileHeader, 84, ByteOrder.LittleEndian));
                double mMax = NoDataToZero(ToDouble(fileHeader, 92, ByteOrder.LittleEndian));

                if (fileLengthInBytes == _fileHeaderLength)
                {
                    //
                    // If the shapefile is empty (that is, has no records), 
                    // the values for xMin, yMin, xMax, and yMax are unspecified.
                    //
                    // I like zero better.
                    //
                    xMin = 0.0;
                    yMin = 0.0;
                    xMax = 0.0;
                    yMax = 0.0;
                }

                _extent = new RectangleD(new Vector2D(xMin, yMin), new Vector2D(xMax, yMax));

                //
                // Read each header...
                //

                //
                // Record Header:
                //
                // Position  Field           Value           Type     Byte Order
                // --------  -----           -----           ----     ----------
                // Byte 0    Record Number   Record Number   Integer  Big
                // Byte 4    Content Length  Content Length  Integer  Big
                //
                _shapes = new List<Shape>();
                byte[] recordHeader;

                while ((recordHeader = Read(fs, _recordHeaderLength)) != null)
                {
                    int recordNumber = ToInteger(recordHeader, 0, ByteOrder.BigEndian);
                    int contextLengthInBytes = ToInteger(recordHeader, 4, ByteOrder.BigEndian) * 2;
                    byte[] record = Read(fs, contextLengthInBytes);

                    ShapeType recordShapeType = (ShapeType)ToInteger(record, 0, ByteOrder.LittleEndian);
                    switch (recordShapeType)
                    {
                        case ShapeType.NullShape:
                            //
                            // Filter out null shapes.  Otherwise, every client
                            // would have to deal with them.
                            //
                            break;
                        case ShapeType.Point:
                            _shapes.Add(new PointShape(recordNumber, 
                                new Vector2D(
                                    ToDouble(record, 4, ByteOrder.LittleEndian),
                                    ToDouble(record, 12, ByteOrder.LittleEndian))));
                            break;
                    }
                }
            }
        }

        private int ToInteger(byte[] buffer, int offset, ByteOrder byteOrder)
        {
            if (byteOrder == _byteOrder)
            {
                return BitConverter.ToInt32(buffer, offset);
            }

            byte[] swapped = new byte[4];
            swapped[0] = buffer[offset + 3];
            swapped[1] = buffer[offset + 2];
            swapped[2] = buffer[offset + 1];
            swapped[3] = buffer[offset];

            return BitConverter.ToInt32(swapped, 0);
        }

        private double ToDouble(byte[] buffer, int offset, ByteOrder byteOrder)
        {
            if (byteOrder == _byteOrder)
            {
                return BitConverter.ToDouble(buffer, offset);
            }

            byte[] swapped = new byte[4];
            swapped[0] = buffer[offset + 7];
            swapped[1] = buffer[offset + 6];
            swapped[2] = buffer[offset + 5];
            swapped[3] = buffer[offset + 4];
            swapped[4] = buffer[offset + 3];
            swapped[5] = buffer[offset + 2];
            swapped[6] = buffer[offset + 1];
            swapped[7] = buffer[offset];

            return BitConverter.ToDouble(swapped, 0);
        }

        private double NoDataToZero(double value)
        {
            return (value < _noData) ? 0.0 : value;
        }

        private static byte[] Read(FileStream fileStream, int count)
        {
            byte[] buffer = new byte[count];

            int bytesRemaining = count;
            int offset = 0;

            while (bytesRemaining > 0)
            {
                int bytesRead = fileStream.Read(buffer, offset, bytesRemaining);
                if (bytesRead <= 0)
                {
                    return null;
                }

                offset += bytesRead;
                bytesRemaining -= bytesRead;
            }

            return (bytesRemaining == 0) ? buffer : null;
        }

        public ShapeType ShapeType
        {
            get { return _shapeType; }
        }

        public RectangleD Extent
        {
            get { return _extent; }
        }

        public Shape this[int index] 
        {
            get { return _shapes[index]; }
        }

        public int Count 
        {
            get { return _shapes.Count; }
        }

        public IEnumerator GetEnumerator()
        {
            return _shapes.GetEnumerator();
        }

        private enum ByteOrder
        {
            LittleEndian,
            BigEndian
        }

        private readonly ByteOrder _byteOrder;
        private readonly ShapeType _shapeType;
        private readonly RectangleD _extent;
        private readonly IList<Shape> _shapes;

        private static readonly double _noData = Math.Pow(-10, 38);
        private const int _fileHeaderLength = 100;
        private const int _fileCode = 9994;
        private const int _version = 1000;
        private const int _recordHeaderLength = 8;
    }
}