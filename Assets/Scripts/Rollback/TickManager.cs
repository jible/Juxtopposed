using System;
using UnityEngine;

// Owned by the DeterministicWorld. Turns real time into ticks and handles rollback,
// while what a tick actually does is the world's step, handed in when it's built
public class TickManager
{
    public static uint _maxTicks = 20;
    // Gameplay values are per second, this converts them to per tick
    public const int TicksPerSecond = 60;
    private const float TickDuration = 1f / TicksPerSecond;
    // Most ticks one frame will run to catch up on real time
    private const int MaxTicksPerFrame = 5;
    // Real time not yet spent on a tick
    private float timeSinceLastTick;
    private uint _currentTick = 0;
    private uint _latestAccessedTick = 0;
    private uint _testRollbackTicks = 5; // You will rollback this many tick when debug rollback is pressed
    private uint _currentTickIndex= 0 ;
    public uint CurrentTickIndex
    {
        get
        {
            return _currentTickIndex;
        }
    }
    public uint CurrentTick
    {
        get
        {
            return _currentTick;
        }
        private set
        {
            _currentTick = value;
            _currentTickIndex = value % _maxTicks;
        }
    }
    private readonly InputManager inputManager;
    private readonly Action step;

    public TickManager(InputManager inputManager, Action step)
    {
        this.inputManager = inputManager;
        this.step = step;
        // Display only, one frame per tick so no tick goes undrawn or drawn twice. Simulation speed comes from Advance's timing.
        // Ignored while vSyncCount is on in the quality settings
        Application.targetFrameRate = TicksPerSecond;
    }

    // The simulation runs TicksPerSecond ticks per second of real time, whatever the frame rate,
    // so it plays at the same speed on every machine. A fast frame can run no ticks, a slow one several.
    // Returns how many ticks ran
    public int Advance(float deltaTime)
    {
        timeSinceLastTick += deltaTime;
        int ticksThisFrame = 0;
        while (timeSinceLastTick >= TickDuration && ticksThisFrame < MaxTicksPerFrame)
        {
            timeSinceLastTick -= TickDuration;
            ticksThisFrame++;

            // Serialize inputs for this tick
            inputManager.SaveInputs(CurrentTickIndex);

            // Decide if you are rolling back this frame
            // if (inputManager.Controllers[0].Value.GetButton(ControllerState.ButtonTypes.JUMP)) // For now, instead of comparing inputs, just press space to resimulate /rollback
            // {
            //     RollbackAndResimulate();
            // }
            Tick();
            _latestAccessedTick = CurrentTick;
            CurrentTick += 1;
        }
        // Time past the cap is dropped, so after a hitch the game slows down for a moment instead of
        // running a burst of ticks that makes the next frame slower still
        if (timeSinceLastTick >= TickDuration) timeSinceLastTick = 0;

        return ticksThisFrame;
    }

    public void RollbackAndResimulate()
    {
        CurrentTick = CurrentTick - _testRollbackTicks;
        SerializableDataManager.LoadAll(CurrentTick);

        while (CurrentTick <= _latestAccessedTick) // Until you have resimulated to
        {
            // Debug.Log("resimulating: " + CurrentTick.ToString());
            Tick();
            CurrentTick += 1;
        }
    }

    private void Tick()
    {
        // Serialize all serializable data
        SerializableDataManager.SaveAll(CurrentTickIndex);
        inputManager.LoadInputs(CurrentTickIndex);
        step();
    }
}

public interface ITickable
{
    public void Tick();
}
