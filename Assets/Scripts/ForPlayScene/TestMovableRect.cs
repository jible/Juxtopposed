using System;
using UnityEngine;

public class TestMovableRect : MonoBehaviour , ITickable
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField]
    private InputManager inputManager;


    // Update is called once per frame
    public void Tick()
    {
        DMVector stick = inputManager.Controllers[0].Value.LeftStick.ToVector();

        // GetComponent<DeterministicTransform>().position +=stick /60;
        GetComponent<PhysicsObject>().velocity.Value = stick /60;

    }
}
