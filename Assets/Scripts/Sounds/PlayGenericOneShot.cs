using UnityEngine;
using FMODUnity;

namespace CSSD
{
    public class PlayGenericOneShot : MonoBehaviour
    {
        [SerializeField]
        private EventReference _soundEvent;

        public void PlaySoundEvent()
        {
            if (_soundEvent.IsNull) return;
            
            RuntimeManager.PlayOneShot(_soundEvent);            
        }
    }
}