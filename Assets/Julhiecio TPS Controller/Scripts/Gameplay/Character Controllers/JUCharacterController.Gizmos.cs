using UnityEngine;

namespace JUTPS
{
    public partial class JUCharacterController
    {
        /// <summary>
        /// Character editor debug options.
        /// </summary>
        [System.Serializable]
        public class DebugSettings
        {
            /// <summary>
            /// Show debug gizmos only in editor edit mode (only when <see cref="Application.isPlaying"/> is false).
            /// </summary>
            public bool OnlyInEditMode;

            /// <summary>
            /// Show step up correction gizmos? 
            /// </summary>
            public bool StepUpCorrection;

            /// <summary>
            /// Show ground check sensor gizmos?
            /// </summary>
            public bool GroundCheck;

            /// <summary>
            /// The solid gizmos color.
            /// </summary>
            [Space]
            public Color GizmosColor;

            /// <summary>
            /// The wire gizmos color.
            /// </summary>
            public Color WireGizmosColor;

            /// <summary>
            /// The step mesh used by the gizmo (loaded in runtime).
            /// </summary>
            [HideInInspector]
            public Mesh StepVisualizerMesh;

            public DebugSettings()
            {
                StepUpCorrection = true;
                GroundCheck = true;
                GizmosColor = new Color(0, 0, 0, 0.5f);
                WireGizmosColor = new Color(0.9f, 0.4f, 0.2f, 0.5f);
            }
        }

        /// <summary>
        /// Editor debug options used to draw character gizmos.
        /// </summary>
        public DebugSettings CharacterDebug;

        /// <summary>
        /// Draw debug gizmos.
        /// </summary>
        protected virtual void OnDrawGizmos()
        {
            if (CharacterDebug == null)
                CharacterDebug = new DebugSettings();

            if (Application.isPlaying && CharacterDebug.OnlyInEditMode)
                return;

            if (!CharacterDebug.StepVisualizerMesh)
                LoadStepMesh();

            float gizmoscale = 1f;

            //Step Visualizer
            Gizmos.color = WallAHead ? Color.green : Color.red;
            Gizmos.DrawLine(transform.position + transform.up * WallRayHeight, transform.position + transform.up * WallRayHeight + transform.forward * WallRayDistance);

            if (CharacterDebug.GroundCheck)
            {
                Gizmos.color = IsGrounded ? Color.black : Color.green;
                Gizmos.DrawWireCube(transform.position + transform.up * GroundCheckHeighOfsset, new Vector3(GroundCheckRadius, GroundCheckSize, GroundCheckRadius) * 2);
            }

            //Step Correction Settings
            if (CharacterDebug.StepUpCorrection)
            {
                Vector3 forward = DirectionTransform ? DirectionTransform.forward : transform.forward;
                Quaternion rotation = DirectionTransform ? DirectionTransform.rotation : transform.rotation;

                Vector3 stepPos = transform.position + forward * ForwardStepOffset + transform.up * StepHeight;
                Vector3 stepPosHeight = transform.position + forward * ForwardStepOffset + transform.up * FootstepHeight;

                if (_stepHit.point != Vector3.zero && _stepHit.point.y > transform.position.y + StepHeight)
                {
                    stepPos = _stepHit.point;
                    Gizmos.color = Color.white;
                    Gizmos.DrawWireSphere(_stepHit.point, 0.05f);
                }

                Gizmos.color = CharacterDebug.GizmosColor;
                Gizmos.DrawMesh(CharacterDebug.StepVisualizerMesh, 0, stepPos, rotation, Vector3.one * gizmoscale);

                Gizmos.color = CharacterDebug.WireGizmosColor;
                Gizmos.DrawWireMesh(CharacterDebug.StepVisualizerMesh, 0, stepPos, rotation, Vector3.one * gizmoscale);
                Gizmos.DrawWireMesh(CharacterDebug.StepVisualizerMesh, 0, stepPosHeight, rotation, Vector3.one * gizmoscale);

                Gizmos.DrawLine(stepPos, stepPosHeight);
            }
        }

        private void LoadStepMesh()
        {
            CharacterDebug.StepVisualizerMesh = JUGizmoDrawer.GetJUGizmoDefaultMesh(JUGizmoDrawer.DrawMesh.Steps);
        }
    }
}

