#region License
//
// (C) Copyright 2011 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using OpenGlobe.Renderer;
using OpenTK.Graphics.OpenGL;

namespace OpenGlobe.Renderer.GL3x
{
    internal class QueryGL3x : Query
    {
        public QueryGL3x(QueryType queryType)
            : base(queryType)
        {
            _name = new QueryNameGL3x();
        }

        public void Begin()
        {
            GL.BeginQuery(TypeConverterGL3x.To(QueryType), _name.Value);
        }

        public void End()
        {
            GL.EndQuery(TypeConverterGL3x.To(QueryType));
        }

        #region Query Members

        public override long Result()
        {
            // TODO:  Use GL.GetQueryObjecti64 when it works in OpenTK
            int value;
            GL.GetQueryObject(_name.Value, GetQueryObjectParam.QueryResult, out value);
            return value;
        }

        public override bool IsResultAvailable()
        {
            int value;
            GL.GetQueryObject(_name.Value, GetQueryObjectParam.QueryResultAvailable, out value);
            return (value != 0);
        }

        #endregion

        #region Disposable Members

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _name.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion

        private QueryNameGL3x _name;
    }
}
