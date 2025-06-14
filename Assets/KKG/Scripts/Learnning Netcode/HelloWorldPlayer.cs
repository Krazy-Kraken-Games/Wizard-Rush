using Unity.Netcode;
using UnityEngine;

namespace KrazyKrakenGames.LearningNetcode
{
    public class HelloWorldPlayer : NetworkBehaviour
    {
        public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();
        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                Move();
            }
        }

        public void Move()
        {
            SubmitMoveRequestToRpc();
        }

        [Rpc(SendTo.Server)]
        private void SubmitMoveRequestToRpc()
        {
            var randomPosition = GetRandomPosition();
            transform.position = randomPosition;
            Position.Value = randomPosition;
        }

        Vector3 GetRandomPosition()
        {
            return new Vector3(Random.Range(-3f, 3f), 1f, Random.Range(-3f, 3f));
        }

        private void Update()
        {
            //transform.position = Position.Value;

            //float horizontal = Input.GetAxis("Horizontal");

            //Vector3 nextPostion = new Vector3(transform.position.x, transform.position.y, transform.position.z + horizontal);

            //transform.position = Vector3.MoveTowards(transform.position, nextPostion,1f);
        }
    }
}
