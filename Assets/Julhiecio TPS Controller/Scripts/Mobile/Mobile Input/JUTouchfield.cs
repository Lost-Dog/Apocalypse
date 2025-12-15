using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.InputSystem.Layouts;

namespace JUTPS.CrossPlataform
{
    /// <summary>
    /// Simulate a mobile touchfield and override a input action.
    /// </summary>
    [AddComponentMenu("JU TPS/Mobile/JU Touchfield")]
    public class JUTouchfield : OnScreenControl, IPointerDownHandler, IPointerUpHandler
    {
        private Vector2 _lastPressPoint;
        private PointerEventData _eventData;

        [SerializeField, InputControl(layout = "Vector2")]
        private string _controlPath;

        /// <summary>
        /// The touchfield drag sensibility.
        /// </summary>
        public float Sensibility;

        /// <summary>
        /// The drag distance.
        /// </summary>
        public Vector2 DragDelta { get; private set; }

        /// <summary>
        /// Return true if is pressing this touchfield.
        /// </summary>
        public bool IsPressed { get; private set; }

        protected override string controlPathInternal
        {
            get => _controlPath;
            set => _controlPath = value;
        }

        /// <summary>
        /// Create instance.
        /// </summary>
        public JUTouchfield()
        {
            Sensibility = 1f;
            _controlPath = "<Mouse>/delta";
        }

        void Update()
        {
            if (IsPressed)
            {
                if (_eventData != null)
                {
                    DragDelta = (_eventData.position - _lastPressPoint) * Sensibility;
                    _lastPressPoint = _eventData.position;
                }
                else
                {
                    DragDelta = Vector2.zero;
                    _lastPressPoint = Vector2.zero;
                }

                SendValueToControl(DragDelta);
            }
            else
                DragDelta = Vector2.zero;
        }

        /// <summary>
        /// Internal.
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerDown(PointerEventData eventData)
        {
            IsPressed = true;
            DragDelta = Vector2.zero;

            _eventData = eventData;
            _lastPressPoint = eventData.position;

            SendValueToControl(DragDelta);
        }

        /// <summary>
        /// Internal.
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerUp(PointerEventData eventData)
        {
            IsPressed = false;
            DragDelta = Vector2.zero;

            _lastPressPoint = eventData.position;
            _eventData = null;

            SendValueToControl(DragDelta);
        }
    }
}