using UnityEngine;

namespace GameCreator.Runtime.Common
{
    public static class Vector3Utils
    {
        /// <summary>
        /// Projects a point onto a segment and returns a point between A and B that is closest to
        /// the Point.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="pointA"></param>
        /// <param name="pointB"></param>
        /// <returns></returns>
        public static Vector3 OnSegment(this Vector3 point, Vector3 pointA, Vector3 pointB)
        {
            Vector3 direction = pointB - pointA;

            float length = direction.magnitude;
            if (length < float.Epsilon) return pointA;

            direction.Normalize();

            Vector3 directionPA = point - pointA;
            float projection = Vector3.Dot(directionPA, direction);

            projection = Mathf.Clamp(projection, 0f, length);
            return pointA + direction * projection;
        }

        /// <summary>
        /// Returns the projection of a point onto a vector defined by the given direction.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static Vector3 PointOnVector(this Vector3 point, Vector3 direction)
        {
            float scalarProjection = Vector3.Dot(point, direction) / direction.magnitude;
            return scalarProjection * direction;
        }
        
        /// <summary>
        /// Projects a point onto an infinite ray that passes through an origin and extends
        /// on a direction.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="ray"></param>
        /// <returns></returns>
        public static Vector3 ProjectPointOntoRay(this Vector3 point, Ray ray)
        {
            Vector3 originToPoint = point - ray.origin;
            
            float directionSqrMagnitude = ray.direction.sqrMagnitude;
            if (directionSqrMagnitude < float.Epsilon)
            {
                return ray.origin;
            }
            
            float t = Mathf.Max(0f, Vector3.Dot(originToPoint, ray.direction) / directionSqrMagnitude);
            return ray.origin + t * ray.direction;
        }

        /// <summary>
        /// Linearly interpolates two directions without waving their magnitudes. Considers
        /// the world Y as the up direction.
        /// </summary>
        /// <param name="directionA"></param>
        /// <param name="directionB"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Vector3 LerpDirections(Vector3 directionA, Vector3 directionB, float t)
        {
            if (directionA == directionB) return directionA;

            Quaternion rotationA = Quaternion.LookRotation(directionA.normalized);
            Quaternion rotationB = Quaternion.LookRotation(directionB.normalized);

            return Quaternion.Slerp(rotationA, rotationB, t) * Vector3.forward;
        }
        
        /// <summary>
        /// Returns a value between 0 and 1 which is how close a position is to a or b.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public static float InverseLerp(Vector3 a, Vector3 b, Vector3 position)
        {
            Vector3 direction = b - a;
            if (direction.magnitude <= float.Epsilon) return 0f;
            
            Vector3 projection = Vector3.Project(position - a, direction);
            return Mathf.Clamp01(projection.magnitude / direction.magnitude);
        }

        /// <summary>
        /// Linearly interpolates between two values with an extra intermediate value
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Vector3 Lerp(Vector3 a, Vector3 b, Vector3 c, float t)
        {
            return t <= 0.5f
                ? Vector3.Lerp(a, b, t * 2f)
                : Vector3.Lerp(b, c, t * 2f - 1f);
        }
    }
}