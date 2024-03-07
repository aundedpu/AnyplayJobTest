using Anyplay.Scripts.Player.Animation;
using Anyplay.Scripts.Utilities;
using UnityEngine;

namespace Anyplay.Scripts
{
    [RequireComponent(typeof(EasingPlayerMove))]
    public class AnimationPlayer : MonoBehaviour, IBasePlayerAnimation
    {
        [SerializeField] private Animator animator;
        [SerializeField] private bool crouch ;
        [SerializeField] private bool prone ;
        private PlayerCamera playerCamera;
        [SerializeField] private EasingPlayerMove easingPlayerMove;
        
        private readonly float  duration = 0.75f ;
        private readonly float  proneOffset = .95f ;
        private readonly float  crouchOffset = 1.2f ;
        private readonly float  standIdleOffset = 1.5f ;
        
        private readonly float  standCrouchDistance = 1.4f ;
        private readonly float  proneDistance = 1.3f ;
        
        private readonly float  blockCrouchDuration = .15f ;
        private readonly float  blockProneDuration = 1f ;

        private void Awake()
        {
            playerCamera = FindObjectOfType<PlayerCamera>();
            easingPlayerMove.GetComponent<EasingPlayerMove>();
        }

        public virtual void SetSpeed(float speed)
        {
            animator.SetFloat("speed", speed);
        }

        public virtual void TriggerAnimation(string animationName)
        {
            animator.SetTrigger(animationName);
        }

        public virtual void ResetTriggerAnimation(string animationName)
        {
            animator.ResetTrigger(animationName);
        }

        public Animator Animator
        {
            get => this.animator;
            set => this.animator = value;
        }

        public void Crouch()
        {
            crouch = !crouch;
            if (prone)
            {
                animator?.SetBool("Crouch", crouch);
                prone = false;
                animator?.SetBool("Prone", prone);
                StartCoroutine(easingPlayerMove.EasingCameraMovement(duration, crouchOffset,playerCamera,blockCrouchDuration,
                    Easing.Ease.EaseInQuad));
                StartCoroutine(easingPlayerMove.EasingDistanceMovement(duration, standCrouchDistance,playerCamera,blockCrouchDuration, Easing.Ease.EaseInQuad));

                return;
            }

            animator?.SetBool("Crouch", crouch);
            if (crouch)
                StartCoroutine(easingPlayerMove.EasingCameraMovement(duration, crouchOffset, playerCamera,blockCrouchDuration,Easing.Ease.EaseInQuad));
            else
                StartCoroutine(easingPlayerMove.EasingCameraMovement(duration, standIdleOffset, playerCamera,blockCrouchDuration,Easing.Ease.EaseInQuad));
        }

        public void Prone()
        {
            prone = !prone;
            if (crouch)
            {
                animator?.SetBool("Prone", prone);
                crouch = false;
                animator?.SetBool("Crouch", crouch);
                StartCoroutine(easingPlayerMove.EasingCameraMovement(duration, proneOffset, playerCamera,blockProneDuration,Easing.Ease.EaseInQuad));
                StartCoroutine(easingPlayerMove.EasingDistanceMovement(duration, proneDistance, playerCamera,blockProneDuration,Easing.Ease.EaseInQuad));
                return;
            }
            
            animator?.SetBool("Prone", prone);
            if (prone)
            {
                StartCoroutine(easingPlayerMove.EasingCameraMovement(duration, proneOffset, playerCamera,blockProneDuration,Easing.Ease.EaseInQuad));
                StartCoroutine(easingPlayerMove.EasingDistanceMovement(duration, proneDistance, playerCamera,blockProneDuration,Easing.Ease.EaseInQuad));
            }
            else
            {
                StartCoroutine(easingPlayerMove.EasingCameraMovement(duration, standIdleOffset, playerCamera,blockProneDuration,Easing.Ease.EaseInQuad));
                StartCoroutine(easingPlayerMove.EasingDistanceMovement(duration, standCrouchDistance, playerCamera,blockProneDuration,
                    Easing.Ease.EaseInQuad));
            }
        }
        
        public void Move(float currentHorizontalSpeed , float currentVerticalSpeed)
        {
            animator.SetFloat("hzInput", currentHorizontalSpeed);
            animator.SetFloat("vInput", currentVerticalSpeed);
        }
        
    }
}