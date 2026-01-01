using System.Numerics;
using Modedlus.Math3D;

namespace Modedlus.Systems.RenderInterface;

public interface Camera
{
        public Matrix4x4 GetViewMatrix();

        public Matrix4x4 GetProjectionMatrix();



}