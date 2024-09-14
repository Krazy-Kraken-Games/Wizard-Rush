using KrazyKrakenGames.Multiplayer.Data;
using KrazyKrakenGames.UI;
using Unity.Netcode;
using UnityEngine;

public class NetworkPlayerMonkey : NetworkBehaviour
{
    [SerializeField]
    private NetworkVariable<PlayerData> currentPlayerData
        = new NetworkVariable<PlayerData>(new PlayerData
            {
                playerName = "abc",
                moveSpeed = 5f
            },
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);

    [SerializeField] private PlayerCanvas displayPlayerCanvas;

    [SerializeField] private CharacterController controller;


    [SerializeField] private float rotationThreshold;

    [SerializeField] private GameObject model;



    private void Update()
    {
        if (!IsOwner) return;

        Move();
    }

    private void Locomotion()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 moveVector = new Vector3(horizontal, 0f, vertical);

        transform.position += moveVector * 3f * Time.deltaTime;
    }

    private void Move()
    {
        // set target speed based on move speed, sprint speed and if sprint is pressed
        float targetSpeed = currentPlayerData.Value.moveSpeed;


        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector3 moveDirection = new Vector3(moveHorizontal, 0.0f, moveVertical);
        moveDirection = transform.TransformDirection(moveDirection);

        if (moveDirection == Vector3.zero) targetSpeed = 0.0f;

        // move the player
        controller.Move(moveDirection.normalized * (targetSpeed * Time.deltaTime));

        NewModelRotation(moveHorizontal, moveVertical);
    }

    private void RotateModel(float moveHorizontal, float moveVertical)
    {
        float targetAngle = 0;

        if (Mathf.Abs(moveHorizontal) > rotationThreshold && Mathf.Abs(moveVertical) < rotationThreshold)
        {
            // Rotate 90 degrees when only horizontal movement
            targetAngle = moveHorizontal > 0 ? 90 : -90;
        }
        else if (Mathf.Abs(moveVertical) > rotationThreshold && Mathf.Abs(moveHorizontal) < rotationThreshold)
        {
            // Rotate 90 degrees when only vertical movement
            targetAngle = moveVertical > 0 ? 0 : 180;
        }
        else if (Mathf.Abs(moveHorizontal) > rotationThreshold && Mathf.Abs(moveVertical) > rotationThreshold)
        {
            // Rotate 45 degrees when both axes are pressed
            targetAngle = Mathf.Atan2(moveHorizontal, moveVertical) * Mathf.Rad2Deg;
        }

        Quaternion targetRotation = Quaternion.Euler(0, targetAngle, 0);
        model.transform.rotation = targetRotation;
    }

    private void NewModelRotation(float moveHorizontal, float moveVertical)
    {
        Vector3 moveVector3 = new Vector3(moveHorizontal,0, moveVertical);
        if (moveVector3 != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveVector3);
            model.transform.rotation = targetRotation;
        }
    }


public override void OnNetworkSpawn()
    {
        currentPlayerData.OnValueChanged += OnPlayerDataChanged;

        if (IsOwner)
        {
            currentPlayerData.Value = new PlayerData
            {
                playerName = $"Test{OwnerClientId}",
                moveSpeed = 7f
            };

            controller = GetComponent<CharacterController>();
        }

        PopulatePlayerUI();
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        currentPlayerData.OnValueChanged -= OnPlayerDataChanged;
    }

    private void OnPlayerDataChanged(PlayerData _previousData, PlayerData _currentValue)
    {
        Debug.Log($"Changed detected for: {currentPlayerData.Value.playerName}");
        PopulatePlayerUI();
    }

    private void PopulatePlayerUI()
    {
        displayPlayerCanvas.PopulateDisplay(currentPlayerData.Value);
    }
}
