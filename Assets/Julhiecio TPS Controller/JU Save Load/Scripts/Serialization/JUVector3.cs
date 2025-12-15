using UnityEngine;

namespace JU.SaveLoad.Serialization
{
    /// <summary>
    /// Used as bridge to serialize <see cref="Vector3"/> values by the <see cref="Newtonsoft"/>.
    /// </summary>
    public struct JUVector3
    {
        /// <summary>
        /// The X axis value.
        /// </summary>
        public float x;

        /// <summary>
        /// The Y axis value.
        /// </summary>
        public float y;

        /// <summary>
        /// The Z axis value.
        /// </summary>
        public float z;

        /// <summary>
        /// Create a serializable vector using a <see cref="Vector3"/>. 
        /// </summary>
        /// <param name="vector">The vector.</param>
        public JUVector3(Vector3 vector)
        {
            x = vector.x;
            y = vector.y;
            z = vector.z;
        }

        /// <summary>
        /// Create a serializable vector using raw values. 
        /// </summary>
        /// <param name="x">The X axis value.</param>
        /// <param name="y">The Y axis value.</param>
        /// <param name="z">The Z axis value.</param>
        public JUVector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }
}