using KrazyKrakenGames.Interfaces.Objects;
using Unity.Netcode;
using UnityEngine;

namespace KrazyKrakenGames.Interactables
{
    public class Ingredient : BaseInteractableObject, IStoreable
    {
        [SerializeField] private IngredientType type;

        [SerializeField] private NetworkVariable<IngredientType> networkType
            = new NetworkVariable<IngredientType>(
                IngredientType.DEFAULT,
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Server);

       

        public void Store()
        {
            Debug.Log($"The base ingredient {ingredientData} will be stored");

            State.Value = ObjectState.FEED;
        }

        protected override void PickRpcHelper(ulong initiatorID)
        {
            base.PickRpcHelper(initiatorID);

            Debug.Log("Overridden successfully");
        }
    }
}
