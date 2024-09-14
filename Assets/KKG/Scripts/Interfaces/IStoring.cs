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
    void AddItem(IngredientData data,ulong feederID,ulong itemID);


    /// <summary>
    /// Validation check needs to happen prior to adding/feeding/storing the item
    /// </summary>
    /// <param name="_type"></param>
    /// <returns></returns>
    bool CheckAllowed(IngredientType _type);
}
