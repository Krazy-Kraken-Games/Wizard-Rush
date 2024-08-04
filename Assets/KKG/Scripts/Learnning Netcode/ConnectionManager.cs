using System;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;


namespace KrazyKrakenGames.LearningNetcode
{
    public class ConnectionManager : MonoBehaviour
    {
        private enum ConnectionState
        {
            DISCONNECTED,
            CONNECTING,
            CONNECTED
        }


        private string profileName;
        private string sessionName;
        private int maxPlayers = 4;


        private ConnectionState state = ConnectionState.DISCONNECTED;
        private ISession session;

        private NetworkManager networkManager;

        private async void Awake()
        {
            networkManager = GetComponent<NetworkManager>();

            //Listeners for when new clients are connected/disconnected
            networkManager.OnClientConnectedCallback += OnClientConnectedListener;
            
        }

        private void OnDestroy()
        {
            networkManager.OnClientConnectedCallback -= OnClientConnectedListener;
        }

        private void OnClientConnectedListener(ulong clientId)
        {

        }
    }
}
