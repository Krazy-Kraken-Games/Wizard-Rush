using Unity.Collections;
using Unity.Netcode;

[System.Serializable]
public struct IngredientData : INetworkSerializable, System.IEquatable<IngredientData>
{
    public FixedString128Bytes ingredientName;
    public ulong gameObjectID;  //Network Object ID

    public bool Equals(IngredientData other)
    {
       return ingredientName == other.ingredientName && gameObjectID == other.gameObjectID;  
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        if (serializer.IsReader)
        {
            var reader = serializer.GetFastBufferReader();
            reader.ReadValueSafe(out ingredientName);
            //serializer.SerializeValue(ref gameObjectID);
            reader.ReadValueSafe(out gameObjectID);
        }
        else
        {
            var writer = serializer.GetFastBufferWriter();
            writer.WriteValueSafe(ingredientName);
            writer.WriteValueSafe(gameObjectID);
        }
    }
}
