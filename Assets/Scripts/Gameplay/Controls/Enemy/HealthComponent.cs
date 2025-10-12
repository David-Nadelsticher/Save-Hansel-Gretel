
using Gameplay.General.UI;
using UnityEngine;

namespace Gameplay.Controls.Enemy
{
    /// <summary>
    /// Component that manages health, damage, healing, and death logic for an entity.
    /// Handles UI updates and event callbacks for damage and death.
    /// </summary>
    public class HealthComponent : MonoBehaviour
    {
        #region Inspector Fields
        [SerializeField] private float maxHealth = 100f;
        public float MaxHealth => maxHealth;

        [SerializeField] private float currentHealth;
        public float CurrentHealth => currentHealth;

        [SerializeField] private ResourceBarTracker healthBarUI; // Reference to the health bar UI
        #endregion

        #region Public Properties
        public bool IsDead { get; private set; }
        #endregion

        #region Events
        public System.Action<float> OnDamaged; // Invoked when damaged (passes new health)
        public System.Action OnDeath; // Invoked when dead
        #endregion

        #region Unity Methods
        /// <summary>
        /// Unity Awake method. Initializes health and death state.
        /// </summary>
        private void Awake()
        {
            currentHealth = maxHealth;
            IsDead = false;
        }

        /// <summary>
        /// Unity Start method. Initializes the health bar UI.
        /// </summary>
        private void Start()
        {
            healthBarUI.ChangeMaxAmountTo((int)maxHealth);
        }
        #endregion

        #region Health Logic
        /// <summary>
        /// Applies damage to the entity, updates UI, and triggers events.
        /// </summary>
        /// <param name="damage">Amount of damage to apply.</param>
        public void TakeDamage(float damage)
        {
            if (IsDead) return;

            currentHealth -= damage;
            healthBarUI.ChangeResourceByAmount(-(int)damage);

            if (currentHealth <= 0)
            {
                currentHealth = 0;
                Die();
                return;
            }

            OnDamaged?.Invoke(currentHealth);
        }

        /// <summary>
        /// Heals the entity by a given amount, up to max health, and updates UI.
        /// </summary>
        /// <param name="amount">Amount to heal.</param>
        public void Heal(float amount)
        {
            if (IsDead) return;

            currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
            healthBarUI.ChangeResourceByAmount((int)amount);
        }

        /// <summary>
        /// Kills the entity, sets IsDead, and triggers death event.
        /// </summary>
        private void Die()
        {
            IsDead = true;
            OnDeath?.Invoke();
        }

        /// <summary>
        /// Resets health to max and updates the health bar UI.
        /// </summary>
        public void ResetHealth()
        {
            IsDead = false;
            currentHealth = maxHealth;
            healthBarUI.gameObject.SetActive(true);
            healthBarUI.ResetWithoutAnimation((int)currentHealth - 1, (int)maxHealth, 1000);
            healthBarUI.ChangeMaxAmountTo((int)maxHealth);
        }
        #endregion
    }
}