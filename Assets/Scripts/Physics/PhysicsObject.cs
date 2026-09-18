using System;
using UnityEngine;


[RequireComponent(typeof(DeterministicTransform)), ExecuteAlways]
public class PhysicsObject : SerializableData<bool>
{
    [SerializeReference, SubclassSelector]
    public Shape shape;
    public int mask;
    public int layer;
    public bool isStatic = false;
    public ObjectType objectType;

    public bool isActive
    {
        get => Current;
        set => Current = value;
    }

    /// <summary>
    /// Emitted when this object has been entered by another object
    /// </summary>
    public event Action<PhysicsObject, PhysicsObject> Overlapping;

    public enum ObjectType
    {
        TriggerBox,
        CollisionObject
    }
    /// <summary>
    /// Call this to emit the Overlapping Event- when this is the trigger and detects other.
    /// </summary>
    /// <param name="other"></param>
    public void OnOverlap(PhysicsObject other)
    {
        Overlapping?.Invoke(this, other);
    }

    protected override void Awake()
    {
        base.Awake();
        PhysicsObjectRegistry.Register(this);
    }

    private void Reset()
    {
        Current = true;
    }
}
