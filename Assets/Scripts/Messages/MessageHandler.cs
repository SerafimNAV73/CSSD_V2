using System;
using UnityEngine;
using UnityEngine.UI;
using Fusion;

namespace CSSD
{
    public class MessageHandler : MonoBehaviour
    {
        [SerializeField] private Text _messageText;
        [SerializeField] private float _lifeTime = 3;

        private float _lifeStartTime;

        public static Action<MessageHandler> destroyDel;

        private void Start()
        {
            _lifeStartTime = Time.time;
        }

        private void FixedUpdate()
        {
            if (Time.time - _lifeStartTime > _lifeTime)
            {
                destroyDel?.Invoke(this);
                Destroy(gameObject);
            }
        }

        public void SetString(string message)
        {
            _messageText.text = message;
        }
    }
}