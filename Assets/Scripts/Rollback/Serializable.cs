using System.Collections.Generic;
using UnityEngine;

public static class SerializableDataManager
{
    // List instead of a set cause im trusting that nothing will register twice
    public static List<ISerializable> AllSerializableData = new();
    public static void Register(ISerializable serializable)
    {
        AllSerializableData.Add(serializable);
    }
    public static void Reset()
    {
        AllSerializableData.Clear();
    }
}
// Each serializable object 
public interface ISerializable
{
    public void Save(int tickIndex);
    public void Load(int tickIndex);
}
public class SerializableData<T>: ISerializable where T: unmanaged
{
    public T Current;
    private T[] values= new T[TickManager._maxTicks] ;
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