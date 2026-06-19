using System.Text;
using UnityEngine;

namespace MxMGameplay
{
    // Temporary debug HUD for grounding and state verification during play tests.
    public class NGGroundingDebugHUD : MonoBehaviour
    {
        [SerializeField] private bool forceDisableOverlay = true;
        [SerializeField] private bool visible = true;

        private NGCharacterControllerWrapper wrapper;
        private Threepeat.NGCharacter ngCharacter;
        private Threepeat.MMCGameCreator2 gc2Bridge;

        private GUIStyle labelStyle;
        private StringBuilder sb;

        public void Initialize(NGCharacterControllerWrapper targetWrapper)
        {
            if (forceDisableOverlay)
            {
                enabled = false;
                return;
            }

            wrapper = targetWrapper;
            if (wrapper != null)
            {
                ngCharacter = wrapper.GetComponent<Threepeat.NGCharacter>();
                gc2Bridge = wrapper.GetComponent<Threepeat.MMCGameCreator2>();
            }
        }

        private void Awake()
        {
            sb = new StringBuilder(256);

            if (forceDisableOverlay)
            {
                enabled = false;
            }
        }

        private void EnsureStyles()
        {
            if (labelStyle != null)
            {
                return;
            }

            if (GUI.skin == null)
            {
                return;
            }

            labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                richText = false,
                normal = { textColor = Color.white }
            };
        }

        private void OnGUI()
        {
            if (!visible || wrapper == null)
            {
                return;
            }

            EnsureStyles();
            if (labelStyle == null)
            {
                return;
            }

            Rect panelRect = new Rect(12f, 12f, 900f, 132f);
            Color previousColor = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.65f);
            GUI.Box(panelRect, GUIContent.none);
            GUI.color = previousColor;

            sb.Length = 0;
            sb.Append("MMLC Debug\n");
            sb.Append("IsGrounded: ").Append(wrapper.IsGrounded).Append('\n');
            sb.Append("CurrentState: ").Append(ngCharacter != null ? ngCharacter.currentState.ToString() : "(NGCharacter missing)").Append('\n');
            sb.Append("GroundLayers: ").Append(wrapper.GroundLayers.value).Append(" [").Append(LayerMaskToNames(wrapper.GroundLayers.value)).Append("]");

            if (gc2Bridge != null)
            {
                if (gc2Bridge.TryGetDebugMixerWeights(out float gcWeight, out float mxmWeight))
                {
                    sb.Append('\n');
                    sb.Append("GC2/MxM: gcWeight=").Append(gcWeight.ToString("0.000"))
                        .Append(", mxmWeight=").Append(mxmWeight.ToString("0.000"))
                        .Append(", gcCharacter.enabled=").Append(gc2Bridge.IsGCCharacterEnabled);
                }
                else
                {
                    sb.Append('\n');
                    sb.Append("GC2/MxM: mixer not ready, gcCharacter.enabled=").Append(gc2Bridge.IsGCCharacterEnabled);
                }
            }

            GUI.Label(new Rect(20f, 18f, 888f, 118f), sb.ToString(), labelStyle);
        }

        private static string LayerMaskToNames(int mask)
        {
            if (mask == 0)
            {
                return "None";
            }

            StringBuilder names = new StringBuilder(64);
            for (int i = 0; i < 32; i++)
            {
                if ((mask & (1 << i)) == 0)
                {
                    continue;
                }

                string layerName = LayerMask.LayerToName(i);
                if (string.IsNullOrEmpty(layerName))
                {
                    layerName = $"Layer{i}";
                }

                if (names.Length > 0)
                {
                    names.Append(", ");
                }

                names.Append(layerName);
            }

            return names.ToString();
        }
    }
}
