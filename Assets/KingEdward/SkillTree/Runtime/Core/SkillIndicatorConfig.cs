using System;
using UnityEngine;
using GameCreator.Runtime.Common;

namespace KingEdward.SkillTree
{
    public enum SkillIndicatorType
    {
        None,
        Circle,
        ExpandingCircle,
        Cone,
        Line,
        ExpandingLine
    }

    /// <summary>
    /// Configuration for the skill indicator.
    /// </summary>
    [Serializable]
    public class SkillIndicatorConfig
    {
        [Tooltip("Type of indicator shape")]
        public SkillIndicatorType type = SkillIndicatorType.None;

        [Tooltip("Radius for circle indicator")]
        [SerializeField] private PropertyGetDecimal m_Radius = new PropertyGetDecimal(3.0);

        [Tooltip("Min radius for ExpandingCircle (starts here when holding)")]
        [SerializeField] private PropertyGetDecimal m_MinRadius = new PropertyGetDecimal(1.0);

        [Tooltip("Max radius for ExpandingCircle (reaches here after expand duration)")]
        [SerializeField] private PropertyGetDecimal m_MaxRadius = new PropertyGetDecimal(6.0);

        [Tooltip("Seconds to grow from min to max radius (ExpandingCircle)")]
        [SerializeField] private PropertyGetDecimal m_ExpandDuration = new PropertyGetDecimal(2.0);

        [Tooltip("Angle in degrees for cone indicator (full cone angle, e.g. 5-360)")]
        [SerializeField] private PropertyGetDecimal m_ConeAngle = new PropertyGetDecimal(60.0);

        [Tooltip("Range/distance for cone and line indicators")]
        [SerializeField] private PropertyGetDecimal m_Range = new PropertyGetDecimal(8.0);

        [Tooltip("Vertical offset from ground")]
        [SerializeField] private PropertyGetDecimal m_GroundOffset = new PropertyGetDecimal(0.05);

        [Tooltip("Optional material for this indicator. If set, overrides the default material on the SkillIndicator component.")]
        public Material material;

        [Tooltip("Indicator color")]
        public Color color = new Color(1f, 0.3f, 0.2f, 0.4f);

        [Tooltip("If true, indicator stays at character position (Circle/ExpandingCircle). If false, follows cursor.")]
        public bool fixedAtCharacter = false;

        public bool HasIndicator => type != SkillIndicatorType.None;

        public bool IsCircleType => type == SkillIndicatorType.Circle || type == SkillIndicatorType.ExpandingCircle;

        public bool IsExpanding => type == SkillIndicatorType.ExpandingCircle;

        public bool IsExpandingLine => type == SkillIndicatorType.ExpandingLine;

        public bool IsLineType => type == SkillIndicatorType.Line || type == SkillIndicatorType.ExpandingLine;

        public float Radius => (float)m_Radius.Get(Args.EMPTY);
        public float MinRadius => (float)m_MinRadius.Get(Args.EMPTY);
        public float MaxRadius => (float)m_MaxRadius.Get(Args.EMPTY);
        public float ExpandDuration => (float)m_ExpandDuration.Get(Args.EMPTY);
        public float ConeAngle => (float)m_ConeAngle.Get(Args.EMPTY);
        public float Range => (float)m_Range.Get(Args.EMPTY);
        public float GroundOffset => (float)m_GroundOffset.Get(Args.EMPTY);
    }
}
