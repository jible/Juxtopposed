using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using UnityEngine;
using UnityEngine.Animations;

[ExecuteAlways]
public class PhysicsServer : MonoBehaviour
{
    public static PhysicsServer Instance { get; private set; }
    public List<PhysicsObject> AllEntities = new List<PhysicsObject>();
    public DM64 Epsilon = new(.0001f);

    [SerializeField]
    public DeterministicTransformManager PhysicsRoot;

    [SerializeField]
    public DMVector EditorHashGridSize = new(30);


    public void Start()
    {
        Debug.Log("PhysicsServer Start");
        Instance = this;
        if (PhysicsRoot == null)
        {
            throw new System.Exception("PhysicsRoot is not assigned in PhysicsServer.");
        }

        AllEntities.Clear();
        GetAllEntities(AllEntities, null, true);
    }

    public void RegisterObject(PhysicsObject obj)
    {
        if (!AllEntities.Contains(obj))
        {
            AllEntities.Add(obj);
        }
    }

    private void GetAllEntities(List<PhysicsObject> output, PhysicsObject parent = null,bool first = false)
    {
        if (first)
        {
            output.Clear();
            foreach (var child in PhysicsRoot.GetComponentsInChildren<PhysicsObject>(true))
            {
                if (child != parent)
                {
                    GetAllEntities(output, child);
                }
            }
            return;

        }
        if (parent == null)
        {
            return;
        }

        output.Add(parent);
        foreach (var child in parent.GetComponentsInChildren<PhysicsObject>(true))
        {
            if (child != parent)
            {
                GetAllEntities(output, child);
            }
        }
    }

    public void Tick()
    {
        // Maybe add a check to make sure the game is not running in the editor?

        // Spacial Hashing:
        var tileToCells = new Dictionary<Vector2Int, List<PhysicsObject>>();
        var objectToHashCells = new Dictionary<PhysicsObject, List<Vector2Int>>();

        HashObjects(
            tileToCells,
            objectToHashCells
        );

        var toRemove = new List<PhysicsObject>();
        var interacted = new HashSet<(PhysicsObject, PhysicsObject)>();
        foreach (var entityA in AllEntities)
        {
            if (entityA == null)
            {
                toRemove.Add(entityA);
                continue;
            }
            // itterate through each object it is overlapping
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
                    CheckForOverlap(entityA, entityB);


                }


            }
        }
        foreach (var entity in toRemove)
        {
            AllEntities.Remove(entity);
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
        bool bothColiders = a.objectType == PhysicsObject.ObjectType.CollisionObject && b.objectType == PhysicsObject.ObjectType.CollisionObject;
        bool aCollisionB = bothColiders && b.isStatic && (a.mask & b.layer) == 0;
        bool bCollisionA = bothColiders && a.isStatic && (b.mask & a.layer) == 0;
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
            if (aCollisionB || bCollisionA)
            {
                HandleCollision(a, b);
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
        // If both are static or neither are static, nothing happens
        if (a.isStatic && b.isStatic)
        {
            return;
        }


        // Make a the static one
        if (!a.isStatic)
        {
            (a,b) = (b,a);
        }

        // Resolve the edges of the collision by moving the non-static object out of the static one





    }

    // Spacial Hash Makers:

    private void HashObjects(
        Dictionary<Vector2Int, List<PhysicsObject>> tileToCells,
        Dictionary<PhysicsObject, List<Vector2Int>> objectToHashCells)
    {
        foreach( var entity in AllEntities)
        {
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
        DMVector reach = entity.shape.GetReach();

        DM64 leftMost = (entity.GetComponent<DeterministicTransform>().position.x - reach.x).Floor();
        DM64 rightMost = (entity.GetComponent<DeterministicTransform>().position.x + reach.x).Floor();

        DM64 upMost = (entity.GetComponent<DeterministicTransform>().position.y + reach.y).Floor();
        DM64 downMost = (entity.GetComponent<DeterministicTransform>().position.y - reach.y).Floor();

        for (DM64 x = leftMost; x <= rightMost; x += EditorHashGridSize.x)
        {
            for (DM64 y = downMost; y <= upMost; y += EditorHashGridSize.y)
            {
                output.Add(new Vector2Int(x.to_int(), y.to_int()));
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
            DM64 AL = aTransform.globalPosition.x;
            DM64 AR = aTransform.globalPosition.x + a.size.x;
            DM64 AB = aTransform.globalPosition.y;
            DM64 AT = aTransform.globalPosition.y + a.size.y;

            DM64 BL = bTransform.globalPosition.x;
            DM64 BR = bTransform.globalPosition.x + b.size.x;
            DM64 BB = bTransform.globalPosition.y;
            DM64 BT = bTransform.globalPosition.y + b.size.y;

            return (
                (AL < BR) &&
                (AR > BL) &&
                (AB < BT) &&
                (AT > BB)
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