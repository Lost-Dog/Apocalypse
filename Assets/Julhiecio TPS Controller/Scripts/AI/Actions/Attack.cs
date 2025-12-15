using UnityEngine;
using JUTPS.WeaponSystem;
using JUTPS;
using System.Collections.Generic;
using JU.AI;

namespace JU.CharacterSystem.AI
{
    /// <summary>
    /// Control an AI character to attack an object.
    /// </summary>
    [System.Serializable]
    public class Attack : JU_AIActionBase
    {
        /// <summary>
        /// Gun weapon attack settings.
        /// </summary>
        [System.Serializable]
        public struct GunAttackSettings
        {
            /// <summary>
            /// Actions that can be performed if the AI is closest to the target.
            /// </summary>
            public enum ClosestToTargetActions
            {
                /// <summary>
                /// Stay stoped.
                /// </summary>
                StayStoped,

                /// <summary>
                /// Flank the target by moving randomly to left or right around target position.
                /// </summary>
                FlankTarget
            }

            /// <summary>
            /// Actions that can be performed if the AI is inside attack area.
            /// </summary>
            public enum InAttackAreaActions
            {
                /// <summary>
                /// Stay stoped.
                /// </summary>
                StayStoped,

                /// <summary>
                /// Flank the target by moving randomly to left or right.
                /// </summary>
                FlankTarget
            }

            /// <summary>
            /// Fire pose conditions options.
            /// </summary>
            public enum FirePosesConditions
            {
                /// <summary>
                /// Always use fire pose.
                /// </summary>
                Always,

                /// <summary>
                /// Use fire pose only if the target is on shot distance.
                /// </summary>
                OnlyWhenOnShotDistance
            }

            /// <summary>
            /// Gun weapon shooting settings.
            /// </summary>
            [System.Serializable]
            public struct ShootingSettings
            {
                /// <summary>
                /// Precision of the shoot relative by the distance and angle to the target.
                /// </summary>
                [Min(0f)] public float Precision;

                /// <summary>
                /// The target distance allowed to shot.
                /// </summary>
                [Min(1f)] public float MaxShotDistance;

                /// <summary>
                /// Obstacles layer to not shot on walls, building or any other obstacle.
                /// </summary>
                public LayerMask ObstaclesLayer;

                /// <summary>
                /// Conditions to use fire pose.
                /// </summary>
                public FirePosesConditions FirePose;
            }

            /// <summary>
            /// The min accepted distance from target.
            /// If the AI is nearest to target, will do <see cref="IfClosestToTarget"/> action.
            /// </summary>
            [Min(1f)] public float MinDistance;

            /// <summary>
            /// The max accepted distance from target. The AI will move to the target position if the target is far away.
            /// </summary>
            [Min(2f)] public float MaxDistance;

            /// <summary>
            /// The AI will run if is distante to the destination.
            /// </summary>
            [Min(0f)] public float StartRunDistance;

            /// <summary>
            /// The stop distance if nearest from destination.
            /// </summary>
            [Min(1f)] public float StopDistance;

            /// <summary>
            /// Actions that can be performed if the AI is inside attack area.
            /// </summary>
            [Header("Conditions")]
            public InAttackAreaActions IfInAttackArea;

            /// <summary>
            /// Actions that can be performed if the AI is closest to the target.
            /// </summary>
            public ClosestToTargetActions IfClosestToTarget;

            /// <summary>
            /// Change flank direction interval when AI is on attack area if <see cref="IfInAttackArea"/> is <see cref="InAttackAreaActions.FlankTarget"/>.
            /// </summary>
            [Header("Flanking")]
            [Min(0.2f)] public float FlankRandomlyInterval;

            /// <summary>
            /// Shooting settings.
            /// </summary>
            public ShootingSettings Shooting;
        }

        /// <summary>
        ///  All melee and punch attack settings.
        /// </summary>
        [System.Serializable]
        public struct MeleeAttackSettings
        {
            /// <summary>
            /// Start run distance.
            /// </summary>
            public float StartRunDistance;

            /// <summary>
            /// Melee attack distance.
            /// </summary>
            public float AttackDistance;
        }

        private enum States
        {
            MoveToTarget,
            FlankTarget,
            StayStoped
        }

        private States _currentState;
        private Vector3 _currentMoveToPosition;
        private float _setRandomFlankDestinationTimer;

        /// <summary>
        /// All gun attack settings.
        /// </summary>
        public GunAttackSettings GunAttack;

        /// <summary>
        /// All melee and punch attack settings.
        /// </summary>
        public MeleeAttackSettings MeleeAttack;

        /// <summary>
        /// The target to attack.
        /// </summary>
        public GameObject Target { get; private set; }

        /// <summary>
        /// The target collider.
        /// </summary>
        public Collider TargetCollider { get; private set; }

        /// <summary>
        /// If <see cref="Target"/> is a character, this will return the target's character controller.
        /// </summary>
        public JUCharacterController TargetCharacter { get; private set; }

        /// <summary>
        /// The target <see cref="JUHealth"/> component.
        /// </summary>
        public JUHealth TargetHealth { get; private set; }

        /// <summary>
        /// The target position based on <see cref="TargetCollider"/> center if have or <see cref="Target"/> transform.
        /// </summary>
        public Vector3 TargetCenter
        {
            get
            {
                if (!TargetCollider)
                    return Target.transform.position;

                return TargetCollider.bounds.center;
            }
        }

        /// <summary>
        /// The more closest <see cref="Target"/> position based on target <see cref="TargetCollider"/> bounds.
        /// If not have a collider it returns the <see cref="Target"/> transform position.
        /// </summary>
        public Vector3 TargetClosestPoint
        {
            get
            {
                if (!TargetCollider)
                    return Target.transform.position;

                return TargetCollider.ClosestPoint(Ai.Center);
            }
        }

        /// <summary>
        /// The target distance.
        /// </summary>
        public float TargetDistance
        {
            get
            {
                Vector3 aiPosition = Ai.Center;

                // Use Collider.ClosestPoint for big targets.
                // This can be better if the AI must punch a car for example (the car have a big collider, it's not possible use the car transform position).
                Vector3 closestTargetPoint = TargetCollider ? TargetClosestPoint : Target.transform.position;
                closestTargetPoint.y = aiPosition.y;

                return Vector3.Distance(closestTargetPoint, aiPosition);
            }
        }

        /// <inheritdoc/>
        public Attack() : base()
        {
            GunAttack = new GunAttackSettings
            {
                MaxDistance = 20,
                MinDistance = 4,
                StartRunDistance = 7,
                StopDistance = 2,
                FlankRandomlyInterval = 2,
                IfClosestToTarget = GunAttackSettings.ClosestToTargetActions.FlankTarget,
                IfInAttackArea = GunAttackSettings.InAttackAreaActions.FlankTarget,
                Shooting = new GunAttackSettings.ShootingSettings
                {
                    MaxShotDistance = 100,
                    ObstaclesLayer = 0,
                    Precision = 15,
                    FirePose = GunAttackSettings.FirePosesConditions.Always
                }
            };

            MeleeAttack = new MeleeAttackSettings
            {
                AttackDistance = 1,
                StartRunDistance = 2
            };
        }

        /// <inheritdoc/>
        public override void Reset()
        {
            base.Reset();

            LayerMask[] defaultObstacleLayers = {
                LayerMask.NameToLayer("Default"),
                LayerMask.NameToLayer("Wall"),
                LayerMask.NameToLayer("Walls"),
                LayerMask.NameToLayer("Obstacle"),
                LayerMask.NameToLayer("Obstacles"),
                LayerMask.NameToLayer("Terrain"),
                LayerMask.NameToLayer("Character"),
                LayerMask.NameToLayer("Characters"),
                LayerMask.NameToLayer("Player"),
                LayerMask.NameToLayer("Players"),
                LayerMask.NameToLayer("Vehicle"),
                LayerMask.NameToLayer("Vehicles")
            };

            // Setup default obstacle layers.
            GunAttack.Shooting.ObstaclesLayer = 0;
            for (int i = 0; i < defaultObstacleLayers.Length; i++)
            {
                if (defaultObstacleLayers[i] != -1)
                    GunAttack.Shooting.ObstaclesLayer |= 1 << defaultObstacleLayers[i];
            }
        }

        /// <inheritdoc/>
        public override void Setup(JUCharacterAIBase ai)
        {
            base.Setup(ai);

            _currentMoveToPosition = ai.transform.position;
            _setRandomFlankDestinationTimer = GunAttack.FlankRandomlyInterval;
        }

        /// <summary>
        /// Control the AI to attack a object.
        /// </summary>
        /// <param name="target">The target to attack.</param>
        /// <param name="control">Current AI control data.</param>
        public void Update(GameObject target, ref JUCharacterAIBase.AIControlData control)
        {
            if (target != Target)
            {
                Target = target;
                if (!Target)
                {
                    TargetCharacter = null;
                    TargetCollider = null;
                    TargetHealth = null;
                    SetStoped(ref control);
                    return;
                }
                else
                {
                    TargetCollider = Target.GetComponent<Collider>();
                    TargetHealth = Target.GetComponent<JUHealth>();
                    TargetCharacter = Target.GetComponentInParent<JUCharacterController>();
                }
            }

            if (TargetHealth && TargetHealth.IsDead)
            {
                SetStoped(ref control);
                return;
            }

            if (Ai.Character.HoldableItemInUseRightHand && Ai.Character.HoldableItemInUseRightHand is Weapon)
            {
                UpdateGunMoviment(ref control);
                UpdateGunAttack(ref control);
            }
            else
                UpdateMeleeWeapon(ref control);
        }

        private void SetStoped(ref JUCharacterAIBase.AIControlData control)
        {
            control.MoveToDirection = Vector3.zero;
            control.LookToDirection = Ai.transform.forward;
            control.IsAttackPose = false;
            control.IsAttacking = false;
        }

        private void UpdateGunMoviment(ref JUCharacterAIBase.AIControlData control)
        {
            float distanceToTarget = TargetDistance;
            Vector3 aiPosition = Ai.Center;
            Vector3 targetPosition = TargetCenter;
            Vector3 directionToTarget = targetPosition - aiPosition;
            Quaternion lookToTarget = Quaternion.LookRotation(directionToTarget);
            States state = _currentState;

            // Finding the current state.

            // Closest to target.
            if (distanceToTarget < GunAttack.MinDistance)
            {
                switch (GunAttack.IfClosestToTarget)
                {
                    case GunAttackSettings.ClosestToTargetActions.StayStoped: state = States.StayStoped; break;
                    case GunAttackSettings.ClosestToTargetActions.FlankTarget: state = States.FlankTarget; break;
                    default: throw new System.InvalidOperationException();
                }
            }

            // On outside attack area.
            else if (distanceToTarget > GunAttack.MaxDistance)
                state = States.MoveToTarget;

            // On attack area.
            else
            {
                switch (GunAttack.IfInAttackArea)
                {
                    case GunAttackSettings.InAttackAreaActions.StayStoped: state = States.StayStoped; break;
                    case GunAttackSettings.InAttackAreaActions.FlankTarget: state = States.FlankTarget; break;
                    default: throw new System.InvalidOperationException();
                }
            }

            // Used only if is using navmesh:

            // Force update navmeshpath if change state to not wait the refresh timer.
            // Must be updated because delays cause innacuracy in the AI.
            bool forceUpdatePath = state != _currentState;
            _currentState = state;

            // Force generate a new random flank position.
            if (forceUpdatePath && state == States.FlankTarget)
                _setRandomFlankDestinationTimer = GunAttack.FlankRandomlyInterval;

            // Control the AI based on the current state.
            switch (state)
            {
                case States.StayStoped:

                    // Stat stoped on the current position.
                    _currentMoveToPosition = aiPosition;
                    break;
                case States.FlankTarget:

                    _setRandomFlankDestinationTimer += Time.deltaTime;
                    if (_setRandomFlankDestinationTimer > GunAttack.FlankRandomlyInterval)
                    {
                        // Try flank the target moving to left or right.

                        // Random flank direction (left or right)
                        Vector3 flankDirection = lookToTarget * Vector3.right * Mathf.Lerp(-1, 1, Mathf.RoundToInt(Random.Range(0f, 1f)));

                        // Generate a random flank position inside navmesh.
                        Vector3 flankPosition = targetPosition + (flankDirection * Mathf.Lerp(GunAttack.MinDistance, GunAttack.MaxDistance, Random.Range(0f, 1f)));

                        if (NavigationSettings.Mode == JUCharacterAIBase.NavigationModes.Simple)
                        {
                            _currentMoveToPosition = flankPosition;
                            _setRandomFlankDestinationTimer = 0;
                        }

                        else if (NavigationSettings.Mode == JUCharacterAIBase.NavigationModes.UseNavmesh)
                        {
                            // Ensure the flank position is inside the navmesh.
                            // If not, do not reset the timer to generate a new flank posision on next frame.
                            if (JU_Ai.ClosestToNavMesh(flankPosition, out flankPosition))
                            {
                                _currentMoveToPosition = flankPosition;
                                _setRandomFlankDestinationTimer = 0;
                            }
                        }
                    }

                    break;
                case States.MoveToTarget:

                    _currentMoveToPosition = targetPosition;
                    break;
                default:
                    throw new System.InvalidOperationException();
            }

            if (NavigationSettings.Mode == JUCharacterAIBase.NavigationModes.UseNavmesh)
                UpdatePathToDestination(_currentMoveToPosition, forceUpdatePath);

            Vector3 moveDirection;
            Vector3 movePosition;

            // Getting the move direction.
            switch (NavigationSettings.Mode)
            {
                case JUCharacterAIBase.NavigationModes.Simple:

                    moveDirection = _currentMoveToPosition - aiPosition;
                    movePosition = _currentMoveToPosition;
                    break;
                case JUCharacterAIBase.NavigationModes.UseNavmesh:

                    // Use the waypoint path generated by navmesh.
                    moveDirection = NavmeshPath.Count > 0 ? NavmeshPath[CurrentNavmeshWaypoint] - aiPosition : Vector3.zero;
                    movePosition = DestinationOnNavmesh;
                    break;
                default:
                    throw new System.InvalidOperationException();
            }

            float distanceToFinalDestination = Vector3.Distance(aiPosition, new Vector3(movePosition.x, aiPosition.y, movePosition.z));
            bool running = distanceToFinalDestination > GunAttack.StartRunDistance;

            if (distanceToFinalDestination < GunAttack.StopDistance)
                moveDirection = Vector3.zero;

            control.MoveToDirection = moveDirection;
            control.IsRunning = running;
        }

        private void UpdateMeleeWeapon(ref JUCharacterAIBase.AIControlData control)
        {
            Vector3 aiPosition = Ai.Center;
            _currentMoveToPosition = TargetCollider.ClosestPoint(aiPosition);

            Vector3 targetDirection = _currentMoveToPosition - aiPosition;

            Vector3 moveDirection = Vector3.zero;
            switch (NavigationSettings.Mode)
            {
                case JUCharacterAIBase.NavigationModes.Simple:
                    moveDirection = targetDirection;
                    break;
                case JUCharacterAIBase.NavigationModes.UseNavmesh:

                    UpdatePathToDestination(_currentMoveToPosition, false);

                    if (NavmeshPath.Count > 0)
                        moveDirection = NavmeshPath[CurrentNavmeshWaypoint] - aiPosition;

                    break;
                default:
                    throw new System.InvalidOperationException();
            }

            float distanceToTarget = Vector3.Distance(aiPosition, _currentMoveToPosition);
            bool running = distanceToTarget > MeleeAttack.StartRunDistance;
            bool attacking = distanceToTarget < MeleeAttack.AttackDistance;

            if (attacking)
                moveDirection = Vector3.zero;

            control.MoveToDirection = moveDirection;
            control.LookToDirection = targetDirection;
            control.IsAttackPose = false;
            control.IsRunning = running;
            control.IsAttacking = attacking;
        }

        private void UpdateGunAttack(ref JUCharacterAIBase.AIControlData control)
        {
            Vector3 weaponPosition = Ai.Character.RightHandWeapon.transform.position;
            Vector3 targetPosition = TargetCenter;

            if (TargetCharacter)
            {
                // The character capsule size is too big compared with character's size when is prone.
                // Let's set the shot position 10% upper the ground instead of use the center of the capsule as target position.
                if (TargetCharacter.IsProne)
                {
                    Bounds characterBounds = TargetCharacter.coll.bounds;
                    targetPosition = characterBounds.center + (Vector3.down * characterBounds.size.y * 0.45f);
                }
            }

            Vector3 directionToTarget = targetPosition - weaponPosition;

            bool attacking = TargetIsOnShotView(targetPosition);
            bool attackPose;

            switch (GunAttack.Shooting.FirePose)
            {
                case GunAttackSettings.FirePosesConditions.Always:
                    attackPose = true;
                    break;
                case GunAttackSettings.FirePosesConditions.OnlyWhenOnShotDistance:
                    attackPose = Vector3.Distance(weaponPosition, targetPosition) < GunAttack.Shooting.MaxShotDistance;
                    break;
                default:
                    throw new System.InvalidOperationException();
            }

            control.IsAttacking = attacking;
            control.LookToDirection = directionToTarget;
            control.IsAttackPose = attackPose;
        }

        private bool TargetIsOnShotView(Vector3 targetPosition)
        {
            Vector3 aiPosition = Ai.Center;
            Vector3 directionToTarget = Vector3.ProjectOnPlane(targetPosition - aiPosition, Vector3.up);
            float targetDistance = Vector3.Distance(aiPosition, targetPosition);

            GunAttackSettings.ShootingSettings shooting = GunAttack.Shooting;

            if (targetDistance > shooting.MaxShotDistance)
                return false;

            Vector3 aiForward = Ai.transform.forward;

            float angleToTarget = Vector3.Angle(aiForward, directionToTarget / directionToTarget.magnitude);
            float precisionByDistance = angleToTarget * targetDistance;
            if (precisionByDistance > shooting.Precision)
                return false;

            // Do not shot on other objects that isn't the target.
            if (Physics.Linecast(aiPosition, targetPosition, out RaycastHit hit, shooting.ObstaclesLayer))
            {
                if (hit.collider.gameObject != Target)
                    return false;
            }

            return true;
        }
    }
}