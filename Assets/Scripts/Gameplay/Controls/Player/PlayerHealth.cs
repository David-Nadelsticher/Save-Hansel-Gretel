using System;
using Gameplay.General.UI;
using Gameplay.UI;
using UnityEngine;

namespace Gameplay.Controls.Player
{
    [Serializable] class PlayerHealth
    {
        [SerializeField] private bool debugMode = false;
        public int CurrentHealth { get; set; }
        public int MaxHealth { get; set;}
         private int InitialHealth { get; }

         private bool PlayerDie { get; set; }


         public int CurrentEnergy { get; set; }
         public int MaxEnergy { get;set; }

        private int InitialEnergy { get; }
        private ResourceBarTracker healthBarUI;
        private ResourceBarTracker energyBarUI;

        //TODO - add constructor with Player Data
        public PlayerHealth(ResourceBarTracker healthBar, ResourceBarTracker energyBar)
        {
            CurrentEnergy = 100;
            CurrentHealth  = InitialHealth = MaxHealth = InitialEnergy = MaxEnergy = 100;
            healthBarUI = healthBar;
            energyBarUI = energyBar;
            healthBarUI.ChangeMaxAmountTo(MaxHealth);
            energyBarUI.ChangeMaxAmountTo(MaxEnergy);
        }
        public PlayerHealth(ResourceBarTracker healthBar, ResourceBarTracker energyBar,int maxHealth, int initialHealth, int maxEnergy, int initialEnergy)
        {
            MaxHealth = maxHealth;
            InitialHealth = initialHealth;
            CurrentHealth = initialHealth;
            MaxEnergy = maxEnergy;
            InitialEnergy = initialEnergy;
            CurrentEnergy = initialEnergy;
            healthBarUI = healthBar;
            energyBarUI = energyBar;
            healthBarUI.ChangeMaxAmountTo(MaxHealth);
            energyBarUI.ChangeMaxAmountTo(MaxEnergy);
        }
        

        public PlayerHealth(int maxHealth, int initialHealth, int maxEnergy, int initialEnergy)
        {
            MaxHealth = maxHealth;
            InitialHealth = initialHealth;
            CurrentHealth = initialHealth;
            MaxEnergy = maxEnergy;
            InitialEnergy = initialEnergy;
            CurrentEnergy = initialEnergy;
        }

        public void InitializePlayerHealth()
        {
            CurrentHealth = InitialHealth;
        }

        public void InitializePlayerEnergy()
        {
            CurrentEnergy = InitialEnergy;
        }


        public void IncreaseEnergy(int amount)
        {
            if (debugMode) Debug.Log($"Player took {amount} currentEnergy!");
            CurrentEnergy = Mathf.Min(amount + CurrentEnergy, MaxEnergy);
            energyBarUI.ChangeResourceByAmount(amount);

            if (debugMode) Debug.Log($"currentEnergy: {CurrentEnergy}");
        }

        public void IncreaseHealth(int amount)
        {
            if (debugMode) Debug.Log($"Player took {amount} HP");
            CurrentHealth = Mathf.Min(amount + CurrentHealth, MaxHealth);
            healthBarUI.ChangeResourceByAmount(amount);

            if (debugMode) Debug.Log($"current HP: {CurrentEnergy}");
        }

        public void DecreaseEnergy(int amount)
        {
            CurrentEnergy = Mathf.Max(CurrentEnergy - amount, 0);
            energyBarUI.ChangeResourceByAmount(-amount);

            if (debugMode) Debug.Log($"Player Decrease {amount} currentEnergy, current currentEnergy: {CurrentEnergy}");
        }

        public void DecreaseHealth(int amount)
        {
            CurrentHealth = Mathf.Max(CurrentHealth - amount, 0);
            healthBarUI.ChangeResourceByAmount(-amount);

            if (CurrentHealth == 0)
            {
                PlayerDie = true;
            }

            if (debugMode) Debug.Log($"Player Decrease {amount} HP, current HP: {CurrentHealth}");
        }
        public bool IsDead()
        {
            return PlayerDie;
        }

        public void ReturnPlayerBackToLife(bool initialHealth = true)
        {
            if (debugMode) Debug.Log($"Player Back to Life");
            if (initialHealth)
            {
                CurrentHealth = InitialHealth;
            }

            PlayerDie = false;
        }

        public bool CouldExecuteAction(int amount)
        {
            return CurrentEnergy >= amount;
        }
    }
}