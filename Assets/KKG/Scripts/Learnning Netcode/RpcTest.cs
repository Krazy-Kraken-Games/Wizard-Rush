using Unity.Netcode;
using UnityEngine;


namespace KrazyKrakenGames.LearningNetcode
{
    public class RpcTest : NetworkBehaviour
    {
        public override void OnNetworkSpawn()
        {
            if(!IsServer && IsOwner)
            {
                //Basically is a client, then sending an rpc message to server
                ServerOnlyRpc(0, NetworkObjectId);
            }
        }

        [Rpc(SendTo.ClientsAndHost)]
        void ClientAndHostRpc(int value, ulong sourceNetworkObjectId)
        {
            Debug.Log($"Client Received the RPC #{value} on NetworkObject #{sourceNetworkObjectId}");
            if (IsOwner) //Only send an RPC to the owner of the NetworkObject
            {
                ServerOnlyRpc(value + 1, sourceNetworkObjectId);
            }
        }

        [Rpc(SendTo.Server)]
        void ServerOnlyRpc(int value, ulong sourceNetworkObjectId)
        {
            Debug.Log($"Server Received the RPC #{value} on NetworkObject #{sourceNetworkObjectId}");
            ClientAndHostRpc(value, sourceNetworkObjectId);
        }
    }
}
