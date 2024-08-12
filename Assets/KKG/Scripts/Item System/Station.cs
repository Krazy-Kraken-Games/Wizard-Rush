using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class Station : MonoBehaviour, ITriggerable, IStoring
{
    public GameObject GameObject => gameObject;

    [SerializeField]
    private NetworkVariable<FixedString128Bytes> addedIngredient
      = new NetworkVariable<FixedString128Bytes>(string.Empty,
          NetworkVariableReadPermission.Everyone,
          NetworkVariableWritePermission.Server);

    //Dummy will be removed when actual ingredient data structure is created
    [SerializeField] private string IngredientNameTest;

    //Move start and destroy content to On Network Spawns when actually spawning occurs on network
    private void Start()
    {
        addedIngredient.OnValueChanged += OnAddedIngredientChange;
    }

    private void OnDestroy()
    {
        addedIngredient.OnValueChanged -= OnAddedIngredientChange;
    }

    public void AddItem(FixedString128Bytes item)
    {
        //Lets add the items without any checks
        AddItemServerRpc(item);
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddItemServerRpc(FixedString128Bytes item)
    {
        addedIngredient.Value = item;
    }

    private void OnAddedIngredientChange(FixedString128Bytes old, FixedString128Bytes curr)
    {
        IngredientNameTest = curr.Value.ToString();
    }
}
