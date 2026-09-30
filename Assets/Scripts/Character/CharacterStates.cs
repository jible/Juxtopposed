public class IdleState : CharacterState
{
    public override CharacterStateId Id => CharacterStateId.Idle;
    public override bool ApplyGravity => false;

    public override void Tick(CharacterStateMachine machine)
    {
        if (machine.Character.inputGetter.justPressed(ControllerState.ButtonTypes.JUMP))
        {
            machine.ChangeState(CharacterStateId.Jump);
        }
        else if (machine.Character.Controller.LeftStick.ToVector() != DMVector.zero)
        {
            machine.ChangeState(CharacterStateId.Walk);
        }
    }
}

public class WalkState : CharacterState
{
    public override CharacterStateId Id => CharacterStateId.Walk;

    public override bool ApplyGravity => false;

    public override void Tick(CharacterStateMachine machine)
    {
        Character character = machine.Character;
        if (character.inputGetter.justPressed(ControllerState.ButtonTypes.JUMP))
        {
            machine.ChangeState(CharacterStateId.Jump);
        }
        else if (character.inputGetter.LeftStick() == DMVector.zero)
        {
            machine.ChangeState(CharacterStateId.Idle); 
        } else if (character.PhysicsObject.velocity.Value.x.Sign() != character.inputGetter.LeftStick().x.Sign())
        {
            machine.ChangeState(CharacterStateId.Turn); // If they click a different direction, get a kick
            
        }

    }
}

public class JumpState : CharacterState
{
    public override CharacterStateId Id => CharacterStateId.Jump;
    public override bool ApplyGravity => true;

    public override void EnterState(CharacterStateMachine machine)
    {
        machine.Character.characterMovement.Jump();
    }

    public override void Tick(CharacterStateMachine machine)
    {
        if (machine.Character.characterMovement.IsGrounded)
        {
            machine.ChangeState(CharacterStateId.Idle);
        }
        // Placeholder until there's a jump animation to end on
        else if (machine.Character.PhysicsObject.velocity.Value.y < 0)
        {
            machine.ChangeState(CharacterStateId.Fall);
        }
    }
}

public class FallState : CharacterState
{
    public override CharacterStateId Id => CharacterStateId.Fall;
    public override bool ApplyGravity => true;

    public override void Tick(CharacterStateMachine machine)
    {
        if (machine.Character.characterMovement.IsGrounded)
        {
            machine.ChangeState(CharacterStateId.Idle);
        }
    }
}

public class TurnState : CharacterState
{
    public override CharacterStateId Id => CharacterStateId.Turn;
    public override bool ApplyGravity => false;


    public override void EnterState(CharacterStateMachine machine)
    {
        // give the kick
        machine.Character.characterMovement.GroundedTurnAroundKick();
    }
    public override void OnAnimFinished(CharacterStateMachine machine)
    {
        // Give the turn around kick
        machine.ChangeState(CharacterStateId.Walk);
    }

    // When the anim ends 
}
