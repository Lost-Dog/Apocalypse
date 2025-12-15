using System;
using System.Collections;
using JU.Editor;
using JUTPS.CameraSystems;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace JUTPS.UI
{
    /// <summary>
    /// The game pause screen.
    /// </summary>
    public class JU_UIPause : MonoBehaviour
    {
        private bool _defaultMouseVisible;
        private bool _defaultMouseLock;

        /// <summary>
        /// The scene name of the menu scene, used when the <see cref="MainMenuButton"/> is pressed.
        /// </summary>
        [Header("Scenes")]
        [SerializeField] private string MainMenuScene;

        /// <summary>
        /// The Pause screen UI.
        /// </summary>
        [Header("Screens")]
        public GameObject PauseScreen;

        /// <summary>
        /// The game settings screen, can be accessed by the pause screen.
        /// </summary>
        public JU_UISettings SettingsScreen;

        /// <summary>
        /// The "continue game" button, used to unpause the game calling <seealso cref="JUPauseGame.Continue"/>.
        /// </summary>
        [Header("Buttons")]
        public Button ContinueButton;

        /// <summary>
        /// The pause button on game HUD.
        /// </summary>
        public Button PauseButton;

        /// <summary>
        /// The "game settings" button, shows the settings screen. <para/>
        /// See <seealso cref="JU_UISettings"/>
        /// </summary>
        public Button SettingsButton;

        /// <summary>
        /// The button used to go to the game main menu.
        /// </summary>
        public Button MainMenuButton;

        /// <summary>
        /// The button used to close the game application.
        /// </summary>
        public Button ExitGameButton;

        /// <summary>
        /// The game pause system.
        /// </summary>
        public JUPauseGame PauseManager
        {
            get => JUPauseGame.Instance;
        }

        private bool IsGameFocused
        {
#if UNITY_EDITOR
            get => JUEditor.IsGameFocused;
#else
            get => true;
#endif
        }

        private void Awake()
        {
            Setup();

            // Can't do it during the OnPause because the editor shows the cursor on press Escape, this break the logic.
            InvokeRepeating(nameof(CheckCursorVisibility), 0.1f, 0.1f);

            InputSystem.onEvent += OnPressSomething;
        }

        private IEnumerator Start()
        {
            yield return new WaitForEndOfFrame();
            CheckCursorVisibility();
            StartCoroutine(FixCursorVisibility());
        }

        private void OnDestroy()
        {
            Unsetup();
            InputSystem.onEvent -= OnPressSomething;
        }

        IEnumerator FixCursorVisibility()
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitUntil(() =>
            {
                return IsGameFocused;
            });

            if (!JUPauseGame.IsPaused)
            {
                Cursor.lockState = _defaultMouseLock ? CursorLockMode.Locked : CursorLockMode.None;
                Cursor.visible = _defaultMouseVisible;
            }
        }

        private void OnPressSomething(InputEventPtr eventPtr, InputDevice device)
        {
            if (!(device is Keyboard keyboard))
                return;

            if (!IsGameFocused)
            {
                StartCoroutine(FixCursorVisibility());
            }
        }

        private void Setup()
        {
            if (PauseScreen) PauseScreen.gameObject.SetActive(false);
            if (ContinueButton) ContinueButton.onClick.AddListener(OnPressContinueButton);
            if (PauseButton) PauseButton.onClick.AddListener(OnPressPauseButton);
            if (SettingsButton) SettingsButton.onClick.AddListener(OnPressSettingsButton);
            if (MainMenuButton) MainMenuButton.onClick.AddListener(OnPressMainMenuButton);
            if (ExitGameButton) ExitGameButton.onClick.AddListener(OnPressExitGameButton);

            if (PauseManager)
            {
                PauseManager.OnPause.AddListener(OnPauseGame);
                PauseManager.OnContinue.AddListener(OnContinueGame);
            }

            if (SettingsScreen)
            {
                SettingsScreen.gameObject.SetActive(false);
                SettingsScreen.OnClose.AddListener(OnCloseSettingsScreen);
            }
        }

        private void Unsetup()
        {
            if (PauseManager)
            {
                PauseManager.OnPause.RemoveListener(OnPauseGame);
                PauseManager.OnContinue.RemoveListener(OnContinueGame);
            }
        }

        private void OnCloseSettingsScreen()
        {
            if (PauseManager)
                PauseManager.ControlsEnabled = true;

            PauseScreen.gameObject.SetActive(true);
        }

        private void OnPauseGame()
        {
            if (!PauseScreen)
                return;

            JUCameraController.LockMouse(false, false);
            PauseScreen.SetActive(true);
        }

        private void OnContinueGame()
        {
            if (!PauseScreen)
                return;
                
            JUCameraController.LockMouse(Lock: _defaultMouseLock, Hide: !_defaultMouseVisible);
            PauseScreen.SetActive(false);
        }

        private void OnPressContinueButton()
        {
            JUPauseGame.Continue();
        }

        private void OnPressPauseButton()
        {
            JUPauseGame.Pause();
        }

        private void OnPressSettingsButton()
        {
            if (SettingsScreen) SettingsScreen.gameObject.SetActive(true);
            if (PauseScreen) PauseScreen.gameObject.SetActive(false);

            // Can't unpause the game if isn't on pause screen.
            if (PauseManager)
                PauseManager.ControlsEnabled = false;
        }

        private void OnPressMainMenuButton()
        {
            if (string.IsNullOrEmpty(MainMenuScene))
                return;

            SceneManager.LoadSceneAsync(MainMenuScene);

            // Disable the screen to avoid any user interaction when the game is loading another scene.
            gameObject.SetActive(false);
        }

        private void OnPressExitGameButton()
        {
            Application.Quit();
        }

        private void CheckCursorVisibility()
        {
            if (JUPauseGame.IsPaused || !IsGameFocused)
                return;

            _defaultMouseVisible = Cursor.visible;
            _defaultMouseLock = Cursor.lockState != CursorLockMode.None;
        }
    }
}