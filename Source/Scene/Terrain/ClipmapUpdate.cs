using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenGlobe.Renderer;

namespace OpenGlobe.Scene.Terrain
{
    internal class ClipmapUpdate
    {
        public ClipmapUpdate(ClipmapLevel level, int west, int south, int east, int north)
        {
            _level = level;
            _west = west;
            _south = south;
            _east = east;
            _north = north;
        }

        public ClipmapLevel Level
        {
            get { return _level; }
        }

        public int West
        {
            get { return _west; }
        }

        public int South
        {
            get { return _south; }
        }

        public int East
        {
            get { return _east; }
        }

        public int North
        {
            get { return _north; }
        }

        public int Width
        {
            get { return East - West + 1; }
        }
        
        public int Height
        {
            get { return North - South + 1; }
        }

        /// <summary>
        /// Creates a new region which is equivalent to this one but with a one post buffer added
        /// around the perimeter.
        /// </summary>
        /// <returns>The new region.</returns>
        public ClipmapUpdate AddBuffer()
        {
            return new ClipmapUpdate(_level, _west - 1, _south - 1, _east + 1, _north + 1);
        }

        private ClipmapLevel _level;
        private int _west;
        private int _south;
        private int _east;
        private int _north;
    }
}
