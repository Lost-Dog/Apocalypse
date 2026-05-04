using System;
using UnityEngine;

namespace GameCreator.Runtime.Common
{
    [Title("Distance between Points")]
    [Category("Math/Geometry/Distance between Points")]
    
    [Image(typeof(IconCompass), ColorTheme.Type.TextNormal, typeof(OverlayBar))]
    [Description("The distance between two points in space")]

    [Keywords("Distance", "Separation")]
    
    [Serializable]
    public class GetDecimalMathDistance : PropertyTypeGetDecimal
    {
        [SerializeField] protected PropertyGetPosition m_Position1 = new PropertyGetPosition();
        [SerializeField] protected PropertyGetPosition m_Position2 = new PropertyGetPosition();

        public override double Get(Args args)
        {
            Vector3 position1 = this.m_Position1.Get(args);
            Vector3 position2 = this.m_Position2.Get(args);
            
            return Vector3.Distance(position1, position2);
        }

        public override string String => $"Distance [{this.m_Position1}, {this.m_Position2}]";

        public override double EditorValue
        {
            get
            {
                Vector3 position1 = this.m_Position1.EditorValue;
                Vector3 position2 = this.m_Position2.EditorValue;

                if (position1 == Vector3.zero) return default;
                if (position2 == Vector3.zero) return default;
                
                return Vector3.Distance(
                    position1,
                    position2
                );
            }
        }
    }
}