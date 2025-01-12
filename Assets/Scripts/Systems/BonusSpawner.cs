using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace CSSD
{
    public class BonusSpawner : NetworkBehaviour
    {
        [SerializeField] private List<GameObject> _bonusesList = new List<GameObject>();
        [SerializeField] private float _startSpawnChance = 10;
        [SerializeField] private float _waveChanceUp = 5;

        private float _curSpawnChance;

        private void OnValidate()
        {
            if (_startSpawnChance < 0) _startSpawnChance = 0;
            if (_startSpawnChance > 100) _startSpawnChance = 100;
            if (_waveChanceUp < 0) _waveChanceUp = 0;
        }

        private void OnEnable()
        {
            EnemyWaves.waveCountDel += SpawnChanceUp;
            BonusDealer.spawnBonusDel += SpawnBonus;
        }

        private void OnDisable()
        {
            EnemyWaves.waveCountDel -= SpawnChanceUp;
            BonusDealer.spawnBonusDel -= SpawnBonus;
        }

        private void Start()
        {
            _curSpawnChance = _startSpawnChance;
        }

        private void SpawnBonus(Vector3 pos)
        {
            if (!Object.HasStateAuthority)
                return;

            if (_curSpawnChance >= Random.Range(0, 100))
            {
                Runner.Spawn(_bonusesList[Random.Range(0, _bonusesList.Count)], pos, Quaternion.identity);
            }
        }

        private void SpawnChanceUp(int waveCount)
        {
            _curSpawnChance = _startSpawnChance + _waveChanceUp * waveCount;
            if (_curSpawnChance > 100)
                _curSpawnChance = 100;
        }
    }
}