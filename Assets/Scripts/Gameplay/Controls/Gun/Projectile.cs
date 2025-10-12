using System.Collections;
using BossProject.Core;
using Gameplay.Controls.Player;
using Gameplay.Providers.Pool;
using Unity.Cinemachine;
using UnityEngine;

namespace Gameplay.Controls.Gun
{
    /// <summary>
    /// Represents a projectile that can be fired from weapons.
    /// Handles movement, collision detection, damage dealing, and visual effects.
    /// Implements IPoolable for object pooling support.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class Projectile : BaseMono, IPoolable
    {
        #region Serialized Fields

        [Header("Lifetime Settings")]
        [Tooltip("How long the projectile lives before auto-destruction (seconds)")]
        [SerializeField, Range(0.1f, 30f)] private float lifeTime = 5f;
        [Space]
        [Header("Damage Settings")]
        [Tooltip("Damage dealt to targets on hit")]
        [SerializeField, Range(1, 100)] private int damage = 1;
        [Tooltip("Layers this projectile can hit (e.g., Player, Environment)")]
        [SerializeField] private LayerMask hitLayers;
        [Space]
        [Header("Pooling Settings")]
        [Tooltip("Key for object pooling system to identify this projectile type")]
        [SerializeField] private string projectileKey;
        [Space]
        [Header("Visual Effects")]
        [Tooltip("Particle effect played when projectile hits something")]
        [SerializeField] private ParticleSystem hitEffect;
        [Tooltip("Trail effect shown during projectile movement")]
        [SerializeField] private GameObject trailEffect;
        [Tooltip("Sprite renderer for controlling projectile's visual appearance")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        #endregion

        #region Private Fields
        
        /// <summary>
        /// Coroutine managing the projectile's lifetime
        /// </summary>
        private Coroutine _lifeTimeCoroutine;
        
        /// <summary>
        /// Rigidbody2D component for physics-based movement
        /// </summary>
        private Rigidbody2D _rb;
        
        /// <summary>
        /// Current movement direction of the projectile
        /// </summary>
        private Vector2 _direction = Vector2.right;
        
        /// <summary>
        /// Flag to prevent multiple hits from the same projectile
        /// </summary>
        private bool _isAlreadyHit;
        
        #endregion

        #region Unity Callbacks
        
        /// <summary>
        /// Initializes the projectile's physics components
        /// </summary>
        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0; // Disable gravity for consistent movement
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // Better collision detection
        }

        /// <summary>
        /// Called when the projectile is enabled (from pool)
        /// </summary>
        void OnEnable()
        {
            // Projectile is ready for use
        }

        /// <summary>
        /// Called when the projectile is disabled (returned to pool)
        /// </summary>
        void OnDisable()
        {
            // Stop lifetime coroutine if it's running
            if (_lifeTimeCoroutine != null)
            {
                StopCoroutine(_lifeTimeCoroutine);
                _lifeTimeCoroutine = null;
            }
        }
        
        #endregion

        #region Initialization
        
        /// <summary>
        /// Sets the projectile's movement direction and speed
        /// </summary>
        /// <param name="direction">Normalized direction vector</param>
        /// <param name="projectileSpeed">Speed of the projectile</param>
        public void SetDirection(Vector2 direction, float projectileSpeed = 10f)
        {
            // Start lifetime countdown if not already running
            if (_lifeTimeCoroutine == null) 
                _lifeTimeCoroutine = StartCoroutine(StartlifeTimeProjectileCycle());
            
            // Set movement direction and velocity
            _direction = direction.normalized;
            _rb.linearVelocity = _direction * projectileSpeed;
        }

        /// <summary>
        /// Coroutine that manages the projectile's lifetime
        /// </summary>
        /// <returns>IEnumerator for coroutine execution</returns>
        private IEnumerator StartlifeTimeProjectileCycle()
        {
            yield return new WaitForSeconds(lifeTime);
            _lifeTimeCoroutine = null;
            ProjectilePool.Instance.ReturnToPool(projectileKey, this);
        }
        
        #endregion

        #region Collision Detection and Damage
        
        /// <summary>
        /// Handles collision detection with targets
        /// </summary>
        /// <param name="collision">The collider that was hit</param>
        private void OnTriggerEnter2D(Collider2D collision)
        {
            // Prevent multiple hits from the same projectile
            if (_isAlreadyHit) return;
            
            // Check for player collision
            if (collision.CompareTag("Player"))
            {
                HandlePlayerHit(collision);
            }
            // Check for environmental collision (trees, walls, etc.)
            else if (collision.CompareTag("Tree"))
            {
                HandleEnvironmentHit();
            }
        }

        /// <summary>
        /// Handles collision with player targets
        /// </summary>
        /// <param name="collision">The player collider that was hit</param>
        private void HandlePlayerHit(Collider2D collision)
        {
            var health = collision.GetComponentInParent<PlayerController>();
            if (health != null)
            {
                // Deal damage to player
                health.TakeDamage(damage);
                _isAlreadyHit = true; // Prevent further hits
                
                // Stop lifetime coroutine and handle hit effects
                StopLifetimeCoroutine();
                PlayHitEffectAndReturnToPool();
            }
        }

        /// <summary>
        /// Handles collision with environmental objects
        /// </summary>
        private void HandleEnvironmentHit()
        {
            StopLifetimeCoroutine();
            _isAlreadyHit = true; // Prevent further hits
            PlayHitEffectAndReturnToPool();
        }

        /// <summary>
        /// Stops the lifetime coroutine if it's running
        /// </summary>
        private void StopLifetimeCoroutine()
        {
            if (_lifeTimeCoroutine != null)
            {
                StopCoroutine(_lifeTimeCoroutine);
                _lifeTimeCoroutine = null;
            }
        }
        
        #endregion

        #region Visual Effects and Pool Management
        
        /// <summary>
        /// Plays hit effects and returns the projectile to the pool
        /// </summary>
        private void PlayHitEffectAndReturnToPool()
        {
            // Hide the projectile sprite
            if (spriteRenderer != null) 
                spriteRenderer.enabled = false;
            
            // Stop movement
            _rb.linearVelocity = Vector2.zero;

            // Play hit effect if available
            if (hitEffect != null)
            {
                hitEffect.Play();
                
                // Disable trail effect during hit animation
                if (trailEffect != null) 
                    trailEffect.SetActive(false);
                
                // Return to pool after effect finishes
                StartCoroutine(ReturnToPoolAfterEffect());
            }
            else
            {
                // Return to pool immediately if no hit effect
                ProjectilePool.Instance.ReturnToPool(projectileKey, this);
            }
        }

        /// <summary>
        /// Returns the projectile to the pool after hit effects finish
        /// </summary>
        /// <returns>IEnumerator for coroutine execution</returns>
        private IEnumerator ReturnToPoolAfterEffect()
        {
            // Wait for hit effect to complete plus a small buffer
            yield return new WaitForSeconds(hitEffect.main.duration + 0.1f);
            ProjectilePool.Instance.ReturnToPool(projectileKey, this);
        }
        
        #endregion

        #region Public Methods
        [ContextMenu("Reset Projectile State")]
        public void Reset()
        {
            // Reset hit state
            _isAlreadyHit = false;
            
            // Reset physics
            _rb.linearVelocity = Vector2.zero;
            _direction = Vector2.right;
            
            // Reset visual elements
            if (spriteRenderer != null) 
                spriteRenderer.enabled = true;
            
            if (trailEffect != null) 
                trailEffect.SetActive(true);
            
            // Stop and clear hit effects
            if (hitEffect != null) 
                hitEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        #endregion

        #region Private Methods
        #endregion

        #region Helpers
        #endregion
    }
}