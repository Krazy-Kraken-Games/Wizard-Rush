using Unity.Netcode;
using UnityEngine;

namespace KrazyKrakenGames.LearningNetcode
{
    public class NetGameManager : MonoBehaviour
    {
        public static NetGameManager instance = null;

        [SerializeField] private NetworkManager networkManager;


        private void Awake()
        {
            networkManager = GetComponent<NetworkManager>();

            if(instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            networkManager.OnClientConnectedCallback += OnClientConnectedCallback;
        }

        private void OnDestroy()
        {
            networkManager.OnClientConnectedCallback -= OnClientConnectedCallback;
        }

        private void OnClientConnectedCallback(ulong obj)
        {
            Debug.Log($"Connected client {obj}");
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 300));

            if (!networkManager.IsClient && !networkManager.IsServer)
            {
                //They are not connected to the network, either as client or server
                StartButtons();
            }
            else
            {
                //They are connected to the network in some form
                StatusLabels();

                DisconnectButtons();
            }

            GUILayout.EndArea();
        }

        /// <summary>
        /// Helper function to populate Buttons on GUI
        /// </summary>
        private void StartButtons()
        {
            if (GUILayout.Button("Host"))
                networkManager.StartHost();

            if (GUILayout.Button("Client"))
                networkManager.StartClient();

            if(GUILayout.Button("Server"))
                networkManager.StartServer();
        }

        private void DisconnectButtons()
        {
            if (GUILayout.Button("Disconnect"))
            {
                //Only server can disconnect clients, so we use shutdown
                networkManager.Shutdown();
            }
        }

        /// <summary>
        /// Print out status labels based on type of network manager
        /// </summary>
        private void StatusLabels()
        {
            var mode = networkManager.IsHost ? "Host" : 
                networkManager.IsServer ? "Server" : "Client";

            GUILayout.Label($"Mode: {mode}");
        }


        private void SubmitNewPosition()
        {
            if(GUILayout.Button(networkManager.IsServer ? "Move" : "Request Position Change"))
            {
                var playerObject = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
                var player = playerObject.GetComponent<HelloWorldPlayer>();
                player.Move();
            }
        }


        //Singleton Methods
        public NetworkObject GetNetworkObjectById(ulong initiatorID)
        {
            NetworkManager.Singleton.
                SpawnManager.SpawnedObjects.
                TryGetValue(initiatorID, out var initiator);

            return initiator;
        }
    }
}
