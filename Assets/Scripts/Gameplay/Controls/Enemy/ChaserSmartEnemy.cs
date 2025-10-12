
using System.Collections;
using Core.Data;
using Core.Managers;
using Gameplay.Controls.Player;
using Gameplay.General.Utils;
using Gameplay.Providers;
using UnityEditor;

using UnityEngine;


namespace Gameplay.Controls.Enemy
{
    /// <summary>
    /// ChaserSmartEnemy - Enemy that patrols, chases, and dashes to attack the player.
    /// Inherits full AI FSM from SmartEnemyAIBase.
    /// </summary>
    public class ChaserSmartEnemy : SmartEnemyAIBase
    {
        #region Animator Hashes
        private static readonly int Attack1 = Animator.StringToHash("Attack");
        private static readonly int IsMoving = Animator.StringToHash("IsMoving");
        #endregion

        #region Attack Dash State Machine
        /// <summary>
        /// Represents the phases of the dash attack state machine.
        /// </summary>
        private enum AttackPhase
        {
            None,
            Forward,
            WaitAtEnd,
            Back,
            WaitAtStart
        }
        #endregion

        #region Inspector Fields
        [Header("Chaser Settings")]
        [SerializeField] private float minDistanceToPlayer = 0.5f;
        [SerializeField] private float yAttackThreshold = 0.5f;

        [Header("Attack Settings")]
        private float _attackDistance;
        [SerializeField] private float attackInterval = 2f;
        [SerializeField] private float attackMoveSpeed = 4f;
        [SerializeField] private float attackWaitDuration = 0.2f;
        [SerializeField] private float stopThreshold = 0.1f;

        [Header("Patrol Settings")]
        [SerializeField] private float patrolDistance = 5f;
        #endregion

        #region Private Fields
        private float _lastAttackTime = -999f;
        private AttackPhase _attackPhase = AttackPhase.None;
        private float _attackPhaseTimer;
        private bool _hasDealtDamage;
        private Vector2 _attackStartPosition;
        private Coroutine _returnToPoolCoroutine;
        [SerializeField] private float stopBeforeStartingPointThreshold = 2.5f;
        [SerializeField] private float stopBeforePlayerThreshold = 2f;
        [SerializeField] private float retreatAttackMoveSpeed = 1f;
        #endregion

        #region Unity Methods
        /// <summary>
        /// Unity Start method. Initializes patrol points and attack distance.
        /// </summary>
        protected override void Start()
        {
            base.Start();
            // Patrol points are initialized in SmartEnemyAIBase
            _attackDistance = attackRange;
        }

        /// <summary>
        /// Implements attack dash logic as required by SmartEnemyAIBase FSM.
        /// </summary>
        protected override void Attack()
        {
            _attackDistance = attackRange;
            HandleAttackPhase();
        }

        /// <summary>
        /// Reset all state for pooling.
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            _attackPhase = AttackPhase.None;
            _hasDealtDamage = false;
            _lastAttackTime = -999f;
            _attackDistance = attackRange;
        }

        /// <summary>
        /// On enemy death, start coroutine to return to pool after 1 second.
        /// </summary>
        protected override void Die()
        {
            base.Die();
            StartReturnToPoolWithDelay(1f);
        }
        #endregion

        #region Attack Logic
        /// <summary>
        /// Handles the full dash attack phase state machine.
        /// </summary>
        private void HandleAttackPhase()
        {
            switch (_attackPhase)
            {
                case AttackPhase.None:
                    BeginAttackPhase();
                    break;
                case AttackPhase.Forward:
                    MoveForwardForAttack();
                    break;
                case AttackPhase.WaitAtEnd:
                    WaitAtAttackEnd();
                    break;
                case AttackPhase.Back:
                    RetreatToStartPosition();
                    break;
                case AttackPhase.WaitAtStart:
                    WaitAtStartPosition();
                    break;
            }
        }

        /// <summary>
        /// Initialize dash attack towards the player.
        /// </summary>
        private void BeginAttackPhase()
        {
            _attackStartPosition = transform.position;
            _attackDistance = Vector2.Distance(playerTransform.position, _attackStartPosition);
            _attackPhase = AttackPhase.Forward;
            animator.SetBool(IsMoving, true);
            _hasDealtDamage = false;
        }

        /// <summary>
        /// Move enemy forward for attack, deal damage if reach target.
        /// </summary>
        private void MoveForwardForAttack()
        {
            Vector2 dir = ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;
            _rb.linearVelocity = dir * attackMoveSpeed;
            FlipSprite(dir);
            if (!_hasDealtDamage && Vector2.Distance(transform.position, playerTransform.position) < stopBeforePlayerThreshold)
            {
                TryDealDamageToPlayer();
                _attackPhase = AttackPhase.WaitAtEnd;
                _attackPhaseTimer = attackWaitDuration;
                _rb.linearVelocity = Vector2.zero;
            }
        }

        /// <summary>
        /// Wait at the end of dash before retreating.
        /// </summary>
        private void WaitAtAttackEnd()
        {
            _attackPhaseTimer -= Time.deltaTime;
            if (_attackPhaseTimer <= 0f)
            {
                Vector2 dir = ((Vector2)transform.position- (Vector2)playerTransform.position).normalized;
                _attackStartPosition = (Vector2)transform.position + (dir * (stopBeforePlayerThreshold+ stopBeforeStartingPointThreshold + 0.5f));
                _attackPhase = AttackPhase.Back;
            }
        }

        /// <summary>
        /// Move back to the start position after attack.
        /// </summary>
        private void RetreatToStartPosition()
        {
            Vector2 dir = ((Vector2)_attackStartPosition - (Vector2)transform.position).normalized;
            _rb.linearVelocity = dir * retreatAttackMoveSpeed;
            if (Vector2.Distance(transform.position, _attackStartPosition) <stopBeforeStartingPointThreshold)
            {
                _attackPhase = AttackPhase.WaitAtStart;
                _attackPhaseTimer = attackInterval;
                animator.SetBool(IsMoving, false);
                _rb.linearVelocity = Vector2.zero;
            }
        }

        /// <summary>
        /// Wait at the start position before next attack.
        /// </summary>
        private void WaitAtStartPosition()
        {
            _attackPhaseTimer -= Time.deltaTime;
            if (_attackPhaseTimer <= 0f)
                _attackPhase = AttackPhase.None;
        }

        /// <summary>
        /// Deal damage to player if attack cooldown is over.
        /// </summary>
        private void TryDealDamageToPlayer()
        {
            if (Time.time >= _lastAttackTime + attackInterval)
            {
                PlayAttackSound();
                var player = playerTransform.GetComponent<PlayerController>();
                if (player != null)
                    player.TakeDamage((int)damage);

                animator?.SetTrigger(Attack1);
                _lastAttackTime = Time.time;
                _hasDealtDamage = true;
            }
        }
        #endregion

        #region State Change Logic
        /// <summary>
        /// Handles logic when the enemy state changes (FSM transitions).
        /// </summary>
        protected override void OnStateChange(EnemyState from, EnemyState to) {
            base.OnStateChange(from, to);
            if (to == EnemyState.Attack||from == EnemyState.Attack)
            {
                _attackPhase = AttackPhase.None; // Reset attack phase when entering attack state
                _hasDealtDamage = false; // Reset damage flag
                _lastAttackTime = -999f; // Reset last attack time
            }
        }
        #endregion

        #region Pool Logic
        /// <summary>
        /// Start coroutine to return this enemy to the pool after a delay.
        /// </summary>
        private void StartReturnToPoolWithDelay(float delay)
        {
            StopReturnToPoolCoroutine();
            _returnToPoolCoroutine = StartCoroutine(ReturnToPoolAfterDelay(delay));
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

        /// <summary>
        /// Coroutine: return this enemy to the pool after delay.
        /// </summary>
        private IEnumerator ReturnToPoolAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            EnemiesProvider.Instance.RemoveEnemy(this, enemyKey);
        }
        #endregion

        #region Event Handling
        protected override void OnEnable()
        {
            base.OnEnable();
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
            
            if ((data.Position != Vector2.zero) && (data.Position.x + 3 > transform.position.x))
            {               
                // If player reached a checkpoint further than this enemy, return to pool
                EnemiesProvider.Instance.RemoveEnemy(this, enemyKey);
            }
        }
        #endregion

        #region Audio
        protected override void PlayAttackSound()
        {
            AudioManager.Instance.PlaySoundByAudioType(GameSoundsSo.AudioType.EnemyAttack);
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
        /// Draws gizmos in the Scene view to visualize attack, detection, and patrol ranges.
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            Vector3 enemyPos = transform.position;
            float currentDistance = playerTransform ? Vector2.Distance(enemyPos, playerTransform.position) : 0f;

            // --- ATTACK RANGE ---
            if (playerTransform && currentDistance <= _attackDistance)
                Gizmos.color = new Color(1f, 0.15f, 0.15f, 1f); // strong red
            else
                Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.4f); // faded red

            Gizmos.DrawWireSphere(enemyPos, _attackDistance);

            // --- DETECTION RANGE ---
            if (playerTransform && currentDistance > _attackDistance && currentDistance <= detectionRange)
                Gizmos.color = new Color(1f, 0.8f, 0.15f, 1f); // strong yellow/orange
            else
                Gizmos.color = new Color(1f, 0.9f, 0.3f, 0.2f); // faded yellow

            Gizmos.DrawWireSphere(enemyPos, detectionRange);

            // --- MIN DISTANCE TO PLAYER ---
            if (playerTransform && currentDistance <= minDistanceToPlayer)
                Gizmos.color = new Color(0.15f, 1f, 1f, 1f); // strong cyan
            else
                Gizmos.color = new Color(0.3f, 1f, 1f, 0.3f); // faded cyan

            Gizmos.DrawWireSphere(enemyPos, minDistanceToPlayer);

            // --- LINE TO PLAYER ---
            if (playerTransform != null)
            {
                // Change line color according to state
                if (currentDistance <= _attackDistance)
                    Gizmos.color = Color.red;
                else if (currentDistance <= detectionRange)
                    Gizmos.color = Color.yellow;
                else
                    Gizmos.color = Color.gray;

                Gizmos.DrawLine(enemyPos, playerTransform.position);

                // Small sphere at player position
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(playerTransform.position, 0.15f);

#if UNITY_EDITOR
                // Draw distance label
                Handles.Label((enemyPos + playerTransform.position) / 2,
                    $"Distance: {currentDistance:F2}");
#endif
            }
        }
        #endregion
    }
}