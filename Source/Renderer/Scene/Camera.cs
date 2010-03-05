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
using System.Xml;
using MiniGlobe.Core;
using MiniGlobe.Core.Geometry;

namespace MiniGlobe.Renderer
{
    public class Camera
    {
        public Camera()
        {
            Eye = -Vector3D.UnitY;
            Target = Vector3D.Zero;
            Up = Vector3D.UnitZ;

            FieldOfViewY = Math.PI / 6.0;
            AspectRatio = 1;

            PerspectiveNearPlaneDistance = 0.01;
            PerspectiveFarPlaneDistance = 64;

            OrthographicNearPlaneDistance = 0;
            OrthographicFarPlaneDistance = 1;

            OrthographicLeft = 0;
            OrthographicRight = 1;

            OrthographicBottom = 0;
            OrthographicTop = 1;
        }

        public Vector3D Eye { get; set; }
        public Vector3D Target { get; set; }
        public Vector3D Up { get; set; }

        public Vector3D Forward
        {
            get { return (Target - Eye).Normalize(); }
        }

        public Vector3D Right
        {
            get { return Forward.Cross(Up).Normalize(); }
        }

        public double FieldOfViewX
        {
            get { return (2.0 * Math.Atan(AspectRatio * Math.Tan(FieldOfViewY * 0.5))); }
        }
        public double FieldOfViewY { get; set; }
        public double AspectRatio { get; set; }

        public double PerspectiveNearPlaneDistance { get; set; }
        public double PerspectiveFarPlaneDistance { get; set; }

        public double OrthographicNearPlaneDistance { get; set; }
        public double OrthographicFarPlaneDistance { get; set; }
        public double OrthographicDepth
        {
            get { return Math.Abs(OrthographicFarPlaneDistance - OrthographicNearPlaneDistance); }
        }
        public double OrthographicLeft { get; set; }
        public double OrthographicRight { get; set; }
        public double OrthographicBottom { get; set; }
        public double OrthographicTop { get; set; }

        public void ZoomToTarget(double radius)
        {
            Vector3D toEye = (Eye - Target).Normalize();

            double sin = Math.Sin(Math.Min(FieldOfViewX, FieldOfViewY) * 0.5);
            double distance = (radius / sin);
            Eye = Target + (distance * toEye);
        }

        public void SaveView(string filename)
        {
            XmlDocument xmlDocument = new XmlDocument();

            XmlDeclaration xmlDeclaration = xmlDocument.CreateXmlDeclaration("1.0", "utf-8", null);
            xmlDocument.InsertBefore(xmlDeclaration, xmlDocument.DocumentElement);

            XmlElement cameraNode = xmlDocument.CreateElement("Camera");
            xmlDocument.AppendChild(cameraNode);

            SaveVector(xmlDocument, "Eye", Eye, cameraNode);
            SaveVector(xmlDocument, "Target", Target, cameraNode);
            SaveVector(xmlDocument, "Up", Up, cameraNode);

            xmlDocument.Save(filename);
        }

        static private void SaveVector(XmlDocument xmlDocument, string name, Vector3D value, XmlElement parentNode)
        {
            XmlElement node = xmlDocument.CreateElement(name);
            node.AppendChild(xmlDocument.CreateTextNode(value.X + " " + value.Y + " " + value.Z));
            parentNode.AppendChild(node);
        }

        public void LoadView(string filename)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(filename);
            XmlNodeList nodeList = xmlDocument.GetElementsByTagName("Camera");
            
            string[] eye = nodeList[0].ChildNodes[0].InnerText.Split(new[] { ' ' });
            Eye = new Vector3D(
                Convert.ToDouble(eye[0], CultureInfo.InvariantCulture), 
                Convert.ToDouble(eye[1], CultureInfo.InvariantCulture), 
                Convert.ToDouble(eye[2], CultureInfo.InvariantCulture));
            string[] target = nodeList[0].ChildNodes[1].InnerText.Split(new[] { ' ' });
            Target = new Vector3D(
                Convert.ToDouble(target[0], CultureInfo.InvariantCulture),
                Convert.ToDouble(target[1], CultureInfo.InvariantCulture),
                Convert.ToDouble(target[2], CultureInfo.InvariantCulture));
            string[] up = nodeList[0].ChildNodes[2].InnerText.Split(new[] { ' ' });
            Up = new Vector3D(
                Convert.ToDouble(up[0], CultureInfo.InvariantCulture),
                Convert.ToDouble(up[1], CultureInfo.InvariantCulture),
                Convert.ToDouble(up[2], CultureInfo.InvariantCulture));
        }

        // TODO:  This is not accurate
        public double Altitude(Ellipsoid shape)
        {
            return Eye.Magnitude - shape.MinimumRadius;
        }
    }
}
