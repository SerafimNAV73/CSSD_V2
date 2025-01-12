using Fusion;
using UnityEngine;

namespace CSSD
{
    public class MoveRight : NetworkBehaviour
    {
        [SerializeField] private float _speed = 5;
        public override void FixedUpdateNetwork()
        {
            transform.position += _speed * transform.right * Runner.DeltaTime;
        }
    }
}