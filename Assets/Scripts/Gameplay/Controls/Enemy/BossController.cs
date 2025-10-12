using System;
using Core.Data;
using UnityEngine;
using Core.Managers;
using Gameplay.Controls.Gun;

namespace Gameplay.Controls.Enemy
{
    /// <summary>
    /// Controls the boss enemy's behavior, including phase transitions, attacks, patrol, and death logic.
    /// Handles attack patterns, movement, and event triggers for the boss.
    /// </summary>
    public class BossController : BaseEnemy
    {
        #region Constants
        private static readonly int IsMoving = Animator.StringToHash("IsMoving");
        #endregion

        #region Boss Configuration
        /// <summary>
        /// Configuration for each boss phase, including health threshold, attack pattern, and effects.
        /// </summary>
        [Serializable]
        public class BossConfig
        {
            public float healthPercentageThreshold; // Health % to trigger this phase
            public string phaseName; // Name of the phase
            public AttackPatternEntity attackPatternEntity; // Attack pattern for this phase
            public float attackInterval = 2f; // Time between attacks
            public ParticleSystem attackEffect; // Visual effect for attack
            public GameObject powerEffect; // Power-up effect for this phase
            public ProjectileType projectileType; // Projectile type for this phase
        }

        [Header("Boss Configuration")]
        //[SerializeField] private GameObject projectilePrefab; // Prefab for boss projectiles
        [SerializeField] private ParticleSystem phaseTransitionEffect; // Effect for phase transitions
        [SerializeField] private Gun.Gun gun; // Reference to the boss's gun
        [SerializeField] private BossConfig[] bossConfigs; // Array of phase configurations
        #endregion

        #region Debug
        [Header("Debug")]
        [SerializeField] private bool debugMode; // Enable debug logs
        #endregion

        #region Attack Patterns
        [Header("Attack Patterns")]
        //[SerializeField] private int projectilesPerSpread = 8; // Number of projectiles in a spread
        //[SerializeField] private float spreadAngle = 360f; // Angle of spread for projectiles
        #endregion

        #region Patrol Variables
        [Header("Patrol Settings")]
        [SerializeField] private Vector2 patrolDistance = new Vector2(5f, 0f); // Distance to patrol
        [SerializeField] private float patrolSpeed = 2f; // Speed of patrol
        [SerializeField] private float detectionRange; // Range to detect player
        private Vector2 _patrolTarget; // Current patrol target
        #endregion

        #region Phase State
        private int _currentPhaseIndex; // Current phase index
        private float _nextAttackTime; // Time for next attack
        private float _nextSpawnTime; // (Unused) Time for next spawn
        private Vector2 _startPosition; // Starting position for patrol
        private bool _facingRight = true; // Is boss facing right
        [SerializeField] private float minDistanceToPlayer = 7f; // Minimum distance to maintain from player
        #endregion

        #region Unity Methods
        /// <summary>
        /// Unity Start method. Initializes boss phase, rigidbody, and patrol points.
        /// </summary>
        protected override void Start()
        {
            base.Start();
            // Sort phases by health percentage in descending order
            Array.Sort(bossConfigs,
                (a, b) => b.healthPercentageThreshold.CompareTo(a.healthPercentageThreshold));

            _startPosition = transform.position;
            _patrolTarget = _startPosition + (Vector2.right * patrolDistance);
            // Start the first phase
            if (bossConfigs.Length > 0)
            {
                _currentPhaseIndex = 0;
                ProjectileType projectileType = bossConfigs[_currentPhaseIndex].projectileType;
                gun.SetProjectileType(projectileType);
                gun.SetAttackPattern(bossConfigs[_currentPhaseIndex].attackPatternEntity);
                if (debugMode)
                    Debug.Log(
                        $"[BossController] Initialize Gun with attack pattern: {bossConfigs[_currentPhaseIndex].attackPatternEntity.attackPatternType}");
            }
            else
            {
                if (debugMode) Debug.LogWarning("[BossController] No health phases defined.");
            }
        }

        /// <summary>
        /// Unity Update method. Delegates logic to SRP-compliant handlers.
        /// </summary>
        protected override void Update()
        {
            base.Update();
            if (isDead || playerTransform == null) return;
            HandlePhaseTransition();
            HandleBossMovement();
            HandleAttacks();
        }
        #endregion

        #region Phase Management
        /// <summary>
        /// Handles phase transitions based on health.
        /// </summary>
        private void HandlePhaseTransition()
        {
            CheckPhaseTransition();
        }

        /// <summary>
        /// Checks if the boss should transition to the next phase based on health.
        /// </summary>
        private void CheckPhaseTransition()
        {
            if (healthComponent == null) return;
            float healthPercentage = (healthComponent.CurrentHealth / healthComponent.MaxHealth) * 100f;
            // Check if we should transition to the next phase
            if (_currentPhaseIndex + 1 < bossConfigs.Length &&
                healthPercentage <= bossConfigs[_currentPhaseIndex + 1].healthPercentageThreshold)
            {
                PlayPhaseTransitionSound();
                // Deactivate previous phase's power effect and activate the next
                bossConfigs[_currentPhaseIndex].powerEffect?.SetActive(false);
                bossConfigs[_currentPhaseIndex + 1].powerEffect?.SetActive(true);
                if (phaseTransitionEffect != null)
                {
                    phaseTransitionEffect.Play();
                }

                if (debugMode)
                    Debug.Log(
                        $"[BossController] Transitioning to Phase {_currentPhaseIndex + 1} at {healthPercentage}% health");
                EventManager.Instance.InvokeEvent(EventNames.OnBossPhaseChange,
                    bossConfigs[_currentPhaseIndex + 1].phaseName);
                if (gun != null)
                {
                    gun.SetProjectileType(bossConfigs[_currentPhaseIndex + 1].projectileType);
                    gun.SetAttackPattern(bossConfigs[_currentPhaseIndex + 1].attackPatternEntity);
                }

                _currentPhaseIndex++;
            }
        }

        /// <summary>
        /// Plays the sound for phase transition.
        /// </summary>
        private void PlayPhaseTransitionSound()
        {
            AudioManager.Instance.PlaySoundByAudioType(GameSoundsSo.AudioType.BossPhaseChange);
        }

        /// <summary>
        /// Plays the sound for boss attack.
        /// </summary>
        protected override void PlayAttackSound()
        {
            AudioManager.Instance.PlaySoundByAudioType(GameSoundsSo.AudioType.BossAttack);
        }

        /// <summary>
        /// Plays the sound for boss death.
        /// </summary>
        protected override void PlayDieSound()
        {
            base.PlayDieSound();
            AudioManager.Instance.PlaySoundByAudioType(GameSoundsSo.AudioType.BossDie);
        }
        #endregion

        #region Patrol Logic
        /// <summary>
        /// Handles boss patrol movement and updates movement based on player distance.
        /// </summary>
        private void HandleBossMovement()
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            UpdateMovement(distanceToPlayer);
        }

        /// <summary>
        /// Handles patrol movement and flipping when reaching patrol points.
        /// </summary>
        private void Patrol()
        {
            float direction = _facingRight ? 1 : -1;
            _rb.linearVelocity = new Vector2(direction * patrolSpeed, 0);
            if (_facingRight && transform.position.x >= _patrolTarget.x)
                FlipPatrol();
            else if (!_facingRight && transform.position.x <= _startPosition.x)
                FlipPatrol();
            animator?.SetBool(IsMoving, true);
        }

        /// <summary>
        /// Flips the enemy direction for patrolling.
        /// </summary>
        private void FlipPatrol()
        {
            _facingRight = !_facingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1;
            transform.localScale = localScale;
        }
        #endregion

        #region Attack Logic
        /// <summary>
        /// Handles boss attack logic, including attack interval and triggering effects.
        /// </summary>
        private void HandleAttacks()
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            if (!_playerDead&&distanceToPlayer <= detectionRange && Time.time >= _nextAttackTime)
            {
                bossConfigs[_currentPhaseIndex].attackEffect?.Play();
                EventManager.Instance.InvokeEvent(EventNames.OnBossAttack, bossConfigs[_currentPhaseIndex].phaseName);
                Attack();
                _nextAttackTime = Time.time + bossConfigs[_currentPhaseIndex].attackInterval;
            }
        }

        /// <summary>
        /// Executes the attack pattern based on the current phase.
        /// </summary>
        private void Attack()
        {
            PlayAttackSound();
            if (debugMode) Debug.Log($"[BossController] Attack in phase {_currentPhaseIndex}");
            // Implement different attack patterns based on the current phase
            Vector2 direction = (playerTransform.position - transform.position).normalized;
            gun.Shoot(direction);
        }
        #endregion

        #region Damage & Death
        /// <summary>
        /// Called when the boss takes damage. Triggers hit animation.
        /// </summary>
        protected override void OnDamaged(float damageAmount = 0f)
        {
            base.OnDamaged(damageAmount);
            if (debugMode) Debug.Log($"[BossController] Damaged: {damageAmount}");
        }

        /// <summary>
        /// Called when the boss dies. Triggers death animation and clears enemies.
        /// </summary>
        protected override void Die()
        {
            base.Die();
            EventManager.Instance.InvokeEvent(EventNames.OnBossDefeated, null);
            if (debugMode) Debug.Log("[BossController] Boss died");
            Destroy(this.gameObject, 1f); // Delay to allow death animation to play
            // Optional: Trigger game win condition
        }

        /// <summary>
        /// Updates boss movement based on distance to player, including patrol, approach, and strafing.
        /// </summary>
        /// <param name="distanceToPlayer">Distance from boss to player.</param>
        private void UpdateMovement(float distanceToPlayer)
        {
            if (distanceToPlayer > detectionRange)
            {
                Patrol();
                return;
            }

            Vector2 direction = ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;
            if (distanceToPlayer > minDistanceToPlayer)
            {
                _rb.linearVelocity = direction * moveSpeed;
                FlipSprite(direction);
            }
            else
            {
                // When in range, strafe around the player
                Vector2 strafeDirection = new Vector2(-direction.y, direction.x);
                _rb.linearVelocity = strafeDirection * moveSpeed;
                // Update sprite direction
                if (MathF.Abs(transform.position.y - playerTransform.position.y) < 0.5f)
                {
                    FlipSprite(direction);
                }
            }

            // Update animation
            if (animator != null)
            {
                animator.SetBool(IsMoving, _rb.linearVelocity.magnitude > 0.1f);
            }
        }

        /// <summary>
        /// Flips the boss sprite based on movement direction.
        /// </summary>
        /// <param name="direction">Direction to face.</param>
        protected void FlipSprite(Vector2 direction)
        {
            bool shouldFaceRight = direction.x > 0;
            if (shouldFaceRight != _facingRight)
            {
                _facingRight = shouldFaceRight;
                Vector3 localScale = transform.localScale;
                localScale.x = Mathf.Abs(localScale.x) * (_facingRight ? 1 : -1);
                transform.localScale = localScale;
            }
        }
        #endregion

#if UNITY_EDITOR
        #region Gizmos
        /// <summary>
        /// Draws gizmos for patrol path, detection range, and attack patterns in the editor.
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            // Different colors for different phases
            Color color = Color.red;
            switch (_currentPhaseIndex)
            {
                case 0: color = Color.red; break;
                case 1: color = Color.yellow; break;
                case 2: color = Color.cyan; break;
            }

            Gizmos.color = color;
            if (playerTransform != null)
            {
                Gizmos.DrawLine(transform.position, playerTransform.position);
            }

            // Draw spread or circular attack pattern gizmos based on phase
            if (_currentPhaseIndex == 2) // Spread
            {
                var spreadAngle = bossConfigs[_currentPhaseIndex].attackPatternEntity.attackPatternParams.spreadAngle;
                var projectilesPerSpread = bossConfigs[_currentPhaseIndex].attackPatternEntity.attackPatternParams.projectilesPerSpread;
                Vector2 baseDirection = playerTransform != null
                    ? ((Vector2)playerTransform.position - (Vector2)transform.position).normalized
                    : Vector2.right;
                float angleStep = spreadAngle / (projectilesPerSpread - 1);
                float startAngle = -spreadAngle / 2;
                for (int i = 0; i < projectilesPerSpread; i++)
                {
                    float angle = startAngle + (angleStep * i);
                    Vector2 dir = Quaternion.Euler(0, 0, angle) * baseDirection;
                    Gizmos.DrawRay(transform.position, dir * 3f);
                }
            }
            else if (_currentPhaseIndex == 1) // Circular
            {
                var projectilesPerSpread = bossConfigs[_currentPhaseIndex].attackPatternEntity.attackPatternParams.projectilesPerSpread;
                float angleStep = 360f / projectilesPerSpread;
                for (int i = 0; i < projectilesPerSpread; i++)
                {
                    float angle = angleStep * i;
                    Vector2 dir = Quaternion.Euler(0, 0, angle) * Vector2.right;
                    Gizmos.DrawRay(transform.position, dir * 3f);
                }
            }

            // Draw patrol path gizmo
            Gizmos.color = Color.green;
            Gizmos.DrawLine(_startPosition, _patrolTarget);
            Gizmos.DrawSphere(_startPosition, 0.2f);
            Gizmos.DrawSphere(_patrolTarget, 0.2f);
            // Draw detection range gizmo with color change if player detected
            if (playerTransform != null)
            {
                float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
                if (distanceToPlayer <= detectionRange)
                    Gizmos.color = Color.red; // Player detected
                else
                    Gizmos.color = Color.magenta; // Player not detected
            }
            else
            {
                Gizmos.color = Color.magenta;
            }

            Gizmos.DrawWireSphere(transform.position, detectionRange);
        }
        #endregion
#endif
    }
}