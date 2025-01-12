using System;
using UnityEngine;

namespace CSSD
{
    public class ArmorEffect : MonoBehaviour, IEffect
    {
        [SerializeField] private Settings _settings;

        public void Effect(NetworkPlayer player)
        {
            player.Health.DamageReductionUpgrade(player, _settings.value, _settings.time, _settings.armorUIIcon);
            AudioManager._instance.ArmorPickUpEvent();
        }

        [Serializable]
        public class Settings
        {
            public float time;
            public int value;
            public GameObject armorUIIcon;
        }
    }
}