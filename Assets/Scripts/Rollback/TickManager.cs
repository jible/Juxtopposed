using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class TickManager : MonoBehaviour
{
    [SerializeField]
    public static uint _maxTicks = 20;
    private uint _currentTick = 0;
    private uint _latestAccessedTick = 0;
    private uint _testRollbackTicks = 5; // You will rollback this many tick when debug rollback is pressed
    private bool isRollingBack =false;
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
         
    public void Awake()
    {
        Instance = this;
        inputManager = FindAnyObjectByType<InputManager>();
        tickables = GetAllTickables(transform);
        physicsServer = GetComponent<PhysicsServer>();
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

    public void Update()
    {
        
        // If the scene is not yet configured, skip frame
        if (!Ready) return;
        
        // Serialize inputs for this frame
        inputManager.SaveInputs(CurrentTickIndex);

        // Decide if you are rolling back this frame
        if (Input.GetKeyDown(KeyCode.R)) // For now, instead of comparing inputs, just press R to resimulate /rollback
        {
            // To roll back
            isRollingBack = true;
            CurrentTick = CurrentTick - _testRollbackTicks;

            while (CurrentTick <= _latestAccessedTick) // Until you have resimulated to
            {
                SerializableDataManager.LoadAll(CurrentTick);
                Tick();
                CurrentTick += 1;
            }

        }
        Tick();
        _latestAccessedTick = CurrentTick;
        CurrentTick += 1;
        
        // Once you are done making changes to position, you can update the deterministic transform followers (to be implemented)
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
