using UnityEngine;

namespace CSSD
{
    [RequireComponent(typeof(Camera))]
    public class LocalCameraHandler : MonoBehaviour
    {
        [SerializeField] private Transform _cameraAnchorPoint;

        //Other components
        private Camera _localCamera;

        public Camera LocalCamera => _localCamera;

        private void Awake()
        {
            _localCamera = GetComponent<Camera>();
        }

        private void LateUpdate()
        {
            if (_cameraAnchorPoint == null)
                return;

            if (!_localCamera.enabled)
                return;       

            //Move the camera to the position of the player
            _localCamera.transform.position = _cameraAnchorPoint.position;
        }
    }
}