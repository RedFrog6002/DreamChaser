using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamChaser
{
    public static class Extentions
    {
        public static UnityEngine.Vector3 ToU3(this Hkmp.Math.Vector2 vector2) => new UnityEngine.Vector3(vector2.X, vector2.Y, 0f);
        public static UnityEngine.Vector3 ToU3(this Hkmp.Math.Vector3 vector3) => new UnityEngine.Vector3(vector3.X, vector3.Y, vector3.Z);
        public static UnityEngine.Vector2 ToU2(this Hkmp.Math.Vector2 vector2) => new UnityEngine.Vector2(vector2.X, vector2.Y);
        public static UnityEngine.Vector2 ToU2(this Hkmp.Math.Vector3 vector3) => new UnityEngine.Vector2(vector3.X, vector3.Y);
        public static Hkmp.Math.Vector3 ToH3(this UnityEngine.Vector2 vector2) => new Hkmp.Math.Vector3(vector2.x, vector2.y, 0f);
        public static Hkmp.Math.Vector3 ToH3(this UnityEngine.Vector3 vector3) => new Hkmp.Math.Vector3(vector3.x, vector3.y, vector3.z);
        public static Hkmp.Math.Vector2 ToH2(this UnityEngine.Vector2 vector2) => new Hkmp.Math.Vector2(vector2.x, vector2.y);
        public static Hkmp.Math.Vector2 ToH2(this UnityEngine.Vector3 vector3) => new Hkmp.Math.Vector2(vector3.x, vector3.y);
        public static T GetRandom<T>(this List<T> list)
        {
            return list[UnityEngine.Random.Range(0, list.Count)];
        }
    }
}
