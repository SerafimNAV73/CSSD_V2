using System;
using UnityEngine;

namespace CSSD
{
    public class BonusDealer : MonoBehaviour
    {
        private HPHandler _hpHandler;

        public static Action<Vector3> spawnBonusDel;

        private void Awake()
        {
            _hpHandler = GetComponent<HPHandler>();
        }

        private void OnEnable()
        {
            _hpHandler.SetDeath(BonusSpawn);
        }

        private void OnDisable()
        {
            _hpHandler.UnSetDeath(BonusSpawn);
        }

        private void BonusSpawn()
        {
            spawnBonusDel?.Invoke(transform.position);
        }
    }
}
