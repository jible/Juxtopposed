using System;
using UnityEngine;


[RequireComponent(typeof(DeterministicTransform)), ExecuteAlways]
public class PhysicsObject : MonoBehaviour
{
    [SerializeReference, SubclassSelector]
    public Shape shape;
    public int mask;
    public int layer;
    public bool isStatic = false;
    public ObjectType objectType;

    [SerializeField]
    private SerializableData<bool> serializedIsActive = new();
    public bool isActive
    {
        get => serializedIsActive.Current;
        set => serializedIsActive.Current = value;
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

    public void Awake()
    {
        PhysicsObjectRegistry.Register(this);
    }

    private void Reset()
    {
        isActive = true;
    }
}
