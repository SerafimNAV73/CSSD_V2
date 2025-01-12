using Fusion;
using UnityEngine;

namespace CSSD 
{
    [RequireComponent(typeof(HPHandler))]
    [RequireComponent(typeof(NetworkObject))]
    public class DeathHandler : NetworkBehaviour
    {
        [SerializeField] private float _preDeathTime;

        private NetworkObject _networkObject;
        private HPHandler _hpHandler;

        //Timing
        TickTimer _maxPreDeathDurationTickTimer = TickTimer.None;

        private bool _isDying;

        private void Awake()
        {
            _hpHandler = GetComponent<HPHandler>();
            _networkObject = GetComponent<NetworkObject>();
        }

        public override void FixedUpdateNetwork()
        {
            if (_hpHandler.IsDead && !_isDying)
            {
                _maxPreDeathDurationTickTimer = TickTimer.CreateFromSeconds(Runner, _preDeathTime);
                _isDying = true;
            }

            if (_isDying && _maxPreDeathDurationTickTimer.Expired(Runner))
            {
                Death();
            }
        }

        private void Death()
        {
            Runner.Despawn(_networkObject);
        }
    }
}