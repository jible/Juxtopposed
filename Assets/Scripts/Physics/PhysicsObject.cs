using UnityEngine;


[RequireComponent(typeof(DeterministicTransform)), ExecuteAlways]
public class PhysicsObject : MonoBehaviour
{
    [SerializeReference, SubclassSelector]
    public Shape shape;


    public void OnEnable()
    {
        Debug.Log("register! a");
        if (EditorPhysicsShapeRenderer.Instance != null)
        {
            EditorPhysicsShapeRenderer.Instance.RegisterObject(this);
        }
    }
}