using System;
using UnityEngine;

namespace CSSD
{
    public class SpeedEffect : MonoBehaviour, IEffect
    {
        [SerializeField] private Settings _settings;

        public void Effect(NetworkPlayer player)
        {
            player.Movement.SpeedUpgrade(player, _settings.speedMultiplier, _settings.time, _settings.speedUIIcon);
            AudioManager._instance.WalkSpeedPickUpEvent();
        }

        [Serializable]
        public class Settings
        {
            public float speedMultiplier;
            public float time;
            public GameObject speedUIIcon;
        }
    }
}
