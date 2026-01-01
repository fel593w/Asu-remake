using System.Numerics;
using Modedlus.Math3D;

namespace Modedlus.Systems.RenderInterface;

public class OrthographicCamera : Camera
{

    public Transform location;
    public float NearPlane = 0.01f;
    public float FarPlane = 100;
    public float Size;
    public float Width => Size;
    public float Height => Size * aspectRatio;
    public float aspectRatio {get; set;} = 1;

    public void SetAspectRatio(int XRes, int YRes) => SetAspectRatio((float)XRes, (float)YRes);
    public void SetAspectRatio(float X, float Y) => SetAspectRatio(Y/X);
    public void SetAspectRatio(float Value) => aspectRatio = Value;

        public Matrix4x4 GetViewMatrix()
        {
            return Matrix4x4.CreateLookAt(location.Position, location.Position + location.Forward, location.Up);
        }

        public Matrix4x4 GetProjectionMatrix()
        {
            return Matrix4x4.CreateOrthographic(Width, Height, NearPlane, FarPlane);
        }
}