using Fusion;
using UnityEngine;

namespace Darwin
{
    [RequireComponent(typeof(NetworkCharacterController))]
    public class Player : NetworkBehaviour
    {
        private const float MOVE_SPEED = 5.0f;

        private NetworkCharacterController _cc;

        private void Awake()
        {
            _cc = GetComponent<NetworkCharacterController>();
        }

        public override void FixedUpdateNetwork()
        {
            if (GetInput(out NetworkInputData data))
            {
                data.direction.Normalize();
                _cc.Move(MOVE_SPEED * Runner.DeltaTime * data.direction);
            }
        }
    }
}