#region License
//
// (C) Copyright 2011 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using OpenGlobe.Core;

namespace OpenGlobe.Renderer
{
    public abstract class Query : Disposable
    {
        internal Query(QueryType queryType)
        {
            _queryType = queryType;
        }

        public QueryType QueryType
        {
            get { return _queryType; }
        }

        public abstract long Result();
        public abstract bool IsResultAvailable();

        private readonly QueryType _queryType;
    }
}
