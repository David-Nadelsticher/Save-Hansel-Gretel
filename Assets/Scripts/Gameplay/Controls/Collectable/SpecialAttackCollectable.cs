using System;
using Core.Data;
using Gameplay.Controls.Player;
using UnityEngine;
using UnityEngine.Serialization;

namespace Gameplay.Controls.Collectable
{
    [Serializable]
    public class SpecialAttackCollectable : CollectableEffect
    {
        [SerializeField] private AttackType attackType;

        public SpecialAttackCollectable(AttackType attackType)
        {
            this.attackType = attackType;
        }

        public override CollectableType Type => CollectableType.SpecialAttack;

        public override void ApplyReward(PlayerController player)
        {
            player.SetSpecialAttack(attackType);
        }
    }
}