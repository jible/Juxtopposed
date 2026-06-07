using UnityEngine;

[RequireComponent(typeof(DeterministicTransform))]
public class PhysicsObject : MonoBehaviour
{
    [SerializeReference, SubclassSelector]
    public Shape shape = null;



}