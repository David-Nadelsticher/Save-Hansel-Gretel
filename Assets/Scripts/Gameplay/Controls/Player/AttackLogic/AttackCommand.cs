using Core.Data;
using Gameplay.Controls.Enemy;
using UnityEngine;

namespace Gameplay.Controls.Player.AttackLogic
{
    public abstract class AttackCommand
    {
        protected AttackData data;
        public AttackData Data => data;

        public AttackCommand(AttackData data)
        {
            this.data = data;
        }

        public abstract void Execute();

        public abstract void Execute(Collider2D enemyCollider);
    }

    public class BasicAttackCommand : AttackCommand
    {
        public BasicAttackCommand(AttackData data) : base(data)
        {
        }

        public override void Execute()
        {
            //Debug.Log($"[NORMAL] {data.AttackName} - Damage: {data.Damage}");
        }

        public override void Execute(Collider2D enemyCollider)
        {
            if (enemyCollider == null) return;
            //check if the collider has a BaseEnemy component
            var enemy = enemyCollider.GetComponent<HealthComponent>();
            if (enemy != null)
            {
                enemy.TakeDamage(data.Damage);
                //Debug.Log($"Attacked {enemy.name} with {data.AttackName} for {data.Damage} damage.");
            }
            else
            {
                //Debug.LogWarning("No enemy found to attack.");
            }
        }
    }

    public class SpecialAttackCommand : AttackCommand
    {
        public SpecialAttackCommand(AttackData data) : base(data)
        {
        }

        public override void Execute()
        {
            //Debug.Log($"[SPECIAL] {data.AttackName} - Damage: {data.Damage} (Energy: {data.Energy})");
        }

        public override void Execute(Collider2D enemyCollider)
        {
            //Debug.Log($"[SPECIAL] {data.AttackName} - Damage: {data.Damage} (Energy: {data.Energy})");
            if (enemyCollider == null) return;
            //check if the collider has a BaseEnemy component
            var enemy = enemyCollider.GetComponent<HealthComponent>();
            if (enemy != null)
            {
                enemy.TakeDamage(data.Damage);
                //Debug.Log($"Attacked {enemy.name} with {data.AttackName} for {data.Damage} damage.");
            }
            /*else
            {
                //Debug.LogWarning("No enemy found to attack.");
            }*/
        }
    }
}