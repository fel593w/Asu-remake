using System.Numerics;

namespace Modedlus.Math3D;

public interface Transform
{
    Vector3 Position {get; set;}
    Vector3 Rotation {get; set;}
    Vector3 Scale {get; set;}
    Vector3 Forward {get; set;}
    Vector3 Up {get; set;}
    Vector3 Right {get; set;}

    public Matrix4x4 GetTransformMatrix(Transform transform)
    {
        return Matrix4x4.CreateScale(transform.Scale) *
               Matrix4x4.CreateRotationX(transform.Rotation.X) *
               Matrix4x4.CreateRotationY(transform.Rotation.Y) *
               Matrix4x4.CreateRotationZ(transform.Rotation.Z) *
               Matrix4x4.CreateTranslation(transform.Position);
    } 

    public Matrix4x4 GetRotationMatrix()
    {
        return Matrix4x4.CreateRotationX(Rotation.X) *
               Matrix4x4.CreateRotationY(Rotation.Y) *
               Matrix4x4.CreateRotationZ(Rotation.Z);
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