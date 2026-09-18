using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TickManager : MonoBehaviour
{
    [SerializeField]
    public GameObject TickableRoot;
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
         
    private PhysicsShapeRenderer physicsShapeRenderer;
    public void _Ready()
    {
        Instance = this;
        tickables = GetAllTickables(TickableRoot);
        physicsServer = GetComponent<PhysicsServer>();
        physicsShapeRenderer = GetComponent<PhysicsShapeRenderer>();
    }

    private ITickable[] GetAllTickables(GameObject parent)
    {
        return parent.GetComponentsInChildren<ITickable>();
    }

    public void SerializeState()
    {
        
    }

    public void Tick()
    {
        // Serialize the whole game state
        SerializeGameState();

        // Tick each object
        foreach (var tickable in tickables)
        {
            tickable.Tick();
        }

        // Then tick the physics manager
        physicsServer.Tick();
    }

    public void SerializeGameState()
    {
        // physicsServer.SerializeState();
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
