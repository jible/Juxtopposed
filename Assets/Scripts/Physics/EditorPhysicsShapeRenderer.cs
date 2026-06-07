using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteAlways]
public class EditorPhysicsShapeRenderer : MonoBehaviour
{
    public static EditorPhysicsShapeRenderer Instance { get; private set; }
    private bool doRun = false;
    private List<Shape> shapes = new List<Shape>();
    private bool CollectionDirty = true;
    private List< PhysicsObject>allPhysicsObjects = new();

    public float renderZAxis = 0;
    float renderThickness = .5f;

    public void Start()
    {
        GetPhysicsObjects();
        
    }


    private void GetPhysicsObjects()
    {
        allPhysicsObjects = new (Object.FindObjectsByType<PhysicsObject>(FindObjectsSortMode.None) );
    }


    public void RegisterObject(PhysicsObject newObject)
    {
        if (allPhysicsObjects.Contains(newObject))
        {
            return;
        }

        allPhysicsObjects.Append(newObject);
    }

    public void OnDrawGizmos()
    {
        List<PhysicsObject> toRemove = new List<PhysicsObject>();
        foreach (var i in allPhysicsObjects)
        {
            if (i == null)
            {
                toRemove.Add(i);
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
        Gizmos.DrawCube(transform.position.ToVector3(renderZAxis), s.size.ToVector3(renderThickness));
    }
}