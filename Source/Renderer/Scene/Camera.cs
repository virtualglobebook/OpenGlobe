#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.IO;
using System.Xml;
using System.Globalization;
using OpenTK;

namespace MiniGlobe.Renderer
{
    public class Camera
    {
        public Camera()
        {
            Eye = -Vector3d.UnitY;
            Target = Vector3d.Zero;
            Up = Vector3d.UnitZ;

            FieldOfViewY = Math.PI / 6.0;
            AspectRatio = 1;

            PerspectiveNearPlaneDistance = 0.01;
            PerspectiveFarPlaneDistance = 64;

            OrthographicNearPlaneDistance = 0;
            OrthographicFarPlaneDistance = 1;
        }

        public Vector3d Eye { get; set; }
        public Vector3d Target { get; set; }
        public Vector3d Up { get; set; }

        public Vector3d Forward
        {
            get { return Vector3d.Normalize(new Vector3d(Target - Eye)); }
        }

        public Vector3d Right
        {
            get { return Vector3d.Normalize(Vector3d.Cross(Forward, Up)); }
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

        public void ZoomToTarget(double radius)
        {
            Vector3d toEye = Vector3d.Normalize(Eye - Target);

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

        static private void SaveVector(XmlDocument xmlDocument, string name, Vector3d value, XmlElement parentNode)
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
            Eye = new Vector3d(
                Convert.ToDouble(eye[0], CultureInfo.InvariantCulture), 
                Convert.ToDouble(eye[1], CultureInfo.InvariantCulture), 
                Convert.ToDouble(eye[2], CultureInfo.InvariantCulture));
            string[] target = nodeList[0].ChildNodes[1].InnerText.Split(new[] { ' ' });
            Target = new Vector3d(
                Convert.ToDouble(target[0], CultureInfo.InvariantCulture),
                Convert.ToDouble(target[1], CultureInfo.InvariantCulture),
                Convert.ToDouble(target[2], CultureInfo.InvariantCulture));
            string[] up = nodeList[0].ChildNodes[2].InnerText.Split(new[] { ' ' });
            Up = new Vector3d(
                Convert.ToDouble(up[0], CultureInfo.InvariantCulture),
                Convert.ToDouble(up[1], CultureInfo.InvariantCulture),
                Convert.ToDouble(up[2], CultureInfo.InvariantCulture));
        }
    }
}
