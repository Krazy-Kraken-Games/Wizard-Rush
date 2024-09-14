using KrazyKrakenGames.Interactables;
using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerInputHandling : NetworkBehaviour
{
    [SerializeField] private BaseInteractableObject interactableObject;

    [SerializeField] private GameObject nearByStation;

    public Action<BaseInteractableObject> OnInteractButtonFired;
    public Action<GameObject> OnActionEventHandler;

    [SerializeField] private PickUpHolder pickUpHolder;
    private void Update()
    {
        if (!IsOwner) return;

        Interaction();
    }

    private void Interaction()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            //Pick up and Drop
            pickUpHolder.OnPickUpInitiated(interactableObject);
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            //Action Key?
            OnActionEventHandler?.Invoke(nearByStation);
        }
    }

    public void SetInteractionObject(BaseInteractableObject _object)
    {
        interactableObject = _object;
    }

    private void OnTriggerEnter(Collider other)
    {
        var collided = other.gameObject;

        if(collided.tag == "TriggerBox")
        {
            if(collided.TryGetComponent<TriggerBox>(out var triggerBox))
            {
                if (triggerBox != null)
                {
                    if (triggerBox.InteractableObject != null)
                    {
                        interactableObject = triggerBox.InteractableObject;
                    }
                    else if (triggerBox.Triggerable != null)
                    {
                        nearByStation = triggerBox.Triggerable;
                    }
                    else
                    {
                        Debug.Log("Both are empty, must be a spawner?");
                    }
                }
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        var collided = other.gameObject;

        if(other == null)
        {
            Debug.Log($"{gameObject.name} is not touching any colliders");
            interactableObject = null;
        }

        if(collided.tag == "TriggerBox")
        {
            if (collided.TryGetComponent<TriggerBox>(out var triggerBox))
            {
                if (triggerBox != null && triggerBox.InteractableObject != null)
                {
                    interactableObject = triggerBox.InteractableObject;
                }
                else if(triggerBox != null && triggerBox.Triggerable != null)
                {
                    nearByStation = triggerBox.Triggerable;
                }
            }
        }
        else
        {
            interactableObject = null;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var collided = other.gameObject;

        if (collided.tag == "TriggerBox")
        {
            interactableObject = null;
            nearByStation = null;
        }
    }
}
