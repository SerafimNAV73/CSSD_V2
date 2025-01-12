using System;
using UnityEngine;

namespace CSSD
{
    public class DamageEffect : MonoBehaviour, IEffect
    {
        [SerializeField] private Settings _settings;

        public void Effect(NetworkPlayer player)
        {
            player.Weapon.OnDamageUpgrade(player, _settings.damageMultiplier, _settings.time, _settings.damageUIIcon);
            AudioManager._instance.DamagePickUpEvent();
        }

        [Serializable]
        public class Settings
        {
            public float damageMultiplier;
            public float time;
            public GameObject damageUIIcon;
        }
    }
}
