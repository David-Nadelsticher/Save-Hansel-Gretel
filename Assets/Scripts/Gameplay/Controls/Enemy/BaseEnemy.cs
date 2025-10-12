
using UnityEngine;
using BossProject.Core;
using Core.Managers;
using Gameplay.Controls.Player;
using Gameplay.Providers.Pool;

namespace Gameplay.Controls.Enemy
{
    /// <summary>
    /// Abstract base class for all enemy types. Handles health, damage, death, player detection, pooling, and basic animation/physics logic.
    /// </summary>
    public abstract class BaseEnemy : BaseMono, IPoolable
    {
        #region Animator Hashes
        private static readonly int Die1 = Animator.StringToHash("Die");
        private static readonly int IsMoving = Animator.StringToHash("IsMoving");
        private static readonly int Hit = Animator.StringToHash("Hit");
        private static readonly int Attack = Animator.StringToHash("Attack");
        #endregion

        #region Inspector Fields
        [Header("Enemy Stats")]
        [SerializeField] protected float moveSpeed = 5f;
        [SerializeField] protected float damage = 10f;
        [SerializeField] protected HealthComponent healthComponent;

        [Header("Optional Components")]
        [SerializeField] protected SpriteRenderer spriteRenderer;
        [SerializeField] protected Animator animator;
        [SerializeField] protected string enemyKey;
        [SerializeField] protected float knockbackForce;
        [SerializeField] private float showHealthBarRange = 8f; // Range at which the health bar is shown
        [SerializeField] private float thresholdShowHealthBar = 4f; // Threshold for showing health bar
        [SerializeField] protected GameObject healthBarUI; // Health bar for the enemy
        #endregion

        #region Protected Fields
        protected bool isDead;
        protected Transform playerTransform;
        protected Rigidbody2D _rb;
        protected bool _playerDead;
        #endregion

        #region Unity Methods
        /// <summary>
        /// Unity Update method. Handles health bar visibility and skips logic if dead.
        /// </summary>
        protected virtual void Update()
        {
            if (isDead)
                return;
            if (playerTransform != null)
            {
                float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
                HandleShowHealthBar(distanceToPlayer);
            }
        }

        /// <summary>
        /// Unity Start method. Initializes player transform and subscribes to health events.
        /// </summary>
        protected virtual void Start()
        {
            InitializePlayerTransform();
            SubscribeHealthComponent();
        }

        /// <summary>
        /// Unity OnEnable method. Initializes rigidbody, subscribes to health and player death events.
        /// </summary>
        protected virtual void OnEnable()
        {
            InitializeRigidbody();
            SubscribeHealthComponent();
            EventManager.Instance.AddListener(EventNames.OnPlayerDeath, UpdatePlayerDeadStatus);
        }

        /// <summary>
        /// Unity OnDisable method. Unsubscribes from health and player death events.
        /// </summary>
        protected virtual void OnDisable()
        {
            UnsubscribeHealthComponent();
            EventManager.Instance.RemoveListener(EventNames.OnPlayerDeath, UpdatePlayerDeadStatus);
        }
        #endregion

        #region Health & Damage
        /// <summary>
        /// Called when the enemy takes damage. Plays hit sound and delegates to HealthComponent.
        /// </summary>
        /// <param name="damageAmount">Amount of damage taken.</param>
        public virtual void TakeDamage(float damageAmount)
        {
            PlayHitSound();
            healthComponent?.TakeDamage(damageAmount);
        }

        /// <summary>
        /// Called when the health component signals damage. Plays hit animation and applies knockback.
        /// </summary>
        /// <param name="damageAmount">Amount of damage taken (optional).</param>
        protected virtual void OnDamaged(float damageAmount = 0f)
        {
            if (animator != null)
                animator.SetTrigger(Hit);

            // Knockback logic
            if (playerTransform != null && _rb != null)
            {
                // Calculate direction from player to enemy (opposite direction)
                Vector2 knockbackDir = ((Vector2)transform.position - (Vector2)playerTransform.position).normalized;
                _rb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
            }
            // Override in derived classes for specific damage behavior
        }

        /// <summary>
        /// Called when the health component signals death. Plays death animation, disables physics, and triggers events.
        /// </summary>
        protected virtual void Die()
        {
            isDead = true;
            EventManager.Instance.InvokeEvent(EventNames.OnEnemyDie, this);
            if (animator != null)
            {
                animator.SetTrigger(Die1);
            }
            DisablePhysics();
            // Override in derived classes for specific death behavior
        }
        #endregion

        #region Health Component Subscription
        /// <summary>
        /// Subscribes to health component events.
        /// </summary>
        private void SubscribeHealthComponent()
        {
            if (healthComponent != null)
            {
                healthComponent.OnDamaged += OnDamaged;
                healthComponent.OnDeath += Die;
            }
        }

        /// <summary>
        /// Unsubscribes from health component events.
        /// </summary>
        private void UnsubscribeHealthComponent()
        {
            if (healthComponent != null)
            {
                healthComponent.OnDamaged -= OnDamaged;
                healthComponent.OnDeath -= Die;
            }
        }
        #endregion

        #region Player & Rigidbody Initialization
        /// <summary>
        /// Finds and sets the player transform (root parent) by tag.
        /// </summary>
        private void InitializePlayerTransform()
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (playerTransform != null)
            {
                // Climb to root parent if needed
                while (playerTransform.parent != null)
                {
                    playerTransform = playerTransform.parent;
                }
            }
        }

        /// <summary>
        /// Initializes the Rigidbody2D component and sets physics properties.
        /// </summary>
        private void InitializeRigidbody()
        {
            if (_rb == null)
            {
                _rb = GetComponent<Rigidbody2D>() ?? gameObject.AddComponent<Rigidbody2D>();
                _rb.gravityScale = 0;
                _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                _rb.freezeRotation = true;
            }
            EnablePhysics();
        }
        #endregion

        #region Physics Control
        /// <summary>
        /// Enables dynamic physics for the enemy.
        /// </summary>
        private void EnablePhysics()
        {
            if (_rb != null)
            {
                _rb.bodyType = RigidbodyType2D.Dynamic;
            }
        }

        /// <summary>
        /// Disables physics and stops movement for the enemy.
        /// </summary>
        private void DisablePhysics()
        {
            if (_rb != null)
            {
                _rb.linearVelocity = Vector2.zero;
                _rb.bodyType = RigidbodyType2D.Kinematic; // Disable physics interactions
            }
        }
        #endregion

        #region Player Death Handling
        /// <summary>
        /// Handles player death event.
        /// </summary>
        /// <param name="obj">Event parameter.</param>
        protected virtual void UpdatePlayerDeadStatus(object obj)
        {
            _playerDead = true;
        }
        #endregion

        #region Health Bar UI
        /// <summary>
        /// Shows or hides the health bar UI based on player distance.
        /// </summary>
        /// <param name="distanceToPlayer">Distance from enemy to player.</param>
        private void HandleShowHealthBar(float distanceToPlayer)
        {
            if (distanceToPlayer < showHealthBarRange)
            {
                if (healthBarUI != null && !healthBarUI.activeSelf)
                {
                    healthBarUI.SetActive(true);
                    healthComponent.Heal(1f); // Ensure bar is updated visually
                }
            }
            else
            {
                if ((distanceToPlayer >= showHealthBarRange + thresholdShowHealthBar) && (healthBarUI != null) && (healthBarUI.activeSelf))
                {
                    healthBarUI.SetActive(false);
                }
            }
        }
        #endregion

        #region Pooling & Reset
        /// <summary>
        /// Resets the enemy to its initial state for pooling.
        /// </summary>
        public virtual void Reset()
        {
            isDead = false;
            ResetHealth();
            ResetPhysics();
            ResetAnimator();
            if (healthBarUI != null)
                healthBarUI.SetActive(false);
        }

        /// <summary>
        /// Resets the health component.
        /// </summary>
        private void ResetHealth()
        {
            healthComponent?.ResetHealth();
        }

        /// <summary>
        /// Resets physics state (override for custom logic).
        /// </summary>
        protected virtual void ResetPhysics()
        {
            // Override in derived classes if needed
        }

        /// <summary>
        /// Resets animator state and triggers.
        /// </summary>
        protected virtual void ResetAnimator()
        {
            if (animator != null)
            {
                animator.SetBool(IsMoving, false);
                animator.ResetTrigger(Hit);
                animator.ResetTrigger(Die1);
                animator.ResetTrigger(Attack);
            }
        }
        #endregion

        #region Collision & Attack
        /// <summary>
        /// Handles collision with the player and triggers attack logic.
        /// </summary>
        /// <param name="other">Collider2D that entered the trigger.</param>
        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                // Deal damage to player if they have a health component
                var playerHealth = other.GetComponentInParent<PlayerController>();
                if (playerHealth != null)
                {
                    AttackPlayer(playerHealth);
                }
            }
        }

        /// <summary>
        /// Attacks the player (override in derived classes for custom logic).
        /// </summary>
        /// <param name="player">PlayerController to attack.</param>
        protected virtual void AttackPlayer(PlayerController player)
        {
            // Override in derived classes for specific attack behavior
        }
        #endregion

        #region Audio Hooks
        /// <summary>
        /// Plays the attack sound (override in derived classes).
        /// </summary>
        protected virtual void PlayAttackSound() { }
        /// <summary>
        /// Plays the hit sound (override in derived classes).
        /// </summary>
        protected virtual void PlayHitSound() { }
        /// <summary>
        /// Plays the die sound (override in derived classes).
        /// </summary>
        protected virtual void PlayDieSound() { }
        #endregion
    }
}