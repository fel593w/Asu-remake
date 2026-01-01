using System.Numerics;
using Modedlus.Math3D;

namespace Modedlus.Systems.RenderInterface;

public class PrespectiveCamera : Camera
{
    public Transform location;
    public float NearPlane = 0.01f;
    public float FarPlane = 100;
    public float Fov = 45;
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
            return Matrix4x4.CreatePerspectiveFieldOfView(DegreesToRadians(Fov), aspectRatio, NearPlane, FarPlane);
        }

        public static float DegreesToRadians(float degrees)
        {
            return MathF.PI / 180f * degrees;
        }
}