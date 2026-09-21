using System;
using UnityEditor.PackageManager;
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
    private PhysicsObjectRegistry _physicsObjectRegistry;
    private PhysicsObjectRegistry physicsObjectRegistry
    {
        get
        {
            if (_physicsObjectRegistry == null)
            {
                _physicsObjectRegistry = GetComponentInParent<PhysicsObjectRegistry>();
                if (_physicsObjectRegistry == null)
                {
                    Debug.LogError("Physics Object could not find physics object registry");
                }
            }
            return _physicsObjectRegistry;
        }
    }

    [SerializeField]
    private SerializableProperty<bool> serializedIsActive = new();
    public bool isActive
    {
        get => serializedIsActive.Value;
        set => serializedIsActive.Value = value;
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
        physicsObjectRegistry.Register(this);
    }

    // The registry is static and empty after a script reload, which doesn't call Awake again.
    // Register is a no-op if already registered.
    public void OnEnable()
    {
        physicsObjectRegistry.Register(this);
    }

    private void Reset()
    {
        isActive = true;
    }
}
