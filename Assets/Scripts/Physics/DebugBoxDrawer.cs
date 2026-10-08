using UnityEngine;
using UnityEngine.Rendering;

// Draws see through boxes as meshes, for the runtime debug renderers.
// A mesh drawn this way only lasts the frame it was submitted in, so callers draw every frame
public static class DebugBoxDrawer
{
    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private static readonly MaterialPropertyBlock properties = new();
    private static Mesh cube;
    // One for the whole session, so it's never leaked or rebuilt
    private static Material material;
    private static RenderParams renderParams;

    public static void Draw(Vector3 center, Vector3 size, Color color)
    {
        if (material == null) Initialize();
        // The block is copied when the draw is submitted, so reusing it for every box is safe
        properties.SetColor(ColorId, color);
        Graphics.RenderMesh(renderParams, cube, 0, Matrix4x4.TRS(center, Quaternion.identity, size));
    }

    // Unity's own debug line shader: unlit, with a color property and configurable blending for the colors' alpha
    private static void Initialize()
    {
        cube = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
        material = new Material(Shader.Find("Hidden/Internal-Colored")) { hideFlags = HideFlags.HideAndDontSave };
        material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
        material.SetInt("_Cull", (int)CullMode.Off);
        material.SetInt("_ZWrite", 0);
        material.renderQueue = (int)RenderQueue.Transparent;
        renderParams = new RenderParams(material) { matProps = properties };
    }
}
