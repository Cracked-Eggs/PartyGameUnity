using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLook : MonoBehaviour
{
    [Header("Settings")] 
    public Vector2 clampInDegrees = new Vector2(360, 180);
    public bool lockCursor = true;
    public float sensitivity = 2f;  // Changed variable name from mouseSensitivity to a more general one
    private InputAction _lookAction; 
    private InputSystem_Actions inputSystem;

    [Header("Player")] 
    public GameObject character;

    private Vector2 targetDirection;
    private Vector2 targetCharacterDirection;
    private Vector2 lookInput;  // This will store input from the right analog stick
    private Vector2 mouseAbsolute;
    private float lastSentRotation;

    void Awake()
    {
        inputSystem = new InputSystem_Actions();
    }

    void OnEnable()
    {
        _lookAction = inputSystem.Player.Look;  // Bind right stick to look action
        _lookAction.Enable();
    }

    void OnDisable()
    {
        _lookAction.Disable();
    }

    void Start()
    {
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        targetDirection = transform.localRotation.eulerAngles;
        targetCharacterDirection = character.transform.localRotation.eulerAngles;
    }

    void Update()
    {
        // Read input from the right analog stick (Look action)
        lookInput = _lookAction.ReadValue<Vector2>();

        // Calculate the orientation (as in the original mouse look)
        var targetOrientation = Quaternion.Euler(targetDirection);
        var targetCharacterOrientation = Quaternion.Euler(targetCharacterDirection);

        // Instead of mouseDelta, we use the right analog stick input scaled by sensitivity
        mouseAbsolute += lookInput * sensitivity;

        // Clamp the Y-axis (vertical) rotation to avoid looking too far up or down
        mouseAbsolute.y = Mathf.Clamp(mouseAbsolute.y, -clampInDegrees.y * 0.5f, clampInDegrees.y * 0.5f);

        // Apply rotations based on input from the right analog stick
        transform.localRotation = Quaternion.AngleAxis(-mouseAbsolute.y, targetOrientation * Vector3.right) * targetOrientation;

        // Rotate the player horizontally based on the X-axis (right stick horizontal input)
        var yRotation = Quaternion.AngleAxis(mouseAbsolute.x, Vector3.up);
        character.transform.localRotation = yRotation * targetCharacterOrientation;
    }
}
