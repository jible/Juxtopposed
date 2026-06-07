using UnityEngine;

[RequireComponent(typeof(DeterministicTransform))]
public class PhysicsObject : MonoBehaviour
{
    [SerializeReference]
    public Shape shape = null;



}