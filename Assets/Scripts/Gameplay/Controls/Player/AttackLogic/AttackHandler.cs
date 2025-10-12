using Core.Data;
using UnityEngine;

namespace Gameplay.Controls.Player.AttackLogic
{
    public class AttackHandler
    {
        private readonly AttackCommand _basicAttackCommand;
        private AttackCommand _currentSpecialAttackCommand;
        private readonly AttackDatabaseSO _provider;

        public AttackHandler(AttackDatabaseSO database)
        {
            _provider = database;
            _basicAttackCommand = _provider.GetAttack(AttackType.Normal);
            _currentSpecialAttackCommand = _provider.GetAttack(AttackType.Special);
        }

        public void SetSpecialAttack(AttackType type)
        {
            //Debug.Log($"Player took {type} Skill");
            _currentSpecialAttackCommand = _provider.GetAttack(type);
        }

        public void PerformNormalAttack()
        {
            _basicAttackCommand?.Execute();
        }

        public bool CanUseSpecialAttack(int currentEnergy)
        {
            return currentEnergy >= _currentSpecialAttackCommand.Data.Energy;
        }

        public int GetSpecialAttackEnergyCost()
        {
            return _currentSpecialAttackCommand.Data.Energy;
        }

        public void PerformSpecialAttack(Collider2D enemyCollider, Vector2 attackPosition)
        {
            if (_currentSpecialAttackCommand == null) return;
            
            
            // Execute the special attack command
            _currentSpecialAttackCommand.Execute(enemyCollider);
        }
        public void PerformNormalAttack(Collider2D enemyCollider, Vector2 attackPosition)
        {
            
            // Execute the special attack command
            _basicAttackCommand?.Execute(enemyCollider);
        }

        
        public void PerformSpecialAttack()
        {
            _currentSpecialAttackCommand?.Execute();
            
        }
    }
}