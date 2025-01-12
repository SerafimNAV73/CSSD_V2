using System;
using UnityEngine;

namespace CSSD
{
    public class ReloadEffect : MonoBehaviour, IEffect
    {
        [SerializeField] private Settings _settings;

        public void Effect(NetworkPlayer player)
        {
            player.Weapon.OnReloadUpgrade(player, _settings.reloadMultiplier, _settings.time, _settings.reloadUIIconl);
            AudioManager._instance.ReloadPickUpEvent();
        }

        [Serializable]
        public class Settings
        {
            public float reloadMultiplier;
            public float time;
            public GameObject reloadUIIconl;
        }
    }
}