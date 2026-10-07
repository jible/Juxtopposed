using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class TickManager : MonoBehaviour
{
    [SerializeField]
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
    public bool Ready = false;
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
    InputManager inputManager;
    private ITickable[] tickables;
    static TickManager Instance;
    private PhysicsServer physicsServer;
    private HitboxManager hitboxManager;
         
    public void Awake()
    {
        Instance = this;
        inputManager = FindAnyObjectByType<InputManager>();
        physicsServer = GetComponent<PhysicsServer>();
        // Display only, one frame per tick so no tick goes undrawn or drawn twice. Simulation speed comes from Update's timing.
        // Ignored while vSyncCount is on in the quality settings
        Application.targetFrameRate = TicksPerSecond;
        hitboxManager = GetComponent<HitboxManager>();
    }

    // Called by the play manager once every tickable exists, including spawned characters
    public void CollectTickables()
    {
        tickables = GetAllTickables(transform);
    }

    private ITickable[] GetAllTickables(Transform parent)
    {
        Queue<Transform> queue = new();
        List<ITickable> tickables = new();
        queue.Enqueue(parent);
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            // Gets all tickable components in component order
            var currentTickables = current.GetComponents<ITickable>();
            foreach (var tickable in currentTickables)
            {
                tickables.Add(tickable);
            }
            
            foreach (Transform child in current.transform)
            {
                queue.Enqueue(child);
            }
        }
        return tickables.ToArray();
    }

    // The simulation runs TicksPerSecond ticks per second of real time, whatever the frame rate,
    // so it plays at the same speed on every machine. A fast frame can run no ticks, a slow one several
    public void Update()
    {

        // If the scene is not yet configured, skip frame
        if (!Ready) return;

        timeSinceLastTick += Time.deltaTime;
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

        // Once you are done making changes to position, push them to the unity transforms
        if (ticksThisFrame > 0) DeterministicTransformRegistry.SyncAll();

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

    public void Tick()
    {
        // Serialize all serializable data
        SerializableDataManager.SaveAll(CurrentTickIndex);
        inputManager.LoadInputs(CurrentTickIndex);
        foreach (var tickable in tickables)
        {
            tickable.Tick();
        }
        physicsServer.Tick();
        // After physics, so hits are checked at final positions
        hitboxManager.Tick();
    }
}

public static class TickableManager
{
    public static List<ITickable> collection= new();
    public static void Register(ITickable tickable)
    {
        collection.Add(tickable);
    }

    public static void Reset()
    {
        collection.Clear();
    }
}
public interface ITickable
{
    public void Tick();
}
