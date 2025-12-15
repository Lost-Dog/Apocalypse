using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.Events;

namespace JUTPS.CrossPlataform
{
    /// <summary>
    /// UI Button used to create controls overriding InputActions.
    /// </summary>
    [AddComponentMenu("JU TPS/Mobile/JU Button")]
    public class JUButtonVirtual : OnScreenControl, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField, InputControl(layout = "Button")]
        private string _controlPath;

        /// <summary>
        /// Invoked if the button is pressed.
        /// </summary>
        public UnityEvent OnPressed;

        /// <summary>
        /// Invoked on the first button press.
        /// </summary>
        public UnityEvent OnPressedDown;

        /// <summary>
        /// Invoked on the button release.
        /// </summary>
        public UnityEvent OnPressedUp;

        /// <summary>
        /// Return true if is pressing the button.
        /// </summary>
        public bool IsPressed { get; private set; }

        /// <summary>
        /// Return true one time if press the button.
        /// </summary>
        public bool IsPressedDown { get; private set; }

        /// <summary>
        /// Return true if release the button.
        /// </summary>
        public bool IsPressedUp { get; private set; }

        /// <summary>
        /// Internal, set or get the input path.
        /// </summary>
        protected override string controlPathInternal
        {
            get => _controlPath;
            set => _controlPath = value;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            IsPressed = false;
            IsPressedDown = false;
            IsPressedUp = false;
        }

        private void Update()
        {
            if (IsPressed)
                OnPressed.Invoke();
        }

        /// <summary>
        /// Internal.
        /// </summary>
        /// <param name="e"></param>
        public void OnPointerDown(PointerEventData e)
        {
            IsPressed = true;
            IsPressedUp = false;
            IsPressedDown = true;

            StartCoroutine(DisableIsPressedDownAtEndOfFrame());
            SendValueToControl(1f);

            OnPressedDown.Invoke();
            OnPressed.Invoke();
        }

        /// <summary>
        /// Internal.
        /// </summary>
        /// <param name="e"></param>
        public void OnPointerUp(PointerEventData e)
        {
            IsPressed = false;
            IsPressedDown = false;
            IsPressedUp = true;

            StartCoroutine(DisableIsPressedUpAtEndOfFrame());
            SendValueToControl(0f);

            OnPressedUp.Invoke();
        }

        private IEnumerator DisableIsPressedDownAtEndOfFrame()
        {
            yield return new WaitForEndOfFrame();
            IsPressedDown = false;
        }

        private IEnumerator DisableIsPressedUpAtEndOfFrame()
        {
            yield return new WaitForEndOfFrame();
            IsPressedUp = false;
        }
    }
}