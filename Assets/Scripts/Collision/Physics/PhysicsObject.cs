using System;
using Unity.VisualScripting;
using UnityEngine;


// Collected by the DeterministicWorld when it is built, so it never registers itself anywhere
[RequireComponent(typeof(DeterministicTransform))]
public class PhysicsObject : MonoBehaviour
{
    [SerializeReference, SubclassSelector]
    public Shape shape;
    [DoNotSerialize, HideInInspector]
    public SerializableProperty<DMVector> velocity;
    /// <summary>
    /// The Mask is what layer this object "sees". For collision, the non-static object should 
    /// be mask the static one
    /// </summary>
    [Tooltip("The Mask is what layer this object \"sees\". For collision, the non-static object should be mask the static one")]
    public int Mask;
    /// <summary>
    /// The Layer is what layer this object exists on. 
    /// For collision, the static object should be masked by the static one
    /// </summary>
    [Tooltip("The Layer is what layer this object exists on. For collision, the static object should be masked by the static one")]
    public int Layer;
    public bool isStatic = false;
    [Tooltip("Whether the physics shape renderers should draw this object's shape.")]
    public bool renderShape = true;
    [Tooltip("Color the physics shape renderers draw this object's shape with.")]
    public Color renderColor = new Color(1f, 1f, 1f, 0.5f);
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

    private void Reset()
    {
        isActive = true;
    }
}
