using UnityEngine;
using Fusion;

namespace CSSD
{
    public class TimeoutHandler : NetworkBehaviour
    {
        [SerializeField] private float _lifeTime = 1;

        [Networked] private TickTimer life { get; set; }

        private void OnValidate()
        {
            if (_lifeTime < 0) _lifeTime = 0;
        }

        public override void Spawned()
        {
            life = TickTimer.CreateFromSeconds(Runner, _lifeTime);
        }

        public override void FixedUpdateNetwork()
        {
            if (life.Expired(Runner))
            {
                Debug.Log("Lightning despawned");
                Runner.Despawn(Object);
            }        
        }
    }
}
