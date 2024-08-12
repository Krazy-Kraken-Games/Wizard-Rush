
using UnityEngine;

namespace KrazyKrakenGames.Interfaces.Objects
{
    /// <summary>
    /// An interface to determine if an object can be interacted with
    /// </summary>
    public interface IInteractable
    {
        // Method to be used to interact
        void Interact(ulong initiatorID);
    }
}
