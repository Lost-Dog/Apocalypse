// using System;
// using GameCreator.Runtime.Characters;
// using GameCreator.Runtime.Common;
// using UnityEngine;
//
// namespace GameCreator.Runtime.Traversal
// {
//     [Title("Slide to Point")]
//     [Category("Slide to Point")]
//     
//     [Description("Makes a character move from its current position towards a destination point")]
//     [Image(typeof(IconBug), ColorTheme.Type.Green)]
//     
//     [Keywords("Slide", "Position", "Zip")]
//     
//     [Serializable]
//     public class __TraverseLinkTypeSlideToPoint : TraverseLinkType
//     {
//         // EXPOSED MEMBERS: -----------------------------------------------------------------------
//
//         [SerializeField] private Vector3 m_LocalOffset;
//
//         // PROPERTIES: ----------------------------------------------------------------------------
//
//         public Vector3 LocalOffset
//         {
//             get => this.m_LocalOffset;
//             set
//             {
//                 this.m_LocalOffset = value;
//                 this.OnChange();
//             }
//         }
//
//         // CONSTRUCTOR: ---------------------------------------------------------------------------
//
//         public __TraverseLinkTypeSlideToPoint()
//         { }
//
//         public __TraverseLinkTypeSlideToPoint(Vector3 offset)
//         {
//             this.m_LocalOffset = offset;
//         }
//
//         // PUBLIC METHODS: ------------------------------------------------------------------------
//
//         public override Vector3 GetLocalPositionA(Character character, TraverseLink traverseLink)
//         {
//             Vector3 startPosition = this.CalculateStartPosition(character, traverseLink);
//             Vector3 localStartPosition = traverseLink.Transform.InverseTransformPoint(startPosition);
//             
//             return localStartPosition;
//         }
//
//         public override Vector3 GetLocalPositionB(Character character, TraverseLink traverseLink)
//         {
//             return this.m_LocalOffset;
//         }
//
//         public override Quaternion GetLocalRotationA(Character character, TraverseLink traverseLink)
//         {
//             Vector3 direction = traverseLink.transform.position - character.transform.position;
//             direction.y = 0f;
//             
//             Vector3 localDirection = traverseLink.transform.InverseTransformDirection(direction);
//             return Quaternion.LookRotation(localDirection);
//         }
//         
//         public override Quaternion GetLocalRotationB(Character character, TraverseLink traverseLink)
//         {
//             Vector3 direction = traverseLink.transform.position - character.transform.position;
//             direction.y = 0f;
//             
//             Vector3 localDirection = traverseLink.transform.InverseTransformDirection(direction);
//             return Quaternion.LookRotation(localDirection);
//         }
//
//         public override void OnDrawGizmos(Transform transform)
//         {
//             Gizmos.matrix = transform.localToWorldMatrix;
//             
//             Gizmos.color = GIZMOS_COLOR_SOLID;
//             Gizmos.DrawCube(this.m_LocalOffset, Vector3.one * 0.1f);
//         }
//
//         private Vector3 CalculateStartPosition(Character character, TraverseLink traverseLink)
//         {
//             return traverseLink.MotionLink != null
//                 ? traverseLink.MotionLink.CharacterPosition(character)
//                 : default;
//         }
//     }
// }