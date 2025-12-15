using JUTPS.JUInputSystem;
using UnityEngine;

namespace JUTPS
{
	/// <summary>
	/// Stores informations about player and platform input system.
	/// </summary>
	[AddComponentMenu("JU TPS/Gameplay/Game/Game Manager")]
	public class JUGameManager : MonoBehaviour
	{
		private static JUCharacterController _playerController;

		[SerializeField] private bool _simulateMobileDevice;

		/// <summary>
		/// The player controll instance.
		/// If null, will try find and return a <see cref="JUCharacterController"/> with tag "Player".
		/// </summary>
		public static JUCharacterController PlayerController
		{
			get
			{
				if (!_playerController)
				{
					GameObject playerObject = GameObject.FindWithTag("Player");

					if (playerObject)
						_playerController = playerObject.GetComponent<JUCharacterController>();
				}

				return _playerController;
			}
			set => _playerController = value;
		}

		/// <summary>
		/// The main instance.
		/// </summary>
		public static JUGameManager Instance { get; private set; }

		/// <summary>
		/// Return true if is using touch inputs.
		/// </summary>
		public static bool IsMobileControls { get; private set; }

		private void Awake()
		{
			if (Instance && Instance != this)
			{
				Destroy(this);
				return;
			}

			Instance = this;
#if UNITY_ANDROID && !UNITY_EDITOR
			_simulateMobileDevice = SystemInfo.deviceType == DeviceType.Handheld;
#endif
			IsMobileControls = _simulateMobileDevice;
		}

		private void Update()
		{
			IsMobileControls = _simulateMobileDevice;

		}

		private void OnDestroy()
		{
			PlayerController = null;
		}
	}
}