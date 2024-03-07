using System;
using System.Collections;
using Anyplay.Scripts.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Anyplay.Scripts.Player.Animation
{
    public class EasingPlayerMove : MonoBehaviour
    {

        public IEnumerator EasingCameraMovement(float duration, float target, PlayerCamera playerCamera,
            float blockButtonDuration, Easing.Ease easing)
        {
            JoystickUIEvent.instance.ButtonProne.interactable = false;
            JoystickUIEvent.instance.ButtonCrouch.interactable = false;
            float time = 0f;
            var current_playerCamera = playerCamera.offset;
            var target_playerCamera = new Vector3(0, target, 0);

            while (time <= duration)
            {
                time += Time.deltaTime;
                float timeNormalized = EasingFormula.EasingFloat(easing, 0, 1, time / duration);

                playerCamera.offset =
                    current_playerCamera = Vector3.Lerp(current_playerCamera, target_playerCamera, timeNormalized);

                yield return null;
            }

            yield return new WaitForSeconds(blockButtonDuration);
            JoystickUIEvent.instance.ButtonProne.interactable = true;
            JoystickUIEvent.instance.ButtonCrouch.interactable = true;
        }

        public IEnumerator EasingDistanceMovement(float duration, float target, PlayerCamera playerCamera,
            float blockButtonDuration ,Easing.Ease easing)
        {
            JoystickUIEvent.instance.ButtonProne.interactable = false;
            JoystickUIEvent.instance.ButtonCrouch.interactable = false;
            float time = 0f;
            var current_playerCamera = playerCamera.distance
                ;
            var target_playerCamera = target;

            while (time <= duration)
            {
                time += Time.deltaTime;
                float timeNormalized = EasingFormula.EasingFloat(easing, 0, 1, time / duration);

                playerCamera.distance =
                    current_playerCamera = Mathf.Lerp(current_playerCamera, target_playerCamera, timeNormalized);

                yield return null;
            }

            yield return new WaitForSeconds(blockButtonDuration);
            JoystickUIEvent.instance.ButtonProne.interactable = true;
            JoystickUIEvent.instance.ButtonCrouch.interactable = true;
        }
    }
}