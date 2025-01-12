using Fusion;
using System.Collections.Generic;
using UnityEngine;

namespace CSSD
{
    public class PlayerLifeMonitor : NetworkBehaviour
    {
        [SerializeField] private List<NetworkPlayer> _alifePlayers = new List<NetworkPlayer>();

        private void OnEnable()
        {
            EnemyBehaviour.getRandomPlayerDel += GetRandomPlayer;
            EnemyBehaviour.getClosestPlayerDel += GetClosestPlayer;
            NetworkPlayer.playerDeathDel += RemovePlayer;
            NetworkPlayer.playerReviveDel += AddPlayer;
        }

        private void OnDisable()
        {
            EnemyBehaviour.getRandomPlayerDel -= GetRandomPlayer;
            EnemyBehaviour.getClosestPlayerDel -= GetClosestPlayer;
            NetworkPlayer.playerDeathDel -= RemovePlayer;
            NetworkPlayer.playerReviveDel -= AddPlayer;
        }

        public override void FixedUpdateNetwork()
        {
            if (!Object.HasStateAuthority)
                return;

            if (GameManager._instance.CurStatus != GameStatus.WaveStart)
                return;

            if (_alifePlayers.Count == 0)
            {
                RPC_GameOver();
            }
        }

        public void AddPlayer(NetworkPlayer player)
        {
            if (!player.Health.IsDead)
            {
                _alifePlayers.Add(player);
            }
        }

        public void RemovePlayer(NetworkPlayer player)
        {
            _alifePlayers.Remove(player);
        }

        private NetworkPlayer GetRandomPlayer()
        {
            if (_alifePlayers.Count == 0)
                return null;
            else
                return _alifePlayers[Random.Range(0, _alifePlayers.Count)];
        }

        private NetworkPlayer GetClosestPlayer(Transform t)
        {
            if (_alifePlayers.Count == 0)
                return null;
            else
            {
                NetworkPlayer closestPlayer = _alifePlayers[0];
                for (int i = 0; i < _alifePlayers.Count; i++)
                {
                    float newDist = Vector3.Distance(_alifePlayers[i].transform.position, t.position);
                    float oldDist = Vector3.Distance(closestPlayer.transform.position, t.position);
                    if (newDist < oldDist)
                    {
                        closestPlayer = _alifePlayers[i];
                    }
                }
                return closestPlayer;
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_GameOver()
        {
            GameManager._instance.SetGameStatus(GameStatus.GameOver);
        }
    }
}