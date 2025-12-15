using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.InputSystem.Layouts;

namespace JUTPS.CrossPlataform
{
    /// <summary>
    /// Simulate a mobile joystick and override input action.
    /// </summary>
    [AddComponentMenu("JU TPS/Mobile/JU Joystick")]
    public class JUJoystickVirtual : OnScreenControl, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        private PointerEventData _eventData;

        [SerializeField, InputControl(layout = "Vector2")]
        private string _controlPath;

        /// <summary>
        /// The max drag joystick distance.
        /// </summary>
        [Range(0, 1)]
        public float JoystickMaxDistance;

        /// <summary>
        /// The joystick background image.
        /// </summary>
        public Image BackgroundImage;

        /// <summary>
        /// The draggable joystick center.
        /// </summary>
        public Image JoystickImage;

        /// <summary>
        /// Return true if is pressing the joystick.
        /// </summary>
        public bool IsPressed { get; private set; }

        /// <summary>
        /// The joystick drag direction.
        /// </summary>
        public Vector2 InputVector { get; private set; }

        /// <summary>
        /// Return the joystick drag distance normalized.
        /// </summary>
        public float DragDistanceNormalized
        {
            get => InputVector.normalized.magnitude;
        }

        /// <summary>
        /// Internal, set or get the input path.
        /// </summary>
        protected override string controlPathInternal
        {
            get => _controlPath;
            set => _controlPath = value;
        }

        /// <summary>
        /// Create instance.
        /// </summary>
        public JUJoystickVirtual()
        {
            JoystickMaxDistance = 0.45f;
        }

        private void Start()
        {
            if (!BackgroundImage)
                Debug.LogError($"The joystick {name} must have a background image assigned.");
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            InputVector = Vector2.zero;
        }

        private void Update()
        {
            RefreshJoystick();
        }

        /// <summary>
        /// Internal.
        /// </summary>
        /// <param name="e"></param>
        public void OnPointerDown(PointerEventData e)
        {
            _eventData = e;
            IsPressed = true;
            OnDrag(e);
        }

        /// <summary>
        /// Internal.
        /// </summary>
        /// <param name="e"></param>
        public void OnDrag(PointerEventData e)
        {
            if (!BackgroundImage)
                return;

            _eventData = e;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(BackgroundImage.rectTransform, _eventData.position, _eventData.pressEventCamera, out Vector2 relativePosition))
            {
                Vector2 backgroundSize = BackgroundImage.rectTransform.sizeDelta;

                relativePosition.x = relativePosition.x / backgroundSize.x;
                relativePosition.y = relativePosition.y / backgroundSize.y;

                InputVector = new Vector2(relativePosition.x * 2 + 1, relativePosition.y * 2 - 1);
                InputVector = (InputVector.magnitude > 1f) ? InputVector.normalized : InputVector;
                SendValueToControl(InputVector);
            }

            SendValueToControl(InputVector);
        }

        /// <summary>
        /// Internal.
        /// </summary>
        /// <param name="e"></param>
        public void OnPointerUp(PointerEventData e)
        {
            _eventData = default;
            IsPressed = false;
            InputVector = Vector2.zero;

            SendValueToControl(Vector2.zero);
        }

        private void RefreshJoystick()
        {
            if (!BackgroundImage)
                return;

            Vector2 backgroundSize = BackgroundImage.rectTransform.sizeDelta;

            if (JoystickImage)
                JoystickImage.rectTransform.anchoredPosition = InputVector * (backgroundSize * JoystickMaxDistance);
        }
    }
}