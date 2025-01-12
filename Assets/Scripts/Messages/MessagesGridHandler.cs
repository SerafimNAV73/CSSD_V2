using System.Collections.Generic;
using UnityEngine;

namespace CSSD
{
    public class MessagesGridHandler : MonoBehaviour
    {
        [SerializeField] private Transform _messangerGrid;
        [SerializeField] private MessageHandler _messagePrefab;

        [SerializeField] private int _maxMessagesCount = 4;

        private List<MessageHandler> _currentMessages = new List<MessageHandler>();

        private void OnEnable()
        {
            MessageHandler.destroyDel += RemoveMessage;
            NetworkPlayerMessangerHandler.createMessageDell += CreateMessage;
        }

        private void OnDisable()
        {
            MessageHandler.destroyDel -= RemoveMessage;
            NetworkPlayerMessangerHandler.createMessageDell -= CreateMessage;
        }

        private void CreateMessage(string message)
        {
            if (_messagePrefab == null) return;

            if (_currentMessages.Count == _maxMessagesCount)
            {
                MessageHandler messageForDelete = _currentMessages[0];
                _currentMessages.Remove(messageForDelete);
                Destroy(messageForDelete.gameObject);
            }

            MessageHandler newMessageHandler = Instantiate(_messagePrefab, _messangerGrid);
            newMessageHandler.SetString(message);
            _currentMessages.Add(newMessageHandler);
        }

        private void RemoveMessage(MessageHandler message)
        {
            if (_currentMessages.Contains(message))
                _currentMessages.Remove(message);
        }
    }
}