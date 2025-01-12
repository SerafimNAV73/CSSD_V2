using Fusion;
using UnityEngine;

namespace CSSD
{
    public struct NetworkInputData : INetworkInput
    {
        public Vector2 movementInput;
        public Vector2 aimForwardVector;
        public NetworkBool isFirePressed;
        public NetworkBool isReloadButtonPressed;
        public NetworkBool isMenuButtonPressed;
        public NetworkBool isFollowCallButtonPressed;
        public NetworkBool isHelpCallButtonPressed;
        public NetworkBool isAgreeCallButtonPressed;
        public NetworkBool isDisagreeCallButtonPressed;
    }
}