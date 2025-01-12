using UnityEngine;
using UnityEngine.UI;

namespace CSSD {
    [RequireComponent(typeof(Image))]
    public class UIBonusIcon : MonoBehaviour
    {
        private float _maxTime;
        private float _curTime;

        private bool _isStarted;

        private Image _image;

        private void Start()
        {
            CheckImage();
        }

        private void Update()
        {
            UpdateTimer();
        }

        private void UpdateTimer()
        {
            if (_isStarted)
            {
                if (_curTime > 0)
                {
                    _curTime -= Time.deltaTime;
                }
                else
                {
                    Destroy(gameObject);
                }

                _image.fillAmount = _curTime / _maxTime;
            }
        }

        private void CheckImage()
        {
            if (!TryGetComponent(out _image))
                Debug.Log("Lost image component");
            else
                _image.type = Image.Type.Filled;
        }

        public void SetTime(float time)
        {
            _curTime = time;
            _maxTime = time;
            _isStarted = true;
        }
    }
}