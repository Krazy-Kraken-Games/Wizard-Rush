using KrazyKrakenGames.Interactables;
using UnityEngine;

public class TriggerBox : MonoBehaviour
{
    [SerializeField] private BaseInteractableObject interactableObject;

    private void OnTriggerEnter(Collider other)
    {
        var collided = other.gameObject;

        if(collided.tag == "Player")
        {
            collided.GetComponent<PlayerInputHandling>().
                SetInteractionObject(interactableObject);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        var collided = other.gameObject;

        if (collided.tag == "Player")
        {
            collided.GetComponent<PlayerInputHandling>().
                SetInteractionObject(null);
        }
    }
}
