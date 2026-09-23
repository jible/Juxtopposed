using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Subsystems;

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

    public static void SaveAll(uint tick)
    {
        foreach ( ISerializable serializable in AllSerializableData)
        {
            serializable.Save(tick);
        }
    }
    public static void LoadAll( uint tick)
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
    public void Save(uint tickIndex);
    public void Load(uint tickIndex);
}

[System.Serializable]
public class SerializableProperty<T> : ISerializable where T : unmanaged
{
    public T Value;
    // Lets the owner react to a rollback load, which writes Value directly and bypasses any property setter
    public System.Action OnLoaded;
    private readonly T[] values = new T[TickManager._maxTicks];
    public void Save(uint tickIndex)
    {
        values[tickIndex] = Value;
    }
    public void Load(uint tickIndex)
    {
        Value = values[tickIndex];
        OnLoaded?.Invoke();
    }
    public SerializableProperty(){
        SerializableDataManager.Register(this);
    }
    public T GetDataFromFrame(uint tickIndex)
    {
        return values[tickIndex];
    }

}

// This is a serializable data instance that doesn't register so the object can handle loading and saving its data on its own, instead of passing it onto the manager
public class UnregisteredSerializableData<T> : ISerializable where T : unmanaged
{
    public T Value;
    private readonly T[] values = new T[TickManager._maxTicks];
    public void Save(uint tickIndex)
    {
        values[tickIndex] = Value;
    }
    public void Load(uint tickIndex)
    {
        Value = values[tickIndex];
    }
    public T GetDataFromFrame(uint tickIndex)
    {
        return values[tickIndex];
    }

}
