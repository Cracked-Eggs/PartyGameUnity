using UnityEngine;
using UnityEngine.InputSystem;

public class UIController : MonoBehaviour
{
    public InputActionAsset inputActions;

    void OnEnable()
    {
        // Enable the UI action map
        var uiActionMap = inputActions.FindActionMap("UI");
        uiActionMap.Enable();
    }
}