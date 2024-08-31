using UnityEngine;

public enum IngredientType
{
    DEFAULT = 0,
    INGREDIENT = 1,
    PRODUCT = 2,
    AMMO = 3
}


[CreateAssetMenu(fileName = "Ingredient", menuName = "Data/Create Ingredient")]
public class IngredientDataSO : ScriptableObject
{
    public string ingredientName;

    public IngredientType ingredientType;
}
