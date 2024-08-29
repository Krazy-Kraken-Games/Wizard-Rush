using Unity.Collections;
using Unity.Netcode;

[System.Serializable]
public struct IngredientData : INetworkSerializable
{
    public FixedString128Bytes ingredientName;
    public ulong gameObjectID;  //Network Object ID

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ingredientName);
        serializer.SerializeValue(ref gameObjectID);
    }
}
