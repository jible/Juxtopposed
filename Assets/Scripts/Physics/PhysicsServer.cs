using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem.Composites;
using UnityEngine.UI;
using UnityEngine.UIElements;

[DefaultExecutionOrder(-10000)]
[ExecuteAlways]
public class PhysicsServer : MonoBehaviour
{
    public DM64 Epsilon = new(.0001f);
    private List<PhysicsObject>[] BucketByLayer = new List<PhysicsObject>[32];
    private List<PhysicsObject>[] StaticBucketByLayer = new List<PhysicsObject>[32];

    [SerializeField]

    private PhysicsObjectRegistry physicsObjectRegistry;
    private enum Axis
    {
        X,Y
    }
    private static readonly Axis[] axes = {Axis.X, Axis.Y};
    // Awake is not called again after a script reload (domain reload), but statics are wiped.

    public void Awake()
    {
        // Runs before any PhysicsObject/DeterministicTransform Awake (DefaultExecutionOrder),
        // so this clear always happens before those objects register themselves.
        physicsObjectRegistry = GetComponent<PhysicsObjectRegistry>();
        SerializableDataManager.Reset();

        for (int i = 0;  i < BucketByLayer.Length; i++)
        {
            BucketByLayer[i] = new();
            StaticBucketByLayer[i] = new();
        }
    }

    public void RegisterPhysicsObjectsByLayer()
    {
        // We already have every physics object, but we wait to call this until each of them have their properties configured 
        // such that they will not need to change during runtime- ie the player registers their hitboxes,
        //  but needs to set their layer 
        for (int i = 0; i < BucketByLayer.Length; i++)
        {
            BucketByLayer[i].Clear();
            StaticBucketByLayer[i].Clear();
        }
        // Triggers can overlap anything, but only static colliders are ever collided against
        AddToLayerBuckets(BucketByLayer, physicsObjectRegistry.All);
        AddToLayerBuckets(StaticBucketByLayer, physicsObjectRegistry.StaticColliders);
    }

    private static void AddToLayerBuckets(List<PhysicsObject>[] buckets, List<PhysicsObject> objects)
    {
        foreach (PhysicsObject obj in objects)
        {
            for (int i = 0; i < buckets.Length; i++)
            {
                if ((obj.layer & (1 << i)) != 0)
                {
                    buckets[i].Add(obj);
                }
            }
        }
    }

    // Should be pretty much the last thing that happens every frame
    public void Tick()
    {
        collisionEvents.Clear();
        triggerEvents.Clear();

        foreach (Axis axis in axes)
        {
            ApplyVelocity(axis);
            ResolveAllCollisions(axis);
        }

        DetectTriggers();

        DispatchSignals();
    }
    
    
    //A number that constantly increases each time an object queries its other objects
    // The only use for this is object A iterating over the objects it masks and 
    // making sure it doesn't visit them twice if it masks them on 2 different layers.
    private int Stamp = 0;

    // Signals are recorded while the passes run and dispatched together afterwards,
    // so listeners never run while positions are still being resolved
    private struct CollisionEvent
    {
        public PhysicsObject Body;
        public PhysicsObject Solid;
        // Which axis the collision happened on, and which side of it Body was on,
        // as the sign of Body's velocity relative to Solid's along that axis
        public DMVector Side;
    }
    private struct TriggerEvent
    {
        public PhysicsObject Trigger;
        public PhysicsObject Target;
    }
    private readonly List<CollisionEvent> collisionEvents = new();
    private readonly List<TriggerEvent> triggerEvents = new();


    private void ApplyVelocity(Axis axis)
    {   
        foreach (var physicsObject in physicsObjectRegistry.All)
            {
                var x = axis == Axis.X?physicsObject.velocity.Value.x: new DM64(0);
                var y = axis == Axis.Y?physicsObject.velocity.Value.y: new DM64(0);
                physicsObject.deterministicTransform.position += new DMVector( x, y );
            }
    }

    private void ResolveAllCollisions(Axis axis)
    {
        // Only active dynamic bodies query, and only against static colliders
        foreach (var A in physicsObjectRegistry.DynamicBodies)
        {
            if (!A.isActive) continue;
            int stamp = ++Stamp;
            for (int layerIndex = 0; layerIndex < StaticBucketByLayer.Length; layerIndex++)
            {
                // Skip layers this object doesn't mask
                if ((A.mask & (1 << layerIndex)) == 0) continue;
                foreach (var B in StaticBucketByLayer[layerIndex])
                {
                    if (!B.isActive || B.visitStamp == stamp) continue;
                    B.visitStamp = stamp;
                    ResolveCollision(A, B, axis);
                }
            }
        }
    }

    private void DetectTriggers()
    {
        // Only active triggers query, against anything on the layers they mask
        foreach (var A in physicsObjectRegistry.Triggers)
        {
            if (!A.isActive) continue;
            int stamp = ++Stamp;
            for (int layerIndex = 0; layerIndex < BucketByLayer.Length; layerIndex++)
            {
                if ((A.mask & (1 << layerIndex)) == 0) continue;
                foreach (var B in BucketByLayer[layerIndex])
                {
                    if (A == B || !B.isActive || B.visitStamp == stamp) continue;
                    B.visitStamp = stamp;
                    checkTrigger(A, B);
                }
            }
        }
    }

    // Only records for a. The reverse direction is recorded when b's own query reaches a, so a mutual pair isn't recorded twice
    public void checkTrigger(PhysicsObject a, PhysicsObject b)
    {
        if (OverlapChecker.CheckOverlap(a, b))
        {
            triggerEvents.Add(new TriggerEvent { Trigger = a, Target = b });
        }
    }

    private void ResolveCollision(PhysicsObject a, PhysicsObject b, Axis axis)
    {
        // a is a dynamic body that masks b's layer, and b is a static collider
        if (OverlapChecker.CheckOverlap(a, b) && HandleCollision(a, b, axis, out DMVector side))
        {
            collisionEvents.Add(new CollisionEvent { Body = a, Solid = b, Side = side });
        }
    }

    // The side of the collision: zero on the axis the collision didn't happen on, and on the
    // collision axis, the sign of Body's velocity relative to Solid's (which side Body approached from)
    private static DMVector GetCollisionSide(PhysicsObject a, PhysicsObject b, Axis axis)
    {
        if (axis == Axis.X)
        {
            return new DMVector((a.velocity.Value.x - b.velocity.Value.x).Sign(), new DM64(0));
        }
        return new DMVector(new DM64(0), (a.velocity.Value.y - b.velocity.Value.y).Sign());
    }

    /// <summary>
    /// Handles the case such that a is a trigger and it masks b and overlaps b - Emits signal that a has been triggered
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    public void DispatchTrigger(PhysicsObject a, PhysicsObject b)
    {
        // Already checked for overlap- just handle it
        a.OnOverlap(b);
    }

    // Emits everything recorded this tick, collisions first and then triggers, in the order they were detected
    private void DispatchSignals()
    {
        for (int i = 0; i < collisionEvents.Count; i++)
        {
            DispatchCollision(collisionEvents[i].Body, collisionEvents[i].Solid, collisionEvents[i].Side);
        }
        for (int i = 0; i < triggerEvents.Count; i++)
        {
            DispatchTrigger(triggerEvents[i].Trigger, triggerEvents[i].Target);
        }
    }

    /// <summary>
    /// Emits the signal that body was stopped by solid
    /// </summary>
    public void DispatchCollision(PhysicsObject body, PhysicsObject solid, DMVector side)
    {
        body.OnCollide(solid, side);
    }

    // Which way to push a out of b along one axis: back against a's motion, or, if a isn't moving on that axis
    // (a platform moved into it, or it spawned overlapping), away from b's center
    private static DM64 PushDirection(DM64 velocity, DM64 aPosition, DM64 bPosition)
    {
        DM64 direction = -velocity.Sign();
        if (direction == 0) direction = (aPosition - bPosition).Sign();
        if (direction == 0) direction = new DM64(1);
        return direction;
    }

    private bool HandleCollision(PhysicsObject a, PhysicsObject b, Axis axis, out DMVector side)
    {
        side = DMVector.zero;
        // Resolve the edges of the collision by moving the non-static object out of the static one
        // In this case, A is dynamic, b is static
        // Thus a will be repelled from b's surface
        if ( !(a.shape is Square) || !(b.shape is Square) ){
            return false; // Just don't do anything if they aren't squares for now
        }


        Square aSquare = (Square)a.shape;
        Square bSquare = (Square)b.shape;

        DMVector currentAPosition = a.deterministicTransform.position;

        // Assuming the 2 are colliding because it has already been checked,
        // A is the non-static one, so its gets moved 
        // It get slid to the slide opposite to the magnitude of the velocity 
        DM64 newX = currentAPosition.x;
        DM64 newY = currentAPosition.y;
        if (axis == Axis.X){
            newX = b.deterministicTransform.position.x + 
            (PushDirection(a.velocity.Value.x, currentAPosition.x, b.deterministicTransform.position.x) * (Epsilon + aSquare.size.x/2 + bSquare.size.x/2));
        }else if (axis == Axis.Y){
            newY = b.deterministicTransform.position.y + 
            (PushDirection(a.velocity.Value.y, currentAPosition.y, b.deterministicTransform.position.y) * (Epsilon + aSquare.size.y/2 + bSquare.size.y/2));
        }

        a.deterministicTransform.position = new(newX, newY);

        side = GetCollisionSide(a, b, axis);
        return true;
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