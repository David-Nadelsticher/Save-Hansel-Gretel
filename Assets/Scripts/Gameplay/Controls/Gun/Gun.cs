using Gameplay.Controls.Enemy;
using Gameplay.Providers.Pool;
using UnityEngine;

namespace Gameplay.Controls.Gun
{
    /// <summary>
    /// Generic gun script for shooting projectiles with various attack patterns.
    /// Handles fire rate control, projectile instantiation, aiming, and different attack patterns.
    /// Supports single shot, spread shot, and circular attack patterns.
    /// </summary>
    public class Gun : MonoBehaviour
    {
        #region Gun Configuration
        
        [Header("Gun Settings")]
        [SerializeField] private ProjectileType projectileType;        // Type of projectile to shoot
        [SerializeField] private Transform shootPoint;                 // Point where projectiles spawn
        [SerializeField] private float fireRate = 1f;                  // Shots per second
        [SerializeField] private float projectileSpeed = 10f;          // Default projectile speed
        [SerializeField] private float angleOffset;                    // Offset angle for gun rotation
        [SerializeField] private AttackPatternEntity attackPattern;     // Attack pattern configuration
        
        #endregion

        #region Fire Rate Control
        
        [Header("Fire Rate Control")]
        [SerializeField] private bool useFireRate = true;              // Whether to use fire rate limiting
        
        /// <summary>
        /// Time when the next shot can be fired
        /// </summary>
        private float _nextFireTime;
        
        #endregion

        #region Public Shooting Interface
        
        /// <summary>
        /// Shoots a projectile in the given direction if fire rate allows.
        /// Rotates the shootPoint to face the shooting direction.
        /// </summary>
        /// <param name="direction">Normalized direction vector for projectile movement</param>
        /// <param name="customProjectileSpeed">Optional custom projectile speed override</param>
        public void Shoot(Vector2 direction, float? customProjectileSpeed = null)
        {
            // Check if we can fire based on fire rate and shoot point availability
            if ((!useFireRate || Time.time >= _nextFireTime) && shootPoint != null)
            {
                // Execute attack pattern if configured, otherwise use single shot
                if (attackPattern == null || attackPattern.attackPatternType == AttackPatternType.None)
                {
                    float speedToUse = customProjectileSpeed ?? projectileSpeed;
                    FireProjectile(direction, speedToUse);
                }
                else
                {
                    ExecuteAttackPattern(attackPattern.attackPatternType, attackPattern.attackPatternParams, direction);
                }
            }

            // Update next fire time if fire rate is enabled
            if (useFireRate)
                _nextFireTime = Time.time + 1f / fireRate;
        }

        /// <summary>
        /// Changes the projectile type used by this gun
        /// </summary>
        /// <param name="newProjectileType">The new projectile type to use</param>
        public void SetProjectileType(ProjectileType newProjectileType)
        {
            projectileType = newProjectileType;
        }

        /// <summary>
        /// Sets the attack pattern configuration for this gun
        /// </summary>
        /// <param name="newAttackPattern">The new attack pattern to use</param>
        public void SetAttackPattern(AttackPatternEntity newAttackPattern)
        {
            if (newAttackPattern == null)
            {
                Debug.LogWarning("Attempted to set a null attack pattern.");
                return;
            }

            attackPattern = newAttackPattern;
        }
        
        #endregion

        #region Aiming System
        
        /// <summary>
        /// Aims the gun at a target position with optional constraints
        /// </summary>
        /// <param name="targetPosition">World position to aim at</param>
        /// <param name="flipped">Whether the parent object is flipped (affects rotation direction)</param>
        /// <param name="minRotationAngle">Minimum rotation angle in degrees</param>
        /// <param name="maxRotationAngle">Maximum rotation angle in degrees</param>
        public void AimAt(Vector2 targetPosition, bool flipped = false, float minRotationAngle = -45f, float maxRotationAngle = 45f)
        {
            // Calculate direction from gun to target
            Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Apply flip sign if parent is flipped
            float flipSign = flipped ? -1f : 1f;

            // Calculate final angle with offset and clamping
            float finalAngle = Mathf.Clamp((angle + angleOffset) * flipSign, minRotationAngle, maxRotationAngle);

            // Apply rotation
            transform.rotation = Quaternion.Euler(0, 0, finalAngle);
        }
        
        #endregion

        #region Pool Management
        
        /// <summary>
        /// Retrieves a projectile from the object pool
        /// </summary>
        /// <returns>GameObject from pool, or null if failed</returns>
        private GameObject GetProjectileFromPool()
        {
            string projectileKey = GetProjectileKey(projectileType);
            GameObject projectileObj = ProjectilePool.Instance.GetFromPool(projectileKey).gameObject;
            
            if (projectileObj)
            {
                return projectileObj;
            }
            else
            {
                Debug.LogWarning($"Failed to get projectile of type {projectileKey} from pool.");
                return null;
            }
        }

        /// <summary>
        /// Converts projectile type enum to pool key string
        /// </summary>
        /// <param name="projectileType1">The projectile type to convert</param>
        /// <returns>String key for the object pool</returns>
        private string GetProjectileKey(ProjectileType projectileType1)
        {
            switch (projectileType)
            {
                case ProjectileType.Basic:
                    return "Basic";
                case ProjectileType.Elite:
                    return "Elite";
                case ProjectileType.ShooterChoco:
                    return "ShooterChoco";
                case ProjectileType.ShooterCandy:
                    return "ShooterCandy";
                case ProjectileType.ShotgunChoco:
                    return "ShotgunChoco";
                case ProjectileType.ShotgunCandy:
                    return "ShotgunCandy";
                default:
                    Debug.LogWarning($"Unknown projectile type: {projectileType}");
                    return "Basic"; // Fallback to Basic if unknown
            }
        }
        
        #endregion

        #region Attack Pattern Execution
        
        /// <summary>
        /// Executes different attack patterns based on the pattern type
        /// </summary>
        /// <param name="pattern">Type of attack pattern to execute</param>
        /// <param name="parameters">Parameters for the attack pattern</param>
        /// <param name="dir">Base direction for the attack</param>
        public void ExecuteAttackPattern(AttackPatternType pattern, AttackPatternParams parameters, Vector2 dir)
        {
            switch (pattern)
            {
                case AttackPatternType.Single:
                    // Single projectile in the given direction
                    FireProjectile(dir, parameters.projectileSpeed);
                    break;
                    
                case AttackPatternType.Spread:
                    // Multiple projectiles in a spread pattern
                    ExecuteSpreadPattern(parameters, dir);
                    break;
                    
                case AttackPatternType.Circular:
                    // Multiple projectiles in a circular pattern
                    ExecuteCircularPattern(parameters);
                    break;
            }
        }

        /// <summary>
        /// Executes a spread attack pattern with multiple projectiles
        /// </summary>
        /// <param name="parameters">Spread pattern parameters</param>
        /// <param name="baseDir">Base direction for the spread</param>
        private void ExecuteSpreadPattern(AttackPatternParams parameters, Vector2 baseDir)
        {
            float angleStep = parameters.spreadAngle / (parameters.projectilesPerSpread - 1);
            float startAngle = -parameters.spreadAngle / 2;
            
            for (int i = 0; i < parameters.projectilesPerSpread; i++)
            {
                float angle = startAngle + (angleStep * i);
                Vector2 direction = Quaternion.Euler(0, 0, angle) * baseDir;
                FireProjectile(direction, parameters.projectileSpeed);
            }
        }

        /// <summary>
        /// Executes a circular attack pattern with projectiles in all directions
        /// </summary>
        /// <param name="parameters">Circular pattern parameters</param>
        private void ExecuteCircularPattern(AttackPatternParams parameters)
        {
            float angleStep = 360f / parameters.projectilesPerSpread;
            
            for (int i = 0; i < parameters.projectilesPerSpread; i++)
            {
                float angle = angleStep * i;
                Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.right;
                FireProjectile(direction, parameters.projectileSpeed);
            }
        }
        
        #endregion

        #region Projectile Firing
        
        /// <summary>
        /// Creates and fires a projectile in the specified direction
        /// </summary>
        /// <param name="direction">Direction vector for projectile movement</param>
        /// <param name="speed">Speed of the projectile</param>
        private void FireProjectile(Vector2 direction, float speed)
        {
            // Get projectile from pool
            GameObject projectileObj = GetProjectileFromPool();
            if (projectileObj == null)
            {
                Debug.LogError("Failed to get projectile from pool");
                return;
            }

            // Position and orient the projectile
            projectileObj.transform.position = shootPoint.position;
            projectileObj.transform.rotation = Quaternion.identity;

            // Set projectile direction and speed
            var projectileComponent = projectileObj.GetComponent<Projectile>();
            if (projectileComponent != null)
            {
                projectileComponent.SetDirection(direction, speed);
            }
            else
            {
                // Fallback to Rigidbody2D if no Projectile component
                Rigidbody2D rb = projectileObj.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Debug.LogWarning("Projectile does not have a Projectile component, using Rigidbody2D for movement.");
                    rb.linearVelocity = direction.normalized * speed;
                }
            }
        }
        
        #endregion

        #region Debug Visualization
        
#if UNITY_EDITOR
        /// <summary>
        /// Draws debug gizmos in the scene view for gun visualization
        /// </summary>
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;

            // Draw gun position
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(transform.position, 0.1f);

            // Calculate aiming direction (with offset)
            Vector2 direction = Quaternion.Euler(0, 0, angleOffset) * Vector2.right;
            Vector3 endPos = transform.position + (Vector3)(direction * 2f); // 2 units forward

            // Draw the aiming line
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, endPos);

            // Draw label in scene view for debugging
#if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f,
                $"Aim Angle: {(transform.eulerAngles.z).ToString("F1")}°");
#endif
        }
#endif
        
        #endregion
    }
}