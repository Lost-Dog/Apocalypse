using UnityEngine;

namespace CompassNavigatorProDemos {
	public class RandomCameraPatrol : MonoBehaviour {
		public float radius = 25f;
		public float moveSpeed = 5f;
		public float turnSpeed = 120f;
		public float arriveDistance = 0.5f;
		public float minTargetDistance = 2f;

		Vector3 startPosition;
		Vector3 targetPosition;
		float arriveDistanceSqr;
		float minTargetDistanceSqr;

		void Awake() {
			CacheParameters();
			startPosition = transform.position;
			PickNewTarget(true);
		}

		void OnEnable() {
			startPosition = transform.position;
			PickNewTarget(true);
		}

		void OnValidate() {
			if (radius < 0f) radius = 0f;
			if (moveSpeed < 0f) moveSpeed = 0f;
			if (turnSpeed < 0f) turnSpeed = 0f;
			if (arriveDistance < 0f) arriveDistance = 0f;
			if (minTargetDistance < 0f) minTargetDistance = 0f;
			CacheParameters();
		}

		void Update() {
			if (moveSpeed <= 0f) return;

			Vector3 position = transform.position;
			Vector3 toTarget = targetPosition - position;
			if (toTarget.sqrMagnitude <= arriveDistanceSqr) {
				PickNewTarget(false);
				toTarget = targetPosition - position;
			}

			if (toTarget.sqrMagnitude > 0f) {
				Quaternion desiredRotation = Quaternion.LookRotation(toTarget);
				float maxRadians = turnSpeed * Mathf.Deg2Rad * Time.deltaTime * 50f;
				transform.rotation = Quaternion.RotateTowards(transform.rotation, desiredRotation, maxRadians);
				transform.position = Vector3.MoveTowards(position, targetPosition, moveSpeed * Time.deltaTime);
			}
		}

		void CacheParameters() {
			arriveDistanceSqr = arriveDistance * arriveDistance;
			minTargetDistanceSqr = minTargetDistance * minTargetDistance;
		}

		void PickNewTarget(bool forceDistance) {
			Vector3 position = transform.position;
			for (int i = 0; i < 10; i++) {
				Vector2 offset = Random.insideUnitCircle * radius;
				targetPosition = startPosition + new Vector3(offset.x, 0f, offset.y);
				targetPosition.y = startPosition.y;

				if (!forceDistance || minTargetDistanceSqr <= 0f) {
					break;
				}

				Vector3 delta = targetPosition - position;
				if (delta.sqrMagnitude >= minTargetDistanceSqr) {
					break;
				}
			}
		}
	}
}
