using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// The deterministic simulation, owned by the play manager.
// Everything in it is instanced before it's built, then collected in one walk of the world root.
// Nothing joins or leaves after that, so there's no registering, only the arrays built here
public class DeterministicWorld
{
    public readonly TickManager TickManager;
    public readonly PhysicsServer PhysicsServer;
    public readonly HitboxManager HitboxManager;
    public readonly RuntimePhysicsShapeRenderer ShapeRenderer;
    public readonly RuntimeBoxRenderer BoxRenderer;
    // Indexed by entity id
    public readonly IEntity[] Entities;

    // Ticked in collection order, which is the hierarchy order under the root
    private readonly ITickable[] tickables;
    private readonly DeterministicTransform[] transforms;

    public DeterministicWorld(Transform root, InputManager inputManager)
    {
        var collectedTickables = new List<ITickable>();
        var physicsObjects = new List<PhysicsObject>();
        var collectedTransforms = new List<DeterministicTransform>();
        var characters = new List<Character>();
        var entities = new List<IEntity>();

        // Breadth first, components in component order, so every machine collects in the same order.
        // Inactive objects, and everything under them, aren't part of the world
        Queue<Transform> queue = new();
        queue.Enqueue(root);
        while (queue.Count > 0)
        {
            Transform current = queue.Dequeue();
            if (!current.gameObject.activeInHierarchy) continue;

            collectedTickables.AddRange(current.GetComponents<ITickable>());
            physicsObjects.AddRange(current.GetComponents<PhysicsObject>());
            collectedTransforms.AddRange(current.GetComponents<DeterministicTransform>());
            characters.AddRange(current.GetComponents<Character>());
            entities.AddRange(current.GetComponents<IEntity>());

            foreach (Transform child in current)
            {
                queue.Enqueue(child);
            }
        }

        tickables = collectedTickables.ToArray();
        Entities = AssignEntityIds(entities);
        transforms = collectedTransforms.ToArray();

        PhysicsServer = new PhysicsServer(physicsObjects);
        // Hits resolve in player number order, whatever order the characters sit in the hierarchy
        HitboxManager = new HitboxManager(characters.OrderBy(character => character.PlayerIndex).ToArray());
        ShapeRenderer = new RuntimePhysicsShapeRenderer(physicsObjects.ToArray());
        BoxRenderer = new RuntimeBoxRenderer(HitboxManager);
        TickManager = new TickManager(inputManager, Step);
    }

    // Ids are positions in collection order, so they match on every machine
    private static IEntity[] AssignEntityIds(List<IEntity> entities)
    {
        // Hit groups remember who they hit with one bit per entity
        if (entities.Count > HitGroup.MaxEntities)
        {
            Debug.LogError($"The world has {entities.Count} entities, past the {HitGroup.MaxEntities} hit groups can track");
        }
        for (int i = 0; i < entities.Count; i++)
        {
            entities[i].EntityId = (byte)i;
        }
        return entities.ToArray();
    }

    // Runs as many ticks as the elapsed time calls for, then shows the result
    public void Update(float deltaTime)
    {
        if (TickManager.Advance(deltaTime) > 0) SyncTransforms();
        // Every frame, ticked or not, since the shapes only stay drawn for the frame they're submitted in
        ShapeRenderer.Draw();
        BoxRenderer.Draw();
    }

    // One tick of the simulation. Rollback saving and input loading happen around it in the tick manager
    private void Step()
    {
        foreach (var tickable in tickables)
        {
            tickable.Tick();
        }
        PhysicsServer.Tick();
        // After physics, so hits are checked at final positions
        HitboxManager.Tick();
    }

    // Pushes deterministic positions to the unity transforms. Display only
    private void SyncTransforms()
    {
        foreach (var dt in transforms)
        {
            dt.UpdateNormalTransformPosition();
        }
    }
}
