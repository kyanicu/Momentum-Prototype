using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    private PlayerMovement movement;

    // Start is called before the first frame update
    void Awake()
    {
        movement = GetComponentInChildren<PlayerMovement>();
    }

    public void HandleInput()
    {
        movement.HandleInput(); 
    }

    void OnTriggerEnter(Collider col)
    {
        movement.HandleTriggerEnter(col);
    }
    void OnTriggerExit(Collider col)
    {
        movement.HandleTriggerExit(col);
    }

    // Update is called once per frame
    void Update()
    {
        // Temporarily here, will be moved up to InputManager/PlayerInputController
        HandleInput();
    }
}
