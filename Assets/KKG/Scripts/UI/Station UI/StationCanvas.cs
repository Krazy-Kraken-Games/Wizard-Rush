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

        if (statusFiller != null)
        {
            statusFiller.SetMaxTime(timer);
            statusFiller.StartFill();

            statusFiller.OnFillComplete += OnStatusFillCompleted;
        }
    }

    public void Reset()
    {
        statusObject.SetActive(false);

        if (statusFiller != null)
        {
            statusFiller.Reset();
        }
    }

    private void OnStatusFillCompleted()
    {
        statusFiller.OnFillComplete -= OnStatusFillCompleted;
    }

    
}
