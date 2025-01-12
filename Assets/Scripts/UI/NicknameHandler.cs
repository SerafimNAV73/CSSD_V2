using UnityEngine;
using UnityEngine.UI;

namespace CSSD
{
    public class NicknameHandler : MonoBehaviour
    {
        [SerializeField] private Transform _nicknameAnchorPoint;
        [SerializeField] private Text _nicknameText;

        public Text Nickname => _nicknameText;

        private void Start()
        {
            if (_nicknameText == null) _nicknameText = GetComponentInChildren<Text>();
        }

        private void LateUpdate()
        {
            if (_nicknameAnchorPoint == null) return;

            transform.position = _nicknameAnchorPoint.position;
        }
    }
}