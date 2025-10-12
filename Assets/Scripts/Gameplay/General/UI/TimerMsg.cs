using Core.Data;
using Core.Managers;
using UnityEngine;

namespace Gameplay.General.UI
{
    [RequireComponent(typeof(AnimatedMessageDisplayer))]
    public class TimerMsg : MonoBehaviour
    {
        [SerializeField] private AnimatedMessageDisplayer messageDisplayer;
        [SerializeField] private Color32 warningColor = new Color32(220, 10, 10, 255);
        private void ShowTimerWarning(int minutesLeft)
        {
            if (minutesLeft > 1)
            {
                AudioManager.Instance.PlaySoundByAudioType(GameSoundsSo.AudioType.TimerWarning);
                string msg = $"{minutesLeft} minutes left!";
                messageDisplayer.ShowMessage(msg, warningColor);
            }
        }

        // Show a message for the last minute 
        private void ShowLastMinuteMsg()
        {
            AudioManager.Instance.PlaySoundByAudioType(GameSoundsSo.AudioType.TimerWarning);
            messageDisplayer.ShowMessage("Last minute left!", warningColor);
        }
        
        private void OnEnable()
        {
            EventManager.Instance.AddListener(EventNames.OnMinuteWarning, obj => ShowTimerWarning((int)obj));
            EventManager.Instance.AddListener(EventNames.OnLastMinuteWarning, _ => ShowLastMinuteMsg());
        }

        private void OnDisable()
        {
            EventManager.Instance.RemoveListener(EventNames.OnMinuteWarning, obj => ShowTimerWarning((int)obj));
            EventManager.Instance.RemoveListener(EventNames.OnLastMinuteWarning, _ => ShowLastMinuteMsg());
            messageDisplayer.StopCurrentRoutine();
        }
    }
    
}