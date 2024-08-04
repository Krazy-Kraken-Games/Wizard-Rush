
using Unity.Collections;
using Unity.Netcode;

namespace KrazyKrakenGames.Multiplayer.Data
{
    [System.Serializable]   
    public struct PlayerData : INetworkSerializable
    {
        public float moveSpeed;
        public FixedString128Bytes playerName;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref moveSpeed);
            serializer.SerializeValue(ref playerName);
        }
    }
}
