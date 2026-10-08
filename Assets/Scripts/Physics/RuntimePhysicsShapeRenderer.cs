using UnityEngine;

// Draws the shapes of the physics objects the world collected, as meshes rather than gizmos,
// so it needs no Unity callback and also shows in the game view. Owned by the DeterministicWorld,
// which calls Draw every frame
public class RuntimePhysicsShapeRenderer
{
    public float RenderZAxis = 0;
    public float RenderThickness = .5f;

    private readonly PhysicsObject[] objects;

    public RuntimePhysicsShapeRenderer(PhysicsObject[] objects)
    {
        this.objects = objects;
    }

    public void Draw()
    {
        foreach (var obj in objects)
        {
            if (obj.shape == null || !obj.renderShape) continue;

            if (obj.shape is Square square)
            {
                DebugBoxDrawer.Draw(
                    obj.deterministicTransform.globalPosition.ToVector3(RenderZAxis),
                    square.size.ToVector3(RenderThickness),
                    obj.renderColor);
            }
        }
    }
}
