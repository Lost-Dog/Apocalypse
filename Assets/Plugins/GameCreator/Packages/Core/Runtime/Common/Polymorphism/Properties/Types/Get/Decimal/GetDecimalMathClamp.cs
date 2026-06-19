using System;
using UnityEngine;

namespace GameCreator.Runtime.Common
{
    [Title("Clamp Number")]
    [Category("Math/Arithmetic/Clamp Number")]
    
    [Image(typeof(IconAbsolute), ColorTheme.Type.TextNormal)]
    [Description("The numeric value clamped between two numbers")]

    [Keywords("Float", "Decimal", "Double", "Between", "Clamp", "Range")]
    
    [Serializable]
    public class GetDecimalMathClamp : PropertyTypeGetDecimal
    {
        [SerializeField] protected PropertyGetDecimal m_Number = new PropertyGetDecimal();
        [SerializeField] protected PropertyGetDecimal m_Min = new PropertyGetDecimal(0f);
        [SerializeField] protected PropertyGetDecimal m_Max = new PropertyGetDecimal(1f);

        public override double Get(Args args)
        {
            double number = this.m_Number.Get(args);
            double min = this.m_Min.Get(args);
            double max = this.m_Max.Get(args);
            
            return Math.Clamp(number, min, max);
        }

        public override string String => $"{this.m_Number} in [{this.m_Min}, {this.m_Max}]";
    }
}