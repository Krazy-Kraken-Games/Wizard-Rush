using Unity.Collections;
using Unity.Netcode;

[System.Serializable]
public struct IngredientData : INetworkSerializable, System.IEquatable<IngredientData>
{
    public FixedString128Bytes ingredientName;
    public ulong gameObjectID;  //Network Object ID
    public IngredientType ingredientType;

    public bool Equals(IngredientData other)
    {
       return ingredientName == other.ingredientName 
            && gameObjectID == other.gameObjectID
            && ingredientType == other.ingredientType;  
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        if (serializer.IsReader)
        {
            var reader = serializer.GetFastBufferReader();
            reader.ReadValueSafe(out ingredientName);
            reader.ReadValueSafe(out gameObjectID);
            reader.ReadValueSafe(out ingredientType);
        }
        else
        {
            var writer = serializer.GetFastBufferWriter();
            writer.WriteValueSafe(ingredientName);
            writer.WriteValueSafe(gameObjectID);
            writer.WriteValueSafe(ingredientType);
        }
    }
}
