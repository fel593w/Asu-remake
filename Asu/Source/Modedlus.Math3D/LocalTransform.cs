using System.Numerics;
using Modedlus.Cli;

namespace Modedlus.Math3D;

[CliConstructible]
public class LocalTransform : Transform
{
    // Transform Properties
    public Vector3 Position {get; set;}= new Vector3();
    public Quaternion Rotation {get; set;} = new Quaternion();
    public Vector3 Scale {get; set;}= new Vector3();

    // Vector Rotations
    public Vector3 Up => Vector3.Transform(Vector3.UnitY, Matrix4x4.CreateFromQuaternion(Rotation));
    
    public Vector3 Right => Vector3.Transform(Vector3.UnitX, Matrix4x4.CreateFromQuaternion(Rotation));
    
    public Vector3 Forward => Vector3.Transform(Vector3.UnitZ, Matrix4x4.CreateFromQuaternion(Rotation));
    

    #region Matrix Constructors

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

    #endregion

}