#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

namespace OpenGlobe.Renderer
{
    public enum UniformType
    {
        Int,
        Float,
        FloatVector2,
        FloatVector3,
        FloatVector4,
        IntVector2,
        IntVector3,
        IntVector4,
        Bool,
        BoolVector2,
        BoolVector3,
        BoolVector4,
        FloatMatrix22,
        FloatMatrix33,
        FloatMatrix44,
        Sampler1D,
        Sampler2D,
        Sampler2DRectangle,
        Sampler2DRectangleShadow,
        Sampler3D,
        SamplerCube,
        Sampler1DShadow,
        Sampler2DShadow,
        FloatMatrix23,
        FloatMatrix24,
        FloatMatrix32,
        FloatMatrix34,
        FloatMatrix42,
        FloatMatrix43,
        Sampler1DArray,
        Sampler2DArray,
        Sampler1DArrayShadow,
        Sampler2DArrayShadow,
        SamplerCubeShadow,
        IntSampler1D,
        IntSampler2D,
        IntSampler2DRectangle,
        IntSampler3D,
        IntSamplerCube,
        IntSampler1DArray,
        IntSampler2DArray,
        UnsignedIntSampler1D,
        UnsignedIntSampler2D,
        UnsignedIntSampler2DRectangle,
        UnsignedIntSampler3D,
        UnsignedIntSamplerCube,
        UnsignedIntSampler1DArray,
        UnsignedIntSampler2DArray
    }

    public class Uniform
    {
        protected Uniform(string name, UniformType type)
        {
            _name = name;
            _type = type;
        }

        public string Name
        {
            get { return _name; }
        }

        public UniformType Datatype
        {
            get { return _type; }
        }

        private readonly string _name;
        private readonly UniformType _type;
    }

    public abstract class Uniform<T> : Uniform
    {
        protected Uniform(string name, UniformType type)
            : base(name, type)
        {
        }

        public abstract T Value { set; get; }
    }
}
