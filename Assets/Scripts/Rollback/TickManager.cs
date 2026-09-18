using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class TickManager : MonoBehaviour
{
    [SerializeField]
    public static int _maxTicks = 20;
    private static int _currentTick = 0;
    private static int _currentTickIndex= 0 ;
    public static int CurrentTickIndex
    {
        get
        {
            return _currentTickIndex;
        }
    }
    public static int CurrentTick
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
    private ITickable[] tickables;
    static TickManager Instance;
    private PhysicsServer physicsServer;
         
    public void _Ready()
    {
        Instance = this;
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
            var tickable = current.GetComponent<ITickable>();
            if (tickable != null)
            {
                tickables.Append(tickable);
            }
            foreach (Transform child in current.transform)
            {
                queue.Enqueue(child);
            }
        }
        return tickables.ToArray();
    }

    public void Tick()
    {
        // Serialize all serializable data
        SerializableDataManager.SaveAll(CurrentTick);

        // Tick each object
        foreach (var tickable in tickables)
        {
            tickable.Tick();
        }

        // Then tick the physics manager
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
