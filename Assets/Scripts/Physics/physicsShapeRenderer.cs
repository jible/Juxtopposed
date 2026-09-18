using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteAlways]
public class PhysicsShapeRenderer : MonoBehaviour
{
    private List<Shape> shapes = new List<Shape>();
    private PhysicsObjectRegistry physicsObjectRegistry;
    public float renderZAxis = 0;
    float renderThickness = .5f;

    void Awake()
    {
        physicsObjectRegistry = GetComponent<PhysicsObjectRegistry>();
    }

    public void OnDrawGizmos()
    {
        foreach (var i in physicsObjectRegistry.All)
        {
            if (i == null)
            {
                continue;
            }

            if (i.shape == null)
            {
                continue;
            }

            if (i.shape is Square s)
            {
                RenderSquare(i,s);
            }
        }
    }

    private void RenderSquare(PhysicsObject owner,Square s)
    {
        DeterministicTransform transform = owner.GetComponent<DeterministicTransform>();
        Gizmos.DrawCube(transform.globalPosition.ToVector3(renderZAxis), s.size.ToVector3(renderThickness));
    }
}
//