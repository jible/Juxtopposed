using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;

[DefaultExecutionOrder(-10000)]
[ExecuteAlways]
public class PhysicsServer : MonoBehaviour
{
    public DM64 Epsilon = new(.0001f);


    [SerializeField]

    public DM64 HashGridSize = new(30);
    private PhysicsObjectRegistry physicsObjectRegistry;

    // Awake is not called again after a script reload (domain reload), but statics are wiped.

    public void Awake()
    {
        // Runs before any PhysicsObject/DeterministicTransform Awake (DefaultExecutionOrder),
        // so this clear always happens before those objects register themselves.
        physicsObjectRegistry = GetComponent<PhysicsObjectRegistry>();
        SerializableDataManager.Reset();
    }


    public void Tick()
    {
        // Spacial Hashing:
        var tileToCells = new Dictionary<Vector2Int, List<PhysicsObject>>();
        var objectToHashCells = new Dictionary<PhysicsObject, List<Vector2Int>>();

        HashObjects(
            tileToCells,
            objectToHashCells
        );

        var interacted = new HashSet<(PhysicsObject, PhysicsObject)>();
        foreach (var entityA in physicsObjectRegistry.All)
        {
            if (entityA == null)
            {
                continue;
            }
            // iterate through each object it is overlapping
            if ( !entityA.isActive || entityA.shape == null)
            {
                continue;
            }
            foreach (var tile in objectToHashCells[entityA])
            {
                foreach (var entityB in tileToCells[tile])
                {
                    if (interacted.Contains((entityA, entityB)) ||entityB == entityA || entityA.shape == null || entityB.shape == null )
                    {
                        continue;
                    }
                    interacted.Add((entityA, entityB));
                    interacted.Add((entityB, entityA));
                    CheckForOverlap(entityA, entityB);
                }
            }
        }
    }

    public void CheckForOverlap(PhysicsObject a, PhysicsObject b)
    {
        /*
         * Check if any of these cases can occur:
         * 
         * both are collision objects and one is static the other is not and the dynamic one masks the static one's layer?
         * 
         * one or both are triggers and the trigger masks the other object's layer
         */
        bool bothColliders = a.objectType == PhysicsObject.ObjectType.CollisionObject && b.objectType == PhysicsObject.ObjectType.CollisionObject;
        bool aCollisionB = bothColliders && !a.isStatic && b.isStatic && (a.mask & b.layer) != 0;
        bool bCollisionA = bothColliders && !b.isStatic && a.isStatic && (b.mask & a.layer) != 0;
        bool aTriggeredByB = a.objectType == PhysicsObject.ObjectType.TriggerBox && (a.mask & b.layer) != 0;
        bool bTriggeredByA = b.objectType == PhysicsObject.ObjectType.TriggerBox && (b.mask & a.layer) != 0;

        if (!(aCollisionB || bCollisionA || aTriggeredByB || bTriggeredByA))
        {
            return;
        }


        // If at least one instance requires a check:
        if (OverlapChecker.CheckOverlap(a, b))
        {
            if (aTriggeredByB)
            {
                HandleTrigger(a, b);
            }
            if (bTriggeredByA)
            {
                HandleTrigger(b, a);
            }
            if (aCollisionB )
            {
                HandleCollision(a, b);
            }
            if (bCollisionA)
            {
                HandleCollision(b,a);
            }
        }
    }

    /// <summary>
    /// Handles the case such that a is a trigger and it masks b and overlaps b - Emits signal that a has been triggered
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    public void HandleTrigger(PhysicsObject a, PhysicsObject b)
    {
        // Already checked for overlap- just handle it
        a.OnOverlap(b);


    }

    public void HandleCollision(PhysicsObject a, PhysicsObject b)
    {
        // Resolve the edges of the collision by moving the non-static object out of the static one
        // In this case, A is dynamic, b is static
        // Thus a will be repelled from b's surface




    }

    // Spacial Hash Makers:

    private void HashObjects(
        Dictionary<Vector2Int, List<PhysicsObject>> tileToCells,
        Dictionary<PhysicsObject, List<Vector2Int>> objectToHashCells)
    {
        foreach( var entity in physicsObjectRegistry.All)
        {
            if (entity == null) // In theory should never happen
            {
                Debug.LogError("Encountered null physics object. Was a physics object deleted in play?");
                continue;
            } 
            if (!entity.isActive || entity.shape == null)
            {
                continue;
            }
            var overlap = GetOverlappingTiles(entity);
            foreach (var tile in overlap)
            {
                if (!tileToCells.ContainsKey(tile))
                {
                    tileToCells[tile] = new List<PhysicsObject>();
                }
                tileToCells[tile].Add(entity);
            }
            objectToHashCells[entity] = overlap;

        }
    }
    

    private List<Vector2Int> GetOverlappingTiles(PhysicsObject entity)
    {
        var output = new List<Vector2Int>();
        entity.shape.GetBounds(entity.GetComponent<DeterministicTransform>().globalPosition, out DMVector min, out DMVector max);

        // Tiles are keyed by grid cell index, so objects in the same cell always share a key
        int leftCell = (min.x / HashGridSize).Floor().to_int();
        int rightCell = (max.x / HashGridSize).Floor().to_int();
        int downCell = (min.y / HashGridSize).Floor().to_int();
        int upCell = (max.y / HashGridSize).Floor().to_int();

        for (int x = leftCell; x <= rightCell; x++)
        {
            for (int y = downCell; y <= upCell; y++)
            {
                output.Add(new Vector2Int(x, y));
            }
        }

        return output;
    }


    private static class OverlapChecker
    {
        public static bool CheckOverlap(PhysicsObject a, PhysicsObject b)
        {

            // PICK UP FROM HERE
            if (a.shape is Square aSquare && b.shape is Square bSquare)
            {
                return SquareSquareOverlap(aSquare, a.GetComponent<DeterministicTransform>(), bSquare, b.GetComponent<DeterministicTransform>());
            }
            else if (a.shape is Circle aCircle && b.shape is Circle bCircle)
            {
                return CircleCircleOverlap(aCircle, a.GetComponent<DeterministicTransform>(), bCircle, b.GetComponent<DeterministicTransform>());
            }
            else if (a.shape is Square aSquare2 && b.shape is Circle bCircle2)
            {
                return SquareCircleOverlap(aSquare2, a.GetComponent<DeterministicTransform>(), bCircle2, b.GetComponent<DeterministicTransform>());
            }
            else if (a.shape is Circle aCircle2 && b.shape is Square bSquare2)
            {
                return SquareCircleOverlap(bSquare2, b.GetComponent<DeterministicTransform>(), aCircle2, a.GetComponent<DeterministicTransform>());
            }
            return false;

        }
        

        public static bool SquareSquareOverlap(Square a, DeterministicTransform aTransform, Square b, DeterministicTransform bTransform)
        {
            a.GetBounds(aTransform.globalPosition, out DMVector aMin, out DMVector aMax);
            b.GetBounds(bTransform.globalPosition, out DMVector bMin, out DMVector bMax);

            return (
                (aMin.x < bMax.x) &&
                (aMax.x > bMin.x) &&
                (aMin.y < bMax.y) &&
                (aMax.y > bMin.y)
            );

        }
        public static bool CircleCircleOverlap(Circle a, DeterministicTransform aTransform, Circle b, DeterministicTransform bTransform)
        {
            return false;
        }

        public static bool SquareCircleOverlap(Square a, DeterministicTransform aTransform, Circle b, DeterministicTransform bTransform)
        {
            return false;
        }

    }
}