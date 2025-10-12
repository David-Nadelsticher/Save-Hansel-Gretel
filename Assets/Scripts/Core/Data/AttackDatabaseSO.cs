using System;
using System.Collections.Generic;
using Gameplay.Controls.Player.AttackLogic;
using UnityEngine;

namespace Core.Data
{
        [CreateAssetMenu(fileName = "AttackDatabaseSO", menuName = "Scriptable Objects/AttackCommand DatabaseSO")]
        public class AttackDatabaseSO : ScriptableObject
        {
            [SerializeField] private List<AttackEntry> attackList = new List<AttackEntry>();

            private Dictionary<AttackType, AttackData> _attackDictionary;

            private void OnEnable()
            {
                if (_attackDictionary == null || _attackDictionary.Count == 0)
                {
                    _attackDictionary = new Dictionary<AttackType, AttackData>();

                    foreach (var entry in attackList)
                    {
                        if (!_attackDictionary.ContainsKey(entry.type))
                        {
                            _attackDictionary.Add(entry.type, entry.data);
                        }
                    }
                }
            }

            public AttackData GetAttackData(AttackType type)
            {
                return _attackDictionary.TryGetValue(type, out var data) ? data : null;
            }

            public AttackCommand GetAttack(AttackType type)
            {
                var data = GetAttackData(type);
                if (data == null)
                {
                    Debug.LogWarning($"No AttackData found for type: {type}");
                    return null;
                }

                return type switch
                {
                    AttackType.Normal => new BasicAttackCommand(data),
                    _ => new SpecialAttackCommand(data)
                };
            }

        }
        [Serializable] public class AttackEntry
        {
            [SerializeField] public AttackType type;
            [SerializeField] public AttackData data;
        }
        
        public enum AttackType
        {
            Normal,
            Special
        }

        [Serializable] public class AttackData 
        {
            [SerializeField] public string attackName;
            [SerializeField] public  int damage;
            [SerializeField] public  int energy;
            //[SerializeField] public  AnimationClip animationClip;
           // [SerializeField] public  GameObject effect;

            public string AttackName => attackName;
            public int Damage => damage;
            public int Energy => energy;
            //public AnimationClip AnimationClip => animationClip;
            //public GameObject Effect => effect;
        }
}

    
