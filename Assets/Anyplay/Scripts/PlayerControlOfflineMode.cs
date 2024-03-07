using Anyplay.Scripts.Player.Animation;
using UnityEngine;
using UnityEngine.Serialization;

namespace Anyplay.Scripts
{
    [RequireComponent(typeof(PlayerInputSystem))]
    [RequireComponent(typeof(CharacterController))]
    public class PlayerControlOfflineMode : MonoBehaviour, IMovementBehaviour
    {
        #region Variables: Moving

        [Header("Move Setting")] [SerializeField]
        private CharacterController characterController;

        [SerializeField] private float speed;
        private Vector3 direction;

        #endregion

        #region Variables: Rotation

        [Header("Rotation Setting")] [SerializeField]
        private float smoothRotation = 2;

        private float currentVelocity;

        #endregion


        [SerializeField] private float mass;
        private float gravity = -9.81f;
        [SerializeField] private float velocity;


        private Camera relativeCamera;
        private PlayerInputSystem playerInput;
        [SerializeField] private AnimationPlayer animationPlayer;

        private void Awake()
        {
            playerInput = GetComponent<PlayerInputSystem>();
        }

        void Start()
        {
            ApplyProcess();
        }

        // Update is called once per frame
        void Update()
        {
            ApplyMovement();
            ApplyGravity();
            ApplyRotation();
        }

        public void ApplyProcess()
        {
            if (relativeCamera == null)
                relativeCamera = Camera.main;

            relativeCamera.GetComponent<PlayerCamera>().Target = this.transform;
            AnimationPlayer = animationPlayer;
            EnableJoystickControl = true;
            if (JoystickUIEvent.instance)
            {
                JoystickUIEvent.instance.ButtonCrouch.onClick.AddListener(() => animationPlayer.Crouch());
                JoystickUIEvent.instance.ButtonProne.onClick.AddListener(() => animationPlayer.Prone());
            }
        }

        public void ApplyGravity()
        {
            if (velocity < 0.0f)
            {
                velocity = -1.0f;
            }
            else
            {
                velocity += ((2 * mass * gravity) * Time.deltaTime);
            }

            direction.y = velocity;
        }

        public void ApplyRotation()
        {
            var rotation = Quaternion.LookRotation(relativeCamera.transform.forward, Vector3.up);
            characterController.transform.rotation = Quaternion.Lerp(characterController.transform.rotation, rotation,
                Time.deltaTime * smoothRotation);
        }

        public void ApplyMovement()
        {
            var horizontal = EnableJoystickControl ? playerInput.Joystick.x : 0;
            var vertical = EnableJoystickControl ? playerInput.Joystick.y : 0;

            var move = new Vector3(horizontal, 0, vertical);
            var cameraDir = relativeCamera.transform.TransformDirection(move);
            var planeVector = Vector3.ProjectOnPlane(cameraDir, transform.up).normalized;
            var inputMagnitude = move.magnitude;
            move = planeVector * inputMagnitude;
            move.y *= 0;


            direction = new Vector3(move.x * (speed), velocity,
                move.z * (speed));
            characterController.Move(direction * Time.deltaTime);

            var horizontalVelocity = new Vector3(horizontal, 0, vertical);
            horizontalVelocity *= speed;

            var currentHorizontalSpeed = horizontalVelocity.magnitude;

            AnimationPlayer?.Move(horizontal, vertical);
        }

        public AnimationPlayer AnimationPlayer { get; set; }
        public bool EnableJoystickControl { get; set; } = false;
    }
}