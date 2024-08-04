using KrazyKrakenGames.Interactables;
using UnityEngine;

public class PlayerInputHandling : MonoBehaviour
{
    [SerializeField] private BaseInteractableObject interactableObject;

    [SerializeField] private Transform holderTransform;
    private void Update()
    {
        Interaction();
    }

    private void Interaction()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if(interactableObject!= null)
            {
                interactableObject.Interact(holderTransform.gameObject);
            }
        }
    }

    public void SetInteractionObject(BaseInteractableObject _object)
    {
        interactableObject = _object;
    }
}
