using System;
using UnityEngine;
using UnityEngine.Rendering;

public class TestMovableRect : MonoBehaviour , ITickable
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField]
    private InputManager inputManager;
    PhysicsObject physicsObject;
    public void Awake()
    {
        physicsObject= GetComponent<PhysicsObject>();
        physicsObject.Colliding += onCollide;
    }

    public void onCollide(PhysicsObject colA, PhysicsObject otherObj, DMVector side)
    {
        if (side == DMVector.Down)
        {
            physicsObject.velocity.Value.y = new(0);
        }
    }
    // Update is called once per frame
    public void Tick()
    {
        DMVector stick = inputManager.Controllers[0].Value.LeftStick.ToVector();

        DM64 max = new DM64(2) / 60;
        physicsObject.velocity.Value.x += (stick.x /60) /2;

        // deteriorate the speed
        physicsObject.velocity.Value.x *= new DM64(9) /10;

        physicsObject.velocity.Value.x =physicsObject.velocity.Value.x.Clamped(-max, max);


        if (inputManager.Controllers[0].Value.GetButton(ControllerState.ButtonTypes.JUMP))
        {
            physicsObject.velocity.Value.y += new DM64(3) / 60;
        }

        // Apply gravity
        physicsObject.velocity.Value.y -= new DM64(1) / 2 / 60; // 1 meter per second/ per second?
        physicsObject.velocity.Value.y =physicsObject.velocity.Value.y.Clamped(-max, max);

    }
}
