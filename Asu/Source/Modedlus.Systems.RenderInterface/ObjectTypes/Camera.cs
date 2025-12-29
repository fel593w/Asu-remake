using System.Numerics;
using Modedlus.Math3D;

namespace Modedlus.Systems.RenderInterface;

public struct Camera
{
    public float Fov;
    public float NearClip;
    public float FarClip;

    public Transform transform;

    public Matrix4x4 GetViewMatrix()
    {
        return Matrix4x4.CreateLookAt(transform.Position, transform.Position + transform.Forward, transform.Up);
    }
}