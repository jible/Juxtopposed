using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TickManager : MonoBehaviour
{
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
    static TickManager Instance;


    public void _Ready()
    {
        Instance = this;
    }


    public void Tick()
    {
        // Tick each object

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
