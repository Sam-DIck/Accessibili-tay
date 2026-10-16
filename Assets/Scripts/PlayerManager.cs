using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    PlayerInputManager playerInputManager;
    Navigator navigator;
    Driver driver;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        
        playerInputManager ??= GetComponent<PlayerInputManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPlayerJoined(PlayerInput player)
    {
        var d = player.GetComponent<Driver>();
        var n = player.GetComponent<Navigator>();
        if (driver == null)
        {
            driver = d;
            n.enabled = false;
        }
        else if (navigator == null)
        {
            navigator = n;
            d.enabled = false;
        }
    }
}
