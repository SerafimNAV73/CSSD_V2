using System;
using UnityEngine;

namespace CSSD
{
    public class HealEffect : MonoBehaviour, IEffect
    {
        [SerializeField] private Settings _settings;

        public void Effect(NetworkPlayer player)
        {
            player.Health.OnHeal(_settings._healValue);
            AudioManager._instance.HealPickUpEvent();
        }

        [Serializable]
        public class Settings
        {
            public int _healValue;
        }
    }
}