using UnityEngine;

namespace Gameplay.Controls.Enemy
{
    /// <summary>
    /// Abstract base class for smart enemy AI with a finite state machine (FSM) for patrol, chase, attack, idle, and death.
    /// Handles patrol logic, state transitions, knockback, and player detection.
    /// </summary>
    public abstract class SmartEnemyAIBase : BaseEnemy
    {
        #region Inspector Fields
        [Header("Knockback Settings")]
        [SerializeField] private float knockbackDuration = 0.2f;
        #endregion

        #region Private Fields
        private float _knockbackTimer;
        private static readonly int Moving = Animator.StringToHash("IsMoving");
        #endregion

        #region Enemy State Enum
        /// <summary>
        /// States for the enemy finite state machine.
        /// </summary>
        public enum EnemyState
        {
            Idle,
            Patrol,
            Chase,
            Attack,
            Dead
        }
        #endregion

        #region AI Settings
        [Header("AI Settings")]
        [Range(0, 10)] [SerializeField] protected float patrolSpeed = 2f;
        [Range(0, 10)] [SerializeField] protected float chaseSpeed = 3f;
        [SerializeField] protected float detectionRange = 7f;
        [SerializeField] protected float attackRange = 1.2f;
        [SerializeField] protected float idleTime = 2f;
        [SerializeField] protected float maxDistanceFromPlayer = 40f;
        #endregion

        #region FSM State Fields
        protected EnemyState CurrentState = EnemyState.Idle;
        protected EnemyState PrevState = EnemyState.Idle;
        protected Vector2 PatrolPointA;
        protected Vector2 PatrolPointB;
        protected Vector2 CurrentPatrolTarget;
        protected float IdleTimer;
        protected bool FacingRight = true;
        private bool _finishedPatrol;
        private bool _finishedIdle;
        #endregion

        #region Unity Methods
        /// <summary>
        /// Unity Start method. Initializes patrol points and sets initial state.
        /// </summary>
        protected override void Start()
        {
            base.Start();
            InitializePatrolPoints();
            IdleTimer = 0f;
            CurrentState = EnemyState.Patrol;
        }

        /// <summary>
        /// Initializes patrol points for the enemy.
        /// </summary>
        private void InitializePatrolPoints()
        {
            PatrolPointA = transform.position;
            PatrolPointB = PatrolPointA + Vector2.right * 5f;
            CurrentPatrolTarget = PatrolPointB;
            _finishedPatrol = false;
        }

        /// <summary>
        /// Unity Update method. Handles FSM logic, knockback, and player distance checks.
        /// </summary>
        protected override void Update()
        {
            base.Update();

            if (isDead || playerTransform == null)
                return;

            // Handle knockback timer
            if (_knockbackTimer > 0f)
            {
                _knockbackTimer -= Time.deltaTime;
                if (animator != null) animator.SetBool(Moving, false);
                return;
            }

            // Dead zone: destroy enemy if too far from player
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer > maxDistanceFromPlayer)
            {
                Die();
                return;
            }

            // State logic
            UpdateState();

            // Handle state transitions
            if (CurrentState != PrevState)
            {
                OnStateChange(PrevState, CurrentState);
                PrevState = CurrentState;
            }

            ActAccordingToState();
        }
        #endregion

        #region FSM Logic
        /// <summary>
        /// Updates the FSM state based on player distance and priorities.
        /// </summary>
        protected virtual void UpdateState()
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

            if (isDead)
            {
                CurrentState = EnemyState.Dead;
                return;
            }

            if (CurrentState == EnemyState.Dead)
                return;
            // Priorities: Dead > Attack > Chase > Patrol/Idle
            if (!_playerDead && distanceToPlayer <= attackRange)
            {
                CurrentState = EnemyState.Attack;
            }
            else if (!_playerDead && distanceToPlayer <= detectionRange)
            {
                CurrentState = EnemyState.Chase;
            }
            else if (CurrentState == EnemyState.Patrol && _finishedPatrol)
            {
                CurrentState = EnemyState.Idle;
                _finishedPatrol = false;
            }
            else if (CurrentState == EnemyState.Idle && _finishedIdle)
            {
                CurrentState = EnemyState.Patrol;
                _finishedIdle = false;
            }
        }

        /// <summary>
        /// Called when the FSM state changes. Handles state entry logic.
        /// </summary>
        /// <param name="from">Previous state.</param>
        /// <param name="to">New state.</param>
        protected virtual void OnStateChange(EnemyState from, EnemyState to)
        {
            if (to == EnemyState.Idle)
            {
                IdleTimer = 0f;
            }

            if (to == EnemyState.Patrol)
            {
                CurrentPatrolTarget = (CurrentPatrolTarget == PatrolPointA) ? PatrolPointB : PatrolPointA;
            }

            if (to == EnemyState.Dead)
            {
                _rb.linearVelocity = Vector2.zero;
            }

            if (to == EnemyState.Attack)
            {
                _rb.linearVelocity = Vector2.zero;
            }
        }

        /// <summary>
        /// Executes logic based on the current FSM state.
        /// </summary>
        protected virtual void ActAccordingToState()
        {
            switch (CurrentState)
            {
                case EnemyState.Idle:
                    Idle();
                    break;
                case EnemyState.Patrol:
                    Patrol();
                    break;
                case EnemyState.Chase:
                    Chase();
                    break;
                case EnemyState.Attack:
                    Attack();
                    break;
                case EnemyState.Dead:
                    // Do nothing
                    break;
            }
        }
        #endregion

        #region State Behaviors
        /// <summary>
        /// Idle state: stops movement and waits for idleTime.
        /// </summary>
        protected virtual void Idle()
        {
            if (animator != null)
                animator.SetBool(Moving, false);

            _rb.linearVelocity = Vector2.zero;
            IdleTimer += Time.deltaTime;

            if (IdleTimer >= idleTime)
                _finishedIdle = true;
        }

        /// <summary>
        /// Patrol state: moves between patrol points.
        /// </summary>
        protected virtual void Patrol()
        {
            Vector2 dir = (CurrentPatrolTarget - (Vector2)transform.position).normalized;
            _rb.linearVelocity = dir * patrolSpeed;
            if (animator != null)
                animator.SetBool(Moving, true);

            FlipSprite(dir);

            if (Vector2.Distance(transform.position, CurrentPatrolTarget) < 0.2f)
            {
                _finishedPatrol = true;
            }
        }

        /// <summary>
        /// Handles collision with patrol obstacles (e.g., trees).
        /// </summary>
        protected override void OnTriggerEnter2D(Collider2D other)
        {
            base.OnTriggerEnter2D(other);
            if (other.CompareTag("Tree"))
            {
                if (CurrentState == EnemyState.Patrol)
                {
                    CurrentPatrolTarget = (CurrentPatrolTarget == PatrolPointA) ? PatrolPointB : PatrolPointA;
                }
            }
        }

        /// <summary>
        /// Chase state: moves toward the player.
        /// </summary>
        protected virtual void Chase()
        {
            Vector2 dir = ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;
            _rb.linearVelocity = dir * chaseSpeed;
            if (animator != null)
            {
                animator.SetBool(Moving, true);
            }

            FlipSprite(dir);
        }

        /// <summary>
        /// Attack state: must be implemented by derived classes.
        /// </summary>
        protected abstract void Attack();
        #endregion

        #region Utility
        /// <summary>
        /// Flips the enemy sprite based on movement direction.
        /// </summary>
        /// <param name="direction">Direction to face.</param>
        protected void FlipSprite(Vector2 direction)
        {
            bool shouldFaceRight = direction.x > 0;
            if (shouldFaceRight != FacingRight)
            {
                FacingRight = shouldFaceRight;
                Vector3 localScale = transform.localScale;
                localScale.x = Mathf.Abs(localScale.x) * (FacingRight ? 1 : -1);
                transform.localScale = localScale;
            }
        }
        #endregion

        #region Pooling & Reset
        /// <summary>
        /// Resets the enemy to its initial state for pooling.
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            CurrentState = EnemyState.Patrol;
            PrevState = EnemyState.Idle;
            IdleTimer = 0f;
            InitializePatrolPoints();
        }
        #endregion

        #region Damage & Knockback
        /// <summary>
        /// Handles taking damage and applies knockback.
        /// </summary>
        /// <param name="damageAmount">Amount of damage taken.</param>
        public override void TakeDamage(float damageAmount)
        {
            base.TakeDamage(damageAmount);

            if (playerTransform != null && _rb != null)
            {
                _knockbackTimer = knockbackDuration;
                Vector2 knockbackDir = ((Vector2)transform.position - (Vector2)playerTransform.position).normalized;
                _rb.linearVelocity = Vector2.zero;
                _rb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
            }
        }
        #endregion

        #region Player Death Handling
        /// <summary>
        /// Handles player death event and resets enemy state.
        /// </summary>
        /// <param name="obj">Event parameter.</param>
        protected override void UpdatePlayerDeadStatus(object obj)
        {
            base.UpdatePlayerDeadStatus(obj);
            CurrentState = EnemyState.Patrol;
            PrevState = EnemyState.Idle;
            IdleTimer = 0f;
            InitializePatrolPoints();
        }
        #endregion
    }
}