//using Zenject;
using UnityEngine;

namespace CSSD
{
    public class LevelUpHealth :  MonoBehaviour, ILevelUp
    {
        [SerializeField] private int _hplevelCup = 10;

        private int _minLevel;
        public int MinLevel => _minLevel;

        public void LevelUp(NetworkPlayer player, int level)
        {
            _minLevel++;
            if (player.CurrentLevel >= MinLevel)
            {
                player.Health.OnMaxHPRaise(_hplevelCup * _minLevel);
            }
        }

        private void Start()
        {
            _minLevel = 1;
        }
    }
}
