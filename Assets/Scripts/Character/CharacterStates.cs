public class IdleState : CharacterState
{
    public override CharacterStateId Id => CharacterStateId.Idle;

    public override void Tick(CharacterStateMachine machine)
    {
        if (machine.Character.Controller.LeftStick.ToVector() != DMVector.zero)
        {
            machine.ChangeState(CharacterStateId.Walk);
        }
    }
}

public class WalkState : CharacterState
{
    public override CharacterStateId Id => CharacterStateId.Walk;

    public override void Tick(CharacterStateMachine machine)
    {
        if (machine.Character.Controller.LeftStick.ToVector() == DMVector.zero)
        {
            machine.ChangeState(CharacterStateId.Idle);
        }
    }
}
