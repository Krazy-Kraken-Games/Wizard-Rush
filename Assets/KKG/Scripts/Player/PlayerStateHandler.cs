using System;
using Unity.Netcode;
using UnityEngine;

[Serializable]
public enum PlayerState
{
    DEFAULT = 0,    //Normal player movement
    CANNON = 1      //WHen player is controlling the cannon
}
public class PlayerStateHandler : NetworkBehaviour
{
    public NetworkVariable<PlayerState> State
        = new NetworkVariable<PlayerState>(PlayerState.DEFAULT,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);


    
    public void SetState(PlayerState _newState)
    {
        if (!IsOwner)
        {
            return;
        }

        State.Value = _newState;
    }
}
