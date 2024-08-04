using UnityEngine;

namespace KrazyKrakenGames.Interfaces.Objects
{
    /// <summary>
    /// An interface to determine an object 
    /// can be picked up by the player
    /// and dropped
    /// </summary>
    public interface IPickable
    {
        void Pick(Transform parent);

        void Drop();
    }
}
