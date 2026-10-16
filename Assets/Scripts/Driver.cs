using UnityEngine;
using UnityEngine.InputSystem;

public class Driver : MonoBehaviour
{
    [SerializeField] PlayerInput playerInput;
    [SerializeField] InputActionReference periscopeAction;
    [SerializeField] InputActionReference turnAction;
    [SerializeField] InputActionReference throttleAction;

    InputAction periscopeInputAction;
    InputAction turnInputAction;
    InputAction throttleInputAction;
    SubController subController;


    private void Awake()
    {
        if (playerInput == null)
        {
            playerInput = new PlayerInput();
        }
        periscopeInputAction = playerInput.actions[periscopeAction.action.name]; // I hate that I have to do this, but there's not a way to get the action without magic strings.
        turnInputAction = playerInput.actions[turnAction.action.name];           // same here
        throttleInputAction = playerInput.actions[throttleAction.action.name];   // same here
        subController ??= FindAnyObjectByType<SubController>();
    }

    private void Update()
    {
        float turnInput = turnInputAction.ReadValue<float>();
        float periscopeInput = periscopeInputAction.ReadValue<float>();
        float throttleInput = throttleInputAction.ReadValue<float>();

        subController.Move(throttleInput, turnInput);
        subController.RotatePeriscope(periscopeInput);
    }
}
