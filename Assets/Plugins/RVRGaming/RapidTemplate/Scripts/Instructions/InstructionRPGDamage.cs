using System;
using UnityEngine;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Variables;

namespace GameCreator.Runtime.VisualScripting
{
    [Title("Calculate RPG Damage Formula")]
    [Description("Performs a custom calculation based on attack, defence, resistances, and a damage multiplier.")]
    [Category("Math/RPG Formula")]

    [Keywords("RPG", "Calculation", "Formula")]
    [Image(typeof(IconDivideCircle), ColorTheme.Type.Blue)]

    [Serializable]
    public class InstructionRPGDamage : Instruction
    {
        [SerializeField]
        private PropertyGetDecimal m_Attack = new PropertyGetDecimal();
        [SerializeField]
        private PropertyGetDecimal m_FireDamage = new PropertyGetDecimal();
        [SerializeField]
        private PropertyGetDecimal m_IceDamage = new PropertyGetDecimal();
        [SerializeField]
        private PropertyGetDecimal m_LightningDamage = new PropertyGetDecimal();
        [SerializeField]
        private PropertyGetDecimal m_Defence = new PropertyGetDecimal();
        [SerializeField]
        private PropertyGetDecimal m_FireResistance = new PropertyGetDecimal();
        [SerializeField]
        private PropertyGetDecimal m_IceResistance = new PropertyGetDecimal();
        [SerializeField]
        private PropertyGetDecimal m_LightningResistance = new PropertyGetDecimal();
        [SerializeField]
        private PropertyGetDecimal m_Multiplier = new PropertyGetDecimal();
        [SerializeField]
        private PropertySetNumber m_Result = new PropertySetNumber();

        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => $"RPG formula to calculate the total damage";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override Task Run(Args args)
        {
            double attackValue = GetSafeValue(m_Attack, args);
            double fireDamageValue = GetSafeValue(m_FireDamage, args);
            double iceDamageValue = GetSafeValue(m_IceDamage, args);
            double lightningDamageValue = GetSafeValue(m_LightningDamage, args);
            double defenceValue = GetSafeValue(m_Defence, args);
            double fireResistanceValue = GetSafeValue(m_FireResistance, args);
            double iceResistanceValue = GetSafeValue(m_IceResistance, args);
            double lightningResistanceValue = GetSafeValue(m_LightningResistance, args);
            double multiplierValue = GetSafeValue(m_Multiplier, args);

            double CalculateEffectiveDamage(double damage, double resistance)
            {
                return damage - Math.Min(damage, resistance);
            }

            double fireEffect = CalculateEffectiveDamage(fireDamageValue, fireResistanceValue);
            double iceEffect = CalculateEffectiveDamage(iceDamageValue, iceResistanceValue);
            double lightningEffect = CalculateEffectiveDamage(lightningDamageValue, lightningResistanceValue);

            double totalDamage = attackValue + fireEffect + iceEffect + lightningEffect;
            double effectiveDefence = Math.Max(1.0, 100.0 + defenceValue);
            double resultValue = (totalDamage * (100.0 / effectiveDefence)) * multiplierValue;

            Debug.Log($"Calculated Result: {resultValue}");
            m_Result.Set(resultValue, args);

            return DefaultResult;
        }

        // HELPER METHODS: ------------------------------------------------------------------------

        private double GetSafeValue(PropertyGetDecimal property, Args args)
        {
            try
            {
                return property.Get(args);
            }
            catch
            {
                Debug.LogWarning($"Failed to retrieve value for property: {property}");
                return 0.0;
            }
        }
    }
}
