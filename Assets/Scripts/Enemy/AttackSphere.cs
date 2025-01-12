using Fusion;
using System.Collections.Generic;
using UnityEngine;

namespace CSSD
{
    public class AttackSphere : NetworkBehaviour
    {
        [Header("Collision detection")]
        [SerializeField] private Transform _checkForImpactPoint;
        [SerializeField] private LayerMask _hitCollisionLayers;

        [Header("Settings")]
        [SerializeField] private float _radius = 1;
        [SerializeField] private int _damage = 1;

        //Hit info
        private List<LagCompensatedHit> _hits = new List<LagCompensatedHit>();

        //Fired by info
        private NetworkObject _firedByNetworkObject;

        //Other components
        private NetworkObject _networkObject;

        public int Damage 
        { 
            get { return _damage; } 
            set { _damage = value; } 
        }

        public override void FixedUpdateNetwork()
        {
            if (!Object.HasStateAuthority)
                return;

            //Check if the rocket has hit anything
            int hitCount = Runner.LagCompensation.OverlapSphere(_checkForImpactPoint.position, _radius, default, _hits, _hitCollisionLayers, HitOptions.IncludeBox2D);

            bool isValidHit = false;

            //We've hit something, so the hit could be valid
            if (hitCount > 0)
                isValidHit = true;

            //Check waha we've hit
            for (int i = 0; i < hitCount; i++)
            {
                //Check if we hit a hitbox
                if (_hits[i].Hitbox != null)
                {
                    //Check that we didn't fire the rocket and hit ourselves. This can happen if the lag is a bit high.
                    if (_hits[i].Hitbox.Root.GetBehaviour<NetworkObject>() == _firedByNetworkObject)
                        isValidHit = false;
                }
            }

            //We hit something valid
            if (isValidHit)
            {
                //Now we need to figure out anything was within the blast radius
                hitCount = Runner.LagCompensation.OverlapSphere(_checkForImpactPoint.position, _radius, default, _hits, _hitCollisionLayers, HitOptions.None);

                //Deal damage to anything within the hit radius
                for (int i = 0; i < hitCount; i++)
                {
                    if (_hits[i].Hitbox.transform.root.TryGetComponent<HPHandler>(out var hpHandler))
                        hpHandler.OnTakeDamage(_damage);
                }

                Runner.Despawn(_networkObject);
            }
        }

        public void Fire(NetworkObject firedByNetworkObject)
        {
            _firedByNetworkObject = firedByNetworkObject;
            _networkObject = GetComponent<NetworkObject>();
        }
    }
}
