using System;
using BossProject.Core;
using Core.Managers;
using Unity.Cinemachine;
using UnityEngine;

namespace Gameplay.General.Utils
{
    public class CameraShakeTrigger:BaseMono
    {
        [Serializable]
        public class ShakeSettings
        {
           // public string name;
            public CinemachineImpulseDefinition impulseDefinition;
            public float intensity = 1f;
            public float duration = 0.5f;
        }

        public CinemachineImpulseSource impulseSource;
        public ShakeSettings defaultShake;
        public ShakeSettings bossAttackShake;
        public ShakeSettings bossDeathShake;
        public ShakeSettings playerSpecialAttackShake;





        private void HandleBossDeathShake(object obj)
        {
            TriggerShake(bossDeathShake);
        }
        
        private void HandleBossAttackShake(object obj)
        {
            TriggerShake(bossAttackShake);
        }
        
        private void HandlePlayerSpecialAttackShake(object obj)
        {
            TriggerShake(playerSpecialAttackShake);
        }


        private void OnEnable()
        {
            EventManager.Instance.AddListener(EventNames.OnBossDefeated, HandleBossDeathShake);
            EventManager.Instance.AddListener(EventNames.OnBossAttack, HandleBossAttackShake);
            EventManager.Instance.AddListener(EventNames.OnPlayerSpecialAttack, HandlePlayerSpecialAttackShake);
        } private void OnDisable()
        {
            EventManager.Instance.RemoveListener(EventNames.OnBossDefeated, HandleBossDeathShake);
            EventManager.Instance.RemoveListener(EventNames.OnBossAttack, HandleBossAttackShake);
            EventManager.Instance.RemoveListener(EventNames.OnPlayerSpecialAttack, HandlePlayerSpecialAttackShake);
        }


        private void TriggerShake(ShakeSettings settings, float overrideIntensity = -1f)
        {
            if (impulseSource == null || settings == null || settings.impulseDefinition == null)
            {
                Debug.LogWarning("ShakeTrigger: Missing impulse source or definition.");
                return;
            }

            impulseSource.ImpulseDefinition = settings.impulseDefinition;

            float finalIntensity = overrideIntensity >= 0 ? overrideIntensity : settings.intensity;

            impulseSource.GenerateImpulse(Vector3.one * finalIntensity);
        }
    }
}

