using UnityEngine;


[RequireComponent(typeof(DeterministicTransform)), ExecuteAlways]
public class PhysicsObject : MonoBehaviour
{
    [SerializeReference, SubclassSelector]
    public Shape shape;
    public int mask;
    public int layer;
    public bool isActive = true;
    public ObjectType objectType;

    public enum ObjectType
    {
        TriggerBox,
        CollisionObject
    }

    public void Start()
    {
        if (physicsShapeRenderer.Instance != null)
        {
            physicsShapeRenderer.Instance.RegisterObject(this);
        }
    }
}