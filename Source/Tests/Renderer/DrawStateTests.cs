#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using NUnit.Framework;
using System.Drawing;
using System.Collections.Generic;
using System;

namespace OpenGlobe.Renderer
{
    [TestFixture]
    public class DrawStateTests
    {
        /// <summary>
        /// Example code in the book.
        /// </summary>
        private static int CompareDrawStates(DrawState left, DrawState right)
        {
            // Sort by shader first
            int leftShader = left.ShaderProgram.GetHashCode();
            int rightShader = right.ShaderProgram.GetHashCode();

            if (leftShader < rightShader)
            {
                return -1;
            }
            else if (leftShader > rightShader)
            {
                return 1;
            }

            // Shaders are equal, compare depth test enabled
            int leftEnabled = Convert.ToInt32(left.RenderState.DepthTest.Enabled);
            int rightEnabled = Convert.ToInt32(right.RenderState.DepthTest.Enabled);
                
            if (leftEnabled < rightEnabled)
            {
                return -1;
            }
            else if (rightEnabled > leftEnabled)
            {
                return 1;
            }

            // Continue comparing other states in order of most to least expensive...
            return 0;
        }

        [Test]
        public void Sort()
        {
            List<DrawState> drawStates = new List<DrawState>();

            drawStates.Sort(CompareDrawStates);
        }
    }
}
