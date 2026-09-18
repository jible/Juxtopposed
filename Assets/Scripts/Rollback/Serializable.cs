using System.Collections.Generic;
using Unity.VisualScripting;

public static class SerializableDataManager
{
    public static List<ISerializable> AllSerializableData = new();
    private static readonly HashSet<ISerializable> registered = new();
    public static void Register(ISerializable serializable)
    {
        if (registered.Contains(serializable)) return;
        AllSerializableData.Add(serializable);
        registered.Add(serializable);
    }
    public static void Reset()
    {
        AllSerializableData.Clear();
        registered.Clear();
    }

    public static void SaveAll(int tick)
    {
        foreach ( ISerializable serializable in AllSerializableData)
        {
            serializable.Save(tick);
        }
    }
    public static void LoadAll( int tick)
    {
        foreach ( ISerializable serializable in AllSerializableData)
        {
            serializable.Load(tick);
        }
    }
}
// Each serializable object
public interface ISerializable
{
    public void Save(int tickIndex);
    public void Load(int tickIndex);
}

[System.Serializable]
public class SerializableData<T> : ISerializable where T : unmanaged
{
    public T Value;
    private readonly T[] values = new T[TickManager._maxTicks];
    public void Save(int tickIndex)
    {
        values[tickIndex] = Value;
    }
    public void Load(int tickIndex)
    {
        Value = values[tickIndex];
    }
    public SerializableData(){
        SerializableDataManager.Register(this);
    }

}
