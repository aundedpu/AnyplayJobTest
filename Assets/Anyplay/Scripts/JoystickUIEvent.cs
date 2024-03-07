using UnityEngine;
using UnityEngine.UI;

public class JoystickUIEvent : MonoBehaviour
{
    public static JoystickUIEvent instance;
    [SerializeField] private Button buttonCrouch;
    [SerializeField] private Button buttonProne;

    private void Awake()
    {
        instance = this;
        ButtonCrouch = buttonCrouch;
        ButtonProne = buttonProne;
    }
    public Button ButtonCrouch { get; set; }
    public Button ButtonProne { get; set; }
}
