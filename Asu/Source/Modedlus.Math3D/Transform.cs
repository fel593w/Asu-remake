using System.Numerics;

namespace Modedlus.Math3D;

public interface Transform
{
    Vector3 Position {get; set;}
    Quaternion Rotation { get; set;}
    Vector3 Scale {get; set;}
    Vector3 Forward {get;}
    Vector3 Up {get;}
    Vector3 Right {get;}

    public Matrix4x4 GetTransformMatrix(Transform transform)
    {
        return Matrix4x4.CreateScale(transform.Scale) *
                Matrix4x4.CreateFromQuaternion(Rotation) *
               Matrix4x4.CreateTranslation(transform.Position);
    } 

    public Matrix4x4 GetRotationMatrix()
    {
        return Matrix4x4.CreateFromQuaternion(Rotation);
    }

    public Matrix4x4 GetScaleMatrix()
    {
        return Matrix4x4.CreateScale(Scale);
    }

    public Matrix4x4 GetTranslationMatrix()
    {
        return Matrix4x4.CreateTranslation(Position);
    } 
}