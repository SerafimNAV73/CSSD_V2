using UnityEngine;
using Fusion;
using FMODUnity;
using System;

namespace CSSD
{
    public class NetworkPlayerMessangerHandler : NetworkBehaviour
    {
        [SerializeField] private string _followCall = "Follow me!";
        [SerializeField] private string _helpCall = "Need help!";
        [SerializeField] private string _agreeCall = "Yes!";
        [SerializeField] private string _disagreeCall = "No!";

        [SerializeField] private EventReference _callEvent;

        [SerializeField] private float _callsCooldown = 1;

        private float _lastCallTime = 0;
        private string _playerNickname = "";

        public static Action<string> createMessageDell;

        private void Start()
        {
            _lastCallTime -= _callsCooldown;
        }

        public override void FixedUpdateNetwork()
        {
            if (!Object.HasInputAuthority) return;

            bool isFollowCallButtonPressed = false;
            bool isHelpCallButtonPressed = false;
            bool isAgreeCallButtonPressed = false;
            bool isDisagreeCallButtonPressed = false;

            //Get the input from the network
            if (GetInput(out NetworkInputData networkInputData))
            {
                isFollowCallButtonPressed = networkInputData.isFollowCallButtonPressed;
                isHelpCallButtonPressed = networkInputData.isHelpCallButtonPressed;
                isAgreeCallButtonPressed = networkInputData.isAgreeCallButtonPressed;
                isDisagreeCallButtonPressed = networkInputData.isDisagreeCallButtonPressed;
            }

            if (isFollowCallButtonPressed) 
            {
                Debug.Log("Follow Call");
                SendCall(_playerNickname, _followCall); 
            }
            if (isHelpCallButtonPressed)
            {
                Debug.Log("Help Call"); 
                SendCall(_playerNickname, _helpCall); 
            }
            if (isAgreeCallButtonPressed)
            {
                Debug.Log("Agree Call"); 
                SendCall(_playerNickname, _agreeCall);
            }
            if (isDisagreeCallButtonPressed)
            {
                Debug.Log("Disagree Call"); 
                SendCall(_playerNickname, _disagreeCall);
            }
        }

        private void SendCall(string nickname, string callText)
        {
            if (Time.time - _lastCallTime < _callsCooldown) return;
            _lastCallTime = Time.time;
            if (!_callEvent.IsNull)
                RuntimeManager.PlayOneShot(_callEvent);

            RPC_SendCall($"{nickname}: {callText}");
        }

        private void CreateMessage(string message)
        {
            createMessageDell?.Invoke(message);
        }

        public void SetNickname(string nickname)
        {
            _playerNickname = nickname;
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_SendCall(string message)
        {
            RPC_CreateMessage(message);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_CreateMessage(string message)
        {
            CreateMessage(message);
        }
    }
}