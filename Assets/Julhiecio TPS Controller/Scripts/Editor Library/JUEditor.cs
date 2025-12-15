#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using UnityEngine.InputSystem;

namespace JU.Editor
{
    /// <summary>
    /// Editor utilities for JU systems.
    /// </summary>
    public static class JUEditor
    {
        private class GameFocusChecker : MonoBehaviour
        {
            private float _focusDelayTimer = 0f;
            private const float FocusDelay = 0.3f;

            public bool IsFocused { get; private set; }

#if UNITY_EDITOR
            private void Awake()
            {
                IsFocused = CheckGameFocus();
            }

            void Update()
            {
                bool focused = CheckGameFocus();
                if (!focused)
                {
                    _focusDelayTimer = 0f;
                    IsFocused = false;
                }
                else
                {
                    if (_focusDelayTimer < FocusDelay)
                    {
                        _focusDelayTimer += Time.unscaledDeltaTime;
                        if (_focusDelayTimer >= FocusDelay)
                            IsFocused = true;
                    }
                }
            }

            private bool CheckGameFocus()
            {
                if (!UnityEditorInternal.InternalEditorUtility.isApplicationActive)
                    return false;

                if (!EditorWindow.focusedWindow)
                    return false;

                EditorWindow gameWindow = null;
                if (EditorWindow.focusedWindow.titleContent.text.Equals("Game"))
                {
                    gameWindow = EditorWindow.focusedWindow;
                }

                if (!gameWindow)
                    return false;

                Vector2 mousePos = Mouse.current.position.value;
                Vector2 screenSize = new Vector2(Screen.width, Screen.height);

                if (mousePos.x < 0 || mousePos.y < 0)
                    return false;

                if (mousePos.x > screenSize.x || mousePos.y > screenSize.y)
                    return false;

                return true;
            }
#endif
        }

        private static Camera _sceneViewCamera;
        private static GameFocusChecker _focusCheckerInstance;

        public static bool IsGameFocused
        {
            get
            {
#if UNITY_EDITOR
                if (_focusCheckerInstance == null)
                    _focusCheckerInstance = new GameObject("Game Focus Checker").AddComponent<GameFocusChecker>();

                return _focusCheckerInstance.IsFocused;
#else
                return true;
#endif
            }
        }

        /// <summary>
        /// The editor viewport camera.
        /// </summary>
        public static Camera SceneViewCamera
        {
            get
            {
#if UNITY_EDITOR
                if (!_sceneViewCamera)
                    _sceneViewCamera = SceneView.lastActiveSceneView.camera;

                return _sceneViewCamera;
#else
                throw new System.Exception("Allowed only on editor.");
#endif
            }
        }

        /// <summary>
        /// The editor viewport position that can be used to spawn new gameObjects on user position.
        /// </summary>
        /// <returns></returns>
        public static Vector3 SceneViewSpawnPosition()
        {
#if UNITY_EDITOR
            if (!SceneViewCamera)
                return Vector3.zero;

            Ray ray = SceneViewCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

            if (Physics.Raycast(ray, out RaycastHit hit))
                return hit.point;

            return SceneViewCamera.transform.position + (SceneViewCamera.transform.forward * 10);
#else
            throw new System.Exception("Allowed only on editor.");
#endif
        }

        /// <summary>
        /// The editor viewport rotation that can be used to spawn new gameObjects on user position.
        /// </summary>
        /// <returns></returns>
        public static Quaternion SceneViewSpawnRotation()
        {
#if UNITY_EDITOR
            if (!SceneViewCamera)
                return Quaternion.identity;

            return Quaternion.Euler(Vector3.up * SceneViewCamera.transform.eulerAngles.y);

#else
            throw new System.Exception("Allowed only on editor.");
#endif
        }
    }
}