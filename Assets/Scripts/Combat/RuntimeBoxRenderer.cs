using System.Collections.Generic;
using UnityEngine;

// Draws the hit and hurt boxes the HitboxManager gathered on the last tick.
// Owned by the DeterministicWorld, which calls Draw every frame
public class RuntimeBoxRenderer
{
    public bool Visible = true;
    // In front of the physics shapes, which sit around z = 0
    public float RenderZAxis = -.3f;
    public float RenderThickness = .1f;
    public Color HitboxColor = new(1f, .2f, .2f, .5f);
    public Color HurtboxColor = new(.2f, .5f, 1f, .4f);

    private readonly HitboxManager hitboxManager;

    public RuntimeBoxRenderer(HitboxManager hitboxManager)
    {
        this.hitboxManager = hitboxManager;
    }

    public void Draw()
    {
        if (!Visible) return;
        // Hurtboxes first, so overlapping hitboxes draw over them
        DrawAll(hitboxManager.Hurtboxes, HurtboxColor);
        DrawAll(hitboxManager.Hitboxes, HitboxColor);
    }

    private void DrawAll(IReadOnlyList<Box> boxes, Color color)
    {
        for (int i = 0; i < boxes.Count; i++)
        {
            DebugBoxDrawer.Draw(boxes[i].Center.ToVector3(RenderZAxis), boxes[i].Size.ToVector3(RenderThickness), color);
        }
    }
}
