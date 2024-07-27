using Unity.Netcode;
using UnityEngine;
using System;


namespace KrazyKrakenGames.LearningNetcode
{
    public class NetworkTransformTest : NetworkBehaviour
    {
        private void Update()
        {
            //Only happen on server
            if (IsServer)
            {
                float theta = Time.frameCount / 10.0f;
                transform.position = new Vector3((float)Math.Cos(theta), 0.0f, (float)Math.Sin(theta));
            }
        }
    }
}
