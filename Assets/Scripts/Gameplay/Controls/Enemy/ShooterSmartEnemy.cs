using System.Collections;
using System.Collections.Generic;
using Core.Data;
using Core.Managers;
using Gameplay.Controls.Gun;
using Gameplay.Controls.Player;
using Gameplay.General.Utils;
using Gameplay.Providers;
using UnityEngine;

namespace Gameplay.Controls.Enemy
{
    /// <summary>
    /// ShooterSmartEnemy - Enemy that patrols, approaches, and shoots at the player.
    /// Inherits full AI FSM from SmartEnemyAIBase.
    /// </summary>
    public class ShooterSmartEnemy : SmartEnemyAIBase
    {
        #region Animator Hashes
        private static readonly int IsMoving = Animator.StringToHash("IsMoving");
        #endregion

        #region Inspector Fields
        [SerializeField] private List<ProjectileType> allowedProjectileTypes = new();

        [Header("Shooter Settings")]
        [SerializeField] private float shootInterval = 2f;
        [SerializeField] private Gun.Gun gun;

        [Header("Patrol Settings")]
        [SerializeField] private float patrolDistance = 5f;
        [SerializeField] private float minYDistanceToPlayer = 1f;
        [SerializeField] private float approachSpeed = 2f;
        #endregion

        #region Private Fields
        private float _nextShootTime;
        private Vector2 _startPosition;
        private Transform _playerCenterTransform;
        private Coroutine _returnToPoolCoroutine;
        #endregion

        #region Unity Methods
        /// <summary>
        /// Unity Start method. Initializes patrol, aim, and projectile type.
        /// </summary>
        protected override void Start()
        {
            base.Start();
            _startPosition = transform.position;
            InitializeAimPlayerCenter();
            InitializeProjectile();
        }

        /// <summary>
        /// Implements shooting logic as required by SmartEnemyAIBase FSM.
        /// </summary>
        protected override void Attack()
        {
            ShootAtPlayer();
        }

        /// <summary>
        /// On enemy death, start coroutine to return to pool after 1 second.
        /// </summary>
        protected override void Die()
        {
            base.Die();
            if (_returnToPoolCoroutine != null)
                StopCoroutine(_returnToPoolCoroutine);
            _returnToPoolCoroutine = StartCoroutine(ReturnToPoolAfterDelay(1f));
        }

        /// <summary>
        /// Reset all state for pooling.
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            _nextShootTime = 0f;
            _startPosition = transform.position;
            InitializeAimPlayerCenter();
            InitializeProjectile();
            StopReturnToPoolCoroutine();
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the projectile type for the gun from allowed types.
        /// </summary>
        private void InitializeProjectile()
        {
            if (gun != null)
            {
                // Take random projectile type from the allowed list
                if (allowedProjectileTypes.Count > 0)
                {
                    ProjectileType randomProjectileType = allowedProjectileTypes[Random.Range(0, allowedProjectileTypes.Count)];
                    gun.SetProjectileType(randomProjectileType);
                    Debug.Log($"Initialized gun with projectile type: {randomProjectileType}");
                }
                else
                {
                    Debug.LogWarning("Allowed projectile types list is empty, cannot initialize gun.");
                }
            }
        }

        /// <summary>
        /// Initializes the reference to the player's center transform for aiming.
        /// </summary>
        private void InitializeAimPlayerCenter()
        {
            if (_playerCenterTransform != null) return;
            if (playerTransform == null)
            {
                //  Debug.Log("PlayerTransform is null, cannot initialize PlayerCenterTransform.");
                return;
            }

            PlayerController playerController = playerTransform.GetComponent<PlayerController>();
            if (playerController != null)
            { 
                //Debug.Log("PlayerController found, using PlayerCenterTransform.");
                _playerCenterTransform = playerController.GetPlayerCenterTransform();
            }
            /*else
            {
                //Debug.LogWarning("PlayerController not found, using PlayerTransform directly.");
            }*/
        }
        #endregion

        #region Attack Logic
        /// <summary>
        /// Handles shooting at the player, including approach and Y alignment.
        /// </summary>
        private void ShootAtPlayer()
        {
            if (_playerCenterTransform == null)
            {
                InitializeAimPlayerCenter();
                if (_playerCenterTransform == null)
                {
                    Debug.LogWarning("PlayerCenterTransform is still null, using PlayerTransform instead.");
                    _playerCenterTransform = playerTransform;
                }
            }

            Vector2 direction = ((Vector2)_playerCenterTransform.position - (Vector2)transform.position).normalized;
            FlipSprite(direction);

            float yDistance = Mathf.Abs(_playerCenterTransform.position.y - transform.position.y);

            if (yDistance > minYDistanceToPlayer)
            {
                if (animator != null)
                    animator.SetBool(IsMoving, true);
                // Move towards player's Y position
                float step = approachSpeed * Time.deltaTime;
                float newY = Mathf.MoveTowards(transform.position.y, _playerCenterTransform.position.y, step);
                transform.position = new Vector2(transform.position.x, newY);
                return; // Don't shoot until close enough
            }

            if (gun != null)
            {
                if (Time.time >= _nextShootTime)
                {
                    PlayAttackSound();
                    gun.Shoot(direction);
                    _nextShootTime = Time.time + shootInterval;
                    if (animator != null)
                        animator.SetBool(IsMoving, false);
                }
            }
        }
        #endregion

        #region Pool Logic
        /// <summary>
        /// Coroutine: return this enemy to the pool after delay.
        /// </summary>
        private IEnumerator ReturnToPoolAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            EnemiesProvider.Instance.RemoveEnemy(this, enemyKey);
        }

        /// <summary>
        /// Stop the pool coroutine if running.
        /// </summary>
        private void StopReturnToPoolCoroutine()
        {
            if (_returnToPoolCoroutine != null)
            {
                StopCoroutine(_returnToPoolCoroutine);
                _returnToPoolCoroutine = null;
            }
        }
        #endregion

        #region Event Handling
        protected override void OnEnable()
        {
            base.OnEnable();
            InitializeAimPlayerCenter();
            EventManager.Instance.AddListener(EventNames.OnBossPhaseChange, HandleBossChangePhase);
            EventManager.Instance.AddListener(EventNames.OnBossDefeated, HandleBossChangePhase);
            EventManager.Instance.AddListener(EventNames.OnCheckpointReached, HandlePlayerReachedCheckpoint);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            StopReturnToPoolCoroutine();
            EventManager.Instance.RemoveListener(EventNames.OnBossPhaseChange, HandleBossChangePhase);
            EventManager.Instance.RemoveListener(EventNames.OnBossDefeated, HandleBossChangePhase);
            EventManager.Instance.RemoveListener(EventNames.OnCheckpointReached,HandlePlayerReachedCheckpoint);
        }

        /// <summary>
        /// Handles boss phase change event by killing this enemy.
        /// </summary>
        private void HandleBossChangePhase(object obj)
        {
            Die();
        }

        /// <summary>
        /// Handles player reaching a checkpoint; removes this enemy if behind checkpoint.
        /// </summary>
        private void HandlePlayerReachedCheckpoint(object obj)
        {
            Checkpoint.CheckpointData data = obj as Checkpoint.CheckpointData;
            if(data== null) return;
            if ((data.Position != Vector2.zero) && (data.Position.x+3 > transform.position.x))
            {
                // If player reached a checkpoint further than this enemy, return to pool
                EnemiesProvider.Instance.RemoveEnemy(this, enemyKey);
            }
        }
        #endregion

        #region Audio
        protected override void PlayAttackSound()
        {
            AudioManager.Instance.PlaySoundByAudioType(GameSoundsSo.AudioType.EnemyShoot);
        }

        protected override void PlayHitSound()
        {
            AudioManager.Instance.PlaySoundByAudioType(GameSoundsSo.AudioType.EnemyHit);
        }

        protected override void PlayDieSound()
        {
            AudioManager.Instance.PlaySoundByAudioType(GameSoundsSo.AudioType.EnemyDie);
        }
        #endregion

        #region Gizmos
        /// <summary>
        /// Draws gizmos in the Scene view to visualize detection range, patrol path, and minimum Y distance to player for shooting.
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            // Draw detection range
            Color gizmoColor = Color.yellow;
            if (playerTransform != null)
            {
                float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
                gizmoColor = distanceToPlayer <= detectionRange ? Color.red : Color.green;
            }

            Gizmos.color = gizmoColor;
            Gizmos.DrawWireSphere(transform.position, detectionRange);

            // Draw patrol path
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(_startPosition, _startPosition + Vector2.right * patrolDistance);

            // Draw minimum Y distance to player for shooting
            if (playerTransform != null)
            {
                // Draw a horizontal line at the Y position where the enemy will start shooting
                float targetY = playerTransform.position.y;
                float minY = targetY - minYDistanceToPlayer;
                float maxY = targetY + minYDistanceToPlayer;

                // Draw lines at min and max Y
                Gizmos.color = Color.magenta;
                Vector3 left = new Vector3(transform.position.x - 1, minY, 0);
                Vector3 right = new Vector3(transform.position.x + 1, minY, 0);
                Gizmos.DrawLine(left, right);

                left = new Vector3(transform.position.x - 1, maxY, 0);
                right = new Vector3(transform.position.x + 1, maxY, 0);
                Gizmos.DrawLine(left, right);

                // Draw a line from enemy to player
                Gizmos.color = Color.white;
                Gizmos.DrawLine(transform.position, playerTransform.position);
            }
        }
        #endregion
    }
}