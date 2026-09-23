using System;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;


[RequireComponent(typeof(DeterministicTransform)), ExecuteAlways]
public class PhysicsObject : MonoBehaviour
{
    [SerializeReference, SubclassSelector]
    public Shape shape;
    [DoNotSerialize, HideInInspector]
    public SerializableProperty<DMVector> velocity;
    public int mask;
    public int layer;
    public bool isStatic = false;
    [Tooltip("Whether PhysicsShapeRenderer should draw this object's shape.")]
    public bool renderShape = true;
    [Tooltip("Color PhysicsShapeRenderer draws this object's shape with.")]
    public Color renderColor = new Color(1f, 1f, 1f, 0.5f);
    public int physicsObjectID = -1;
    [DoNotSerialize, HideInInspector]
    public int visitStamp;
    private DeterministicTransform _deterministicTransform =null;
    [HideInInspector]
    public DeterministicTransform deterministicTransform
    {
        get
        {
            if (_deterministicTransform == null)
            {
                _deterministicTransform = GetComponent<DeterministicTransform>();
            }
            return _deterministicTransform;
        }
    }
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

    /// <summary>
    /// Emitted when this body was stopped by another object.
    /// The DMVector is the side of the collision: zero on the axis that wasn't hit, and on the
    /// hit axis, the sign of this body's velocity relative to the other object's along it.
    /// </summary>
    public event Action<PhysicsObject, PhysicsObject, DMVector> Colliding;

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

    /// <summary>
    /// Call this to emit the Colliding event- when this body collided with other.
    /// </summary>
    /// <param name="other"></param>
    /// <param name="side">The side of the collision (see the Colliding event's doc comment).</param>
    public void OnCollide(PhysicsObject other, DMVector side)
    {
        Colliding?.Invoke(this, other, side);
    }

    public void Awake()
    {
        physicsObjectRegistry.Register(this);
    }

    // The registry is static and empty after a script reload, which doesn't call Awake again.
    // Register is a no-op if already registered.
    public void OnEnable()
    {
        physicsObjectID = physicsObjectRegistry.Register(this);
    }

    private void Reset()
    {
        isActive = true;
    }
}
