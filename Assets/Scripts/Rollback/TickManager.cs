using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class TickManager : MonoBehaviour
{
    [SerializeField]
    public static uint _maxTicks = 20;
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
    DeterministicTransformManager deterministicTransformManager;
    private ITickable[] tickables;
    static TickManager Instance;
    private PhysicsServer physicsServer;
         
    public void Awake()
    {
        Instance = this;
        inputManager = FindAnyObjectByType<InputManager>();
        deterministicTransformManager = GetComponent<DeterministicTransformManager>();
        physicsServer = GetComponent<PhysicsServer>();
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

    public void Update()
    {
        
        // If the scene is not yet configured, skip frame
        if (!Ready) return;
        
        // Serialize inputs for this frame
        inputManager.SaveInputs(CurrentTickIndex);

        // Decide if you are rolling back this frame
        // if (inputManager.Controllers[0].Value.GetButton(ControllerState.ButtonTypes.JUMP)) // For now, instead of comparing inputs, just press space to resimulate /rollback
        // {
        //     RollbackAndResimulate();
        // }
        Tick();
        _latestAccessedTick = CurrentTick;
        CurrentTick += 1;

        // Once you are done making changes to position, you can update the deterministic transform followers (to be implemented)
        deterministicTransformManager.UpdateTransforms();
        
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
