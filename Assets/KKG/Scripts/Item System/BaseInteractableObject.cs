using KrazyKrakenGames.Interfaces.Objects;
using TMPro;
using UnityEngine;

namespace KrazyKrakenGames.Interactables
{
    /// <summary>
    /// A base interactable object class to hold interact with
    /// </summary>
    public class BaseInteractableObject : MonoBehaviour, IInteractable,IPickable
    {
        public string objectName;
        public bool isPicked = false;

        private Rigidbody rb;

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
        }

        public virtual void Interact(GameObject initiator)
        {
            Debug.Log($"{objectName} is interacted with");

            if (isPicked)
                Drop();
            else
                Pick(initiator.transform);
        }

        public void Pick(Transform parent)
        {
            isPicked = true;
            transform.position = parent.position;
            transform.SetParent(parent);

            rb.useGravity = false;
            rb.isKinematic = true;
        }

        public void Drop()
        {
            isPicked = false;
            transform.SetParent(null);

            rb.useGravity = true;
            rb.isKinematic = false;
        }
    }
}
