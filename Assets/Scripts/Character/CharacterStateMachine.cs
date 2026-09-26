using System;

// Every character state. Values must stay contiguous starting at 0, since they index the state table
public enum CharacterStateId : byte
{
    Idle,
    Walk,
}

// Everything the state machine needs to roll back. Kept unmanaged so it fits in a SerializableProperty
public struct CharacterStateData
{
    public CharacterStateId Id;
    public uint TicksInState;
}

public class CharacterStateMachine
{
    // One instance of each state, shared by every character. States must not hold data, per character data belongs on the Character
    private static readonly CharacterState[] states = BuildStateTable(
        new IdleState(),
        new WalkState()
    );

    private static CharacterState[] BuildStateTable(params CharacterState[] stateList)
    {
        var table = new CharacterState[Enum.GetValues(typeof(CharacterStateId)).Length];
        foreach (var state in stateList)
        {
            table[(int)state.Id] = state;
        }
        for (int i = 0; i < table.Length; i++)
        {
            if (table[i] == null)
            {
                throw new InvalidOperationException($"No state registered for {(CharacterStateId)i}");
            }
        }
        return table;
    }

    public readonly Character Character;
    private readonly SerializableProperty<CharacterStateData> data = new();
    // Only lives for the duration of a tick, so it doesn't need to be rolled back
    private bool changedThisTick;

    public CharacterStateId CurrentStateId => data.Value.Id;
    public uint TicksInState => data.Value.TicksInState;
    private CharacterState CurrentState => states[(int)data.Value.Id];

    public CharacterStateMachine(Character character, CharacterStateId startingState)
    {
        Character = character;
        data.Value = new CharacterStateData { Id = startingState, TicksInState = 0 };
        CurrentState.EnterState(this);
    }

    public bool IsInState(CharacterStateId id) { return data.Value.Id == id; }

    public void ChangeState(CharacterStateId id)
    {
        if (IsInState(id)) return;

        CurrentState.ExitState(this);
        data.Value = new CharacterStateData { Id = id, TicksInState = 0 };
        changedThisTick = true;
        CurrentState.EnterState(this);
    }

    public void Tick()
    {
        changedThisTick = false;
        CurrentState.Tick(this);

        // A state entered this tick sees TicksInState == 0 on its first tick
        if (!changedThisTick)
        {
            data.Value.TicksInState++;
        }
    }
}

public abstract class CharacterState
{
    public abstract CharacterStateId Id { get; }
    public virtual void EnterState(CharacterStateMachine machine) { }
    public virtual void ExitState(CharacterStateMachine machine) { }
    public virtual void Tick(CharacterStateMachine machine) { }

    /*
    Paste this into a class to get the defaults:

    public override CharacterStateId Id => CharacterStateId.;
    public override void EnterState(CharacterStateMachine machine) { }
    public override void ExitState(CharacterStateMachine machine) { }
    public override void Tick(CharacterStateMachine machine) { }
     */
}
