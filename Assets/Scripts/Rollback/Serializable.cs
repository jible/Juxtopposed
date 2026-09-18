using System.Collections.Generic;

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
    public T Current;
    private readonly T[] values = new T[TickManager._maxTicks];
    public void Save(int tickIndex)
    {
        values[tickIndex] = Current;
    }
    public void Load(int tickIndex)
    {
        Current = values[tickIndex];
    }
    public SerializableData(){
        SerializableDataManager.Register(this);
    }

}
