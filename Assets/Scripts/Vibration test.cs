using UnityEngine;
using UnityEngine.InputSystem;

public class Vibrationtest : MonoBehaviour
{

    [SerializeField, Range(0f,1f)] float LFEMaxIntensity;
    [SerializeField, Range(0f, 1f)] float HFEMaxIntensity;

    [SerializeField, Range(-1f, 1f)] float bal;

    PlayerInput playerInput;


    private void Awake()
    {
        playerInput ??= GetComponent<PlayerInput>();
    }

    private void Update()
    {
        if (playerInput.devices.Count > 0)
        {
            if (playerInput.devices[0] is Gamepad gamepad)
            {
                float L = Mathf.Clamp01(-bal) * LFEMaxIntensity;
                float R = Mathf.Clamp01(bal) * HFEMaxIntensity;
                Debug.Log($"L: {L}, R: {R}");
                gamepad.SetMotorSpeeds(L, R);
            }
        }
    }

    private void OnApplicationQuit()
    {
        OnDisable();
    }
    private void OnDisable()
    {
        if (playerInput.devices.Count > 0)
        {
            if (playerInput.devices[0] is Gamepad gamepad)
            {
                gamepad.SetMotorSpeeds(0f, 0f);
            }
        }
    }
}
