using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace KingEdward.SkillTree
{
    [Title("Instantiate Multiple Symmetrically in Curve")]
    [Description("Creates multiple instances of a referenced game object, arranging them in a progressive curve around a center point aligned with another referenced GameObject's local space")]

    [Category("KingEdward/Skill Tree/VFX/Instantiate Multiple Symmetrically in Curve")]
    
    [Parameter("Game Object", "Game Object reference that is instantiated")]
    [Parameter("Position", "The center position for the instantiation (e.g., character position)")]
    [Parameter("Rotation", "The rotation of the new game object instances")]
    [Parameter("Counts", "Dynamic list that determines how many instances are created for each corresponding save slot")]
    [Parameter("Side Offset", "Sideways distance between instances (for left-right alignment)")]
    [Parameter("Base Forward Offset", "Base forward distance for the center instance")]
    [Parameter("Forward Multiplier", "Multiplier for progressively increasing forward offset for side instances")]
    [Parameter("Align Reference", "The GameObject whose local axes will be used for alignment")]
    [Parameter("Save", "List where each instantiated game object is stored")]
    [Parameter("Align With Reference", "Whether objects should align with reference rotation")]
    
    [Image(typeof(IconCubeSolid), ColorTheme.Type.Blue, typeof(OverlayPlus))]
    
    [Serializable]
    public class InstructionGameObjectInstantiateMultipleCurve : Instruction
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField] 
        private PropertyGetInstantiate m_GameObject = new PropertyGetInstantiate();

        [SerializeField]
        private PropertyGetPosition m_Position = GetPositionCharactersPlayer.Create;

        [SerializeField]
        private PropertyGetRotation m_Rotation = GetRotationCharactersPlayer.Create;

        [SerializeField]
        private PropertyGetGameObject m_Parent = GetGameObjectNone.Create();

        [SerializeField]
        private PropertyGetGameObject m_AlignReference = GetGameObjectNone.Create();  // GameObject used for alignment

        [SerializeField]
        private PropertyGetInteger m_Count = new PropertyGetInteger(1);  // Number of instances to create

        [SerializeField]
        private PropertySetGameObject m_Save = new PropertySetGameObject();  // Save each instantiated object

        [SerializeField]
        private float m_SideOffset = 1f;  // Sideways distance between instances

        [SerializeField]
        private float m_BaseForwardOffset = 1f;  // Forward distance for the center instance

        [SerializeField]
        private float m_ForwardMultiplier = 0.5f;  // Multiplier to increase the forward offset progressively for side instances

        [SerializeField]
        private bool m_AlignWithReference = true;  // Whether objects should align with reference rotation
        
        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => $"Instantiate Multiple {this.m_GameObject} in Curve";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override Task Run(Args args)
        {
            Vector3 centerPosition = this.m_Position.Get(args);  // This is the central position
            Quaternion baseRotation = this.m_Rotation.Get(args);

            // Get the transform of the GameObject used for alignment (e.g., character, or any other GameObject)
            Transform alignTransform = this.m_AlignReference.Get<Transform>(args);
            if (alignTransform == null)
            {
                Debug.LogWarning("Align Reference GameObject is null. Please assign a valid GameObject.");
                return DefaultResult;
            }

            // Dynamically get the count for instantiation
            int instanceCount = (int)this.m_Count.Get(args);
            if (instanceCount <= 0) return DefaultResult;

            // Calculate positions symmetrically around the center, with a progressively increasing forward offset for the side shots
            for (int j = 0; j < instanceCount; j++)
            {
                float sideOffsetFromCenter = (j - (instanceCount - 1) / 2f) * m_SideOffset;

                // Forward offset increases progressively for side shots
                float forwardOffset = m_BaseForwardOffset + Mathf.Abs((j - (instanceCount - 1) / 2f) * m_ForwardMultiplier);

                Vector3 sideOffset = alignTransform.right * sideOffsetFromCenter;  // Sideways positioning
                Vector3 forwardOffsetVec = alignTransform.forward * forwardOffset;  // Forward/backward positioning
                
                Vector3 position = centerPosition + sideOffset + forwardOffsetVec;

                // Calculate final rotation
                Quaternion finalRotation;
                if (m_AlignWithReference)
                {
                    // Make objects look outward from center along the curve
                    Vector3 directionFromCenter = position - centerPosition;
                    if (directionFromCenter.sqrMagnitude > 0.001f)
                    {
                        // Look in the direction away from center, using align transform's up vector
                        finalRotation = Quaternion.LookRotation(directionFromCenter, alignTransform.up);
                    }
                    else
                    {
                        // If at center, use align transform rotation
                        finalRotation = alignTransform.rotation * baseRotation;
                    }
                }
                else
                {
                    finalRotation = baseRotation;
                }

                InstantiateAtPosition(position, finalRotation, args);
            }

            return DefaultResult;
        }

        // Helper method to instantiate and save at a specific position
        private void InstantiateAtPosition(Vector3 position, Quaternion rotation, Args args)
        {
            GameObject instance = this.m_GameObject.Get(args, position, rotation);

            if (instance != null)
            {
                ProjectileBehavior.RegisterCasterFromArgs(instance, args);
                Transform parent = this.m_Parent.Get<Transform>(args);
                if (parent != null) instance.transform.SetParent(parent);
                
                this.m_Save.Set(instance, args);
            }
        }

    }
}
