using UnityEngine;

public class CharacterMovement : MonoBehaviour, ITickable
{
    private InputManager.InputGetter inputGetter;
    PhysicsObject physicsObject;
    Character character;
    public void Awake()
    {
        physicsObject= GetComponent<PhysicsObject>();
        character = GetComponent<Character>();
        physicsObject.Colliding += onCollide;
    }

    public void Config(InputManager.InputGetter inputGetter)
    {
        this.inputGetter = inputGetter;
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
        DMVector stick = inputGetter.LeftStick();

        DM64 max = new DM64(2) / 60;
        physicsObject.velocity.Value.x += (stick.x /60) /2;

        // deteriorate the speed
        physicsObject.velocity.Value.x *= new DM64(9) /10;

        physicsObject.velocity.Value.x =physicsObject.velocity.Value.x.Clamped(-max, max);


        if (inputGetter.justPressed(ControllerState.ButtonTypes.JUMP))
        {
            physicsObject.velocity.Value.y += new DM64(3) / 60;
        }

        // Apply gravity
        physicsObject.velocity.Value.y -= new DM64(1) / 2 / 60; // 1 meter per second/ per second?
        physicsObject.velocity.Value.y =physicsObject.velocity.Value.y.Clamped(-max, max);

    }
}
