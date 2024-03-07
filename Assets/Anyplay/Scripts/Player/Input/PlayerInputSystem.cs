using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputSystem : MonoBehaviour
{
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void InputPlayer(InputAction.CallbackContext _context)
    {
        Joystick = _context.ReadValue<Vector2>();
    }
    
    public Vector2 Joystick { get; private set; }
}
