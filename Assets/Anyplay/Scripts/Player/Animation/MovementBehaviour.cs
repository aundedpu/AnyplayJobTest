using UnityEngine;

namespace Anyplay.Scripts.Player.Animation
{
    public interface IMovementBehaviour
    {
        void ApplyMovement();
        void ApplyProcess();
        void ApplyGravity();
        void ApplyRotation();
    }
}
