using System;
using System.Collections.Generic;
using GameCreator.Runtime.Cameras;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Shooter
{
    [Title("Pointer on Raycast")]
    [Description("Aims towards the point from the camera's point towards the first collider found")]

    [Category("Pointer on Raycast")]
    [Image(typeof(IconCursor), ColorTheme.Type.Yellow, typeof(OverlayPhysics))]

    [Serializable]
    public class AimPointerOnRaycast : TAim
    {
        private const float INFINITY = 999f;
        
        private static readonly RaycastHit[] HITS = new RaycastHit[128];
        
        private class RaycastComparer : IComparer<RaycastHit>
        {
            public int Compare(RaycastHit a, RaycastHit b)
            {
                return a.distance.CompareTo(b.distance);
            }
        }
        
        private static readonly RaycastComparer RAYCAST_COMPARER = new RaycastComparer();

        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField] private PropertyGetGameObject m_Camera = GetGameObjectCameraMain.Create;
        [SerializeField]
        private InputPropertyValueVector2 m_Cursor = new InputPropertyValueVector2(
            new InputValueVector2MousePosition()
        );

        [SerializeField] private PropertyGetGameObject m_Target = GetGameObjectSelf.Create();

        [SerializeField] private PropertyGetDecimal m_MinDistance = GetDecimalConstantPointOne.Create;
        [SerializeField] private LayerMask m_LayerMask = Physics.DefaultRaycastLayers;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public override Vector3 GetPoint(Args args)
        {
            Camera camera = this.m_Camera.Get<Camera>(args);
            if (camera == null) return default;

            Transform target = this.m_Target.Get<Transform>(args);
            if (target == null) return default;

            float minDistance = (float) this.m_MinDistance.Get(args);
            Vector2 cursor = this.m_Cursor.Read();

            int numHits = Physics.RaycastNonAlloc(
                camera.ScreenPointToRay(cursor),
                HITS,
                INFINITY,
                this.m_LayerMask,
                QueryTriggerInteraction.Ignore
            );
            
            Array.Sort(HITS, 0, numHits, RAYCAST_COMPARER);

            for (int i = 0; i < numHits; ++i)
            {
                if (HITS[i].transform.IsChildOf(args.Self.transform))
                {
                    continue;
                }
                
                float distance = HITS[i].distance;
                return distance >= minDistance 
                    ? HITS[i].point
                    : target.TransformPoint(Vector3.forward * INFINITY);
            }

            return target.TransformPoint(Vector3.forward * INFINITY);
        }

        public override void Enter(Character character)
        {
            base.Enter(character);
            this.m_Cursor.OnStartup();
        }

        public override void Exit(Character character)
        {
            base.Exit(character);
            this.m_Cursor.OnDispose();
        }
    }
}