using KrazyKrakenGames.Interactables;
using UnityEngine;

public class TriggerBox : MonoBehaviour
{
    [SerializeField] private BaseInteractableObject interactableObject;

    [SerializeField] private ITriggerable triggerable;

    public BaseInteractableObject InteractableObject 
        => triggerable.GameObject.GetComponent<BaseInteractableObject>();

    public GameObject Triggerable => triggerable.GameObject;

    private void Start()
    {
        var parent = transform.parent.gameObject;

        //if(parent.TryGetComponent<BaseInteractableObject>(out interactableObject))
        //{
        //    Debug.Log("Interactable object found");
        //}
        //else
        //{
        //    Debug.LogWarning($"{gameObject.name} doesnt have an interactable object"); ;
        //}

        if(parent.TryGetComponent<ITriggerable>(out triggerable))
        {
            Debug.Log("Triggerable interface found", parent);
        }
        else
        {
            Debug.LogWarning("No triggerable interface found too");
        }
    }

}
