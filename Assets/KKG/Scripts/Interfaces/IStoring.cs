using Unity.Collections;

public interface IStoring
{
    //This will be applied on stations to determine storing of items

    /// <summary>
    /// Action function when an item is added
    /// </summary>
    /// <param name="item">String name of item -> To be changed to proper serializable value</param>
    /// <param name="feederID">Network Id of the player object doing the feeding</param>
    /// <param name="itemID">Network ID of the ingredient object being fed</param>
    void AddItem(FixedString128Bytes item,ulong feederID,ulong itemID);
}
