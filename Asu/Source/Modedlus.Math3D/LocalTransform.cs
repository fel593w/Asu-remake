using System.Numerics;
using Modedlus.Cli;

namespace Modedlus.Math3D;

[CliConstructible]
public class LocalTransform : Transform
{
    // Transform Properties
    public Vector3 Position {get; set;}= new Vector3();
    public Vector3 Rotation {get; set;} = new Vector3();
    public Vector3 Scale {get; set;}= new Vector3();

    // Vector Rotations
    public Vector3 Up {get => Vector3.Transform(Vector3.UnitY, GetRotationMatrix()); set { 
        Rotation = new Vector3((float)Math.Asin(value.Y), (float)Math.Asin(value.X), (float)Math.Asin(value.Z)); } 
    }
    public Vector3 Right{get => Vector3.Transform(Vector3.UnitX, GetRotationMatrix()); set { 
        Rotation = new Vector3((float)Math.Asin(value.Z), (float)Math.Asin(value.Y), (float)Math.Asin(value.X)); } 
    }
    public Vector3 Forward {get => Vector3.Transform(-Vector3.UnitZ, GetRotationMatrix()); set { 
        Rotation = new Vector3((float)Math.Asin(value.X), (float)Math.Asin(value.Z), (float)Math.Asin(value.Y)); } 
    }

    #region Matrix Constructors

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

    #endregion

}