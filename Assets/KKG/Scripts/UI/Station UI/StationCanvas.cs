using TMPro;
using UnityEngine;

public class StationCanvas : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI ingredientText;

    [SerializeField] private GameObject statusObject;
    private RadialFiller statusFiller;

    private void Start()
    {
        statusObject.SetActive(false);
        statusFiller = statusObject.GetComponent<RadialFiller>();
    }

    public void PopulateIngredient(string ingredientName)
    {
        ingredientText.text = ingredientName;
    }

    public void StationCooking(float timer)
    {
        statusObject.SetActive(true);


        statusFiller.SetMaxTime(timer);
        statusFiller.StartFill();

        statusFiller.OnFillComplete += OnStatusFillCompleted;
    }

    private void OnStatusFillCompleted()
    {
        statusFiller.OnFillComplete -= OnStatusFillCompleted;
    }

    
}
