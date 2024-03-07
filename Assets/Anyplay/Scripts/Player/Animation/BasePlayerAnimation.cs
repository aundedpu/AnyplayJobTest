using UnityEngine;

namespace Anyplay.Scripts.Player.Animation
{
    public interface IBasePlayerAnimation
    {
        void Crouch();
        void Prone();

        void Move(float currentHorizontalSpeed , float currentVerticalSpeed);

    }
    
    
}
