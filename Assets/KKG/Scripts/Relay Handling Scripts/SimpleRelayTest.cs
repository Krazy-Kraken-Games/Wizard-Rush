using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;


public class SimpleRelayTest : MonoBehaviour
{
    public string joinCodeValue;
    public TextMeshProUGUI statusText;

    public GameObject preJoinUI;
    private async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += OnAuthenticationSignedIn;

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private void OnAuthenticationSignedIn()
    {
        Debug.Log($"Signed in: {AuthenticationService.Instance.PlayerId}");
    }

    public void CreateRelayTest()
    {
        CreateRelay();
    }

    private async void CreateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            Debug.Log($"Join Code:{joinCode}");
            statusText.text = joinCode;

            //Set Relay Server data

            NetworkManager.Singleton.
                GetComponent<UnityTransport>()
                .SetHostRelayData(
                    allocation.RelayServer.IpV4,
                    (ushort)allocation.RelayServer.Port,
                    allocation.AllocationIdBytes,
                    allocation.Key,
                    allocation.ConnectionData

                );

            bool res = NetworkManager.Singleton.StartHost();

            if (res)
            {
                preJoinUI.SetActive(false);
            }
        }
        catch(RelayServiceException ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    public void JoinRelayTest()
    {
        JoinRelay(joinCodeValue);
    }

    public async void JoinRelay(string _code)
    {
        try
        {
            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(_code);

            //Set Client Relay data as joining via client
            NetworkManager.Singleton.
                GetComponent<UnityTransport>().
                SetClientRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData,
                allocation.HostConnectionData);

            bool res = NetworkManager.Singleton.StartClient();

            if (res)
            {
                statusText.text = "Client connected successfully";
                preJoinUI.SetActive(false);
            }
            else
            {
                statusText.text = "Client connection failed via code";
            }
        }
        catch (RelayServiceException ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    #region UI Handling
    //UI Section

    public void UpdateJoinCodeValue(string _value)
    {
        joinCodeValue = _value.ToUpper();
    }

    #endregion



}
