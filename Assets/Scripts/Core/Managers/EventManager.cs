using System;
using System.Collections.Generic;

namespace Core.Managers
{ 
    public class EventManager : MonoSingleton<EventManager>
    {
        
        private Dictionary<EventNames, List<Action<object>>> _activeListeners = new();
            public void AddListener(EventNames eventName, Action<object> listener)
            {
                if (_activeListeners.TryGetValue(eventName, out var listOfEvents))
                {
                    listOfEvents.Add(listener);
                    return;
                }

                _activeListeners.Add(eventName, new List<Action<object>> { listener });
            }

            public void RemoveListener(EventNames eventName, Action<object> listener)
            {
                if (_activeListeners.TryGetValue(eventName, out var listOfEvents))
                {
                    listOfEvents.Remove(listener);

                    if (listOfEvents.Count <= 0)
                    {
                        _activeListeners.Remove(eventName);
                    }
                }
            }

            public void InvokeEvent(EventNames eventName, object obj)
            {
                if (_activeListeners.TryGetValue(eventName, out var listOfEvents))
                {
                    for (int i = 0; i < listOfEvents.Count; i++)
                    {
                        listOfEvents[i].Invoke(obj);
                    }
                }
            }
        }

        public enum EventNames
        {
            None = 0,
            OnStartGame = 1,
            OnGameOver = 3,
            OnEndGame = 4,
            OnPlayerChangePhase = 5,
            OnLayerSortRequest = 6,
            OnPlayerTakingDamage = 7,
            OnBossPhaseChange = 8,
            OnEnemyDie = 9,
            OnPlayerCollect = 10,
            OnCheckpointReached = 11,
            OnMinuteWarning = 12,
            OnLastMinuteWarning = 13,
            OnLastTenSecondsWarning = 14,
            OnTimeOver = 15,
            OnResetTimer = 16,
            OnBossDefeated = 17,
            OnBossAttack = 18,
            OnPlayerSpecialAttack = 19,
            OnPlayerDeath = 20,
            OnPlayerSaveThem = 21,
        }
    }