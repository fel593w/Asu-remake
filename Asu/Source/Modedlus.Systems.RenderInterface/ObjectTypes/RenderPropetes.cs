using System.Numerics;

namespace Modedlus.Systems.RenderInterface;

public class RenderPropetes
{

    public Dictionary<string, RenderProp> Values;

    public void SetUniform(string name, uint slot, float value) => Values.Add(name, new RenderProp{value = value, slot = slot, Type = RenderPropType._float});
    public void SetUniform(string name, uint slot, int value) => Values.Add(name, new RenderProp{value = value, slot = slot, Type = RenderPropType._int});
    public void SetUniform(string name, uint slot, Matrix4x4 value) => Values.Add(name, new RenderProp{value = value, slot = slot, Type = RenderPropType._M4x4});
    public void SetUniform(string name, uint slot, Matrix3x2 value) => Values.Add(name, new RenderProp{value = value, slot = slot, Type = RenderPropType._M3x2});
    public void SetUniform(string name, uint slot, GPUTexture value) => Values.Add(name, new RenderProp{value = value, slot = slot, Type = RenderPropType._textures});

   // public GPUShader shader { get; set; }
}

public struct RenderProp
{
    public object value;
    public uint slot;
    public RenderPropType Type;
}

public enum RenderPropType
{
    _null,
    _float,
    _int,
    _M4x4,
    _M3x2,
    _textures

}