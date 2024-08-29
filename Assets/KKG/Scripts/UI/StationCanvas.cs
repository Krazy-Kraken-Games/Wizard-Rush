using TMPro;
using UnityEngine;

public class StationCanvas : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI ingredientText;

    public void PopulateIngredient(string ingredientName)
    {
        ingredientText.text = ingredientName;
    }
}
