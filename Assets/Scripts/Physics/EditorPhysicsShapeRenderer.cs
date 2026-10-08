using UnityEngine;

// Draws the physics shapes under the world root while not playing.
// Finds them fresh every draw, so there's no registry to keep up to date in edit mode.
// In play mode the world's RuntimePhysicsShapeRenderer draws instead
public class EditorPhysicsShapeRenderer : MonoBehaviour
{
    [SerializeField]
    private Transform worldRoot;
    public float renderZAxis = 0;
    public float renderThickness = .5f;

    public void OnDrawGizmos()
    {
        if (Application.isPlaying || worldRoot == null) return;

        foreach (var obj in worldRoot.GetComponentsInChildren<PhysicsObject>())
        {
            if (obj.shape == null || !obj.renderShape) continue;

            if (obj.shape is Square square)
            {
                Gizmos.color = obj.renderColor;
                Gizmos.DrawCube(obj.deterministicTransform.globalPosition.ToVector3(renderZAxis), square.size.ToVector3(renderThickness));
            }
        }
    }
}
