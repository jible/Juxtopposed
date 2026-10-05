using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

// One box on one frame, as the preview draws it. Display only, so plain Unity types.
// Relative to the character root and facing right
public struct HitboxPreviewData
{
    public BoxType Type;
    public Vector2 Center;
    public Vector2 Size;
    // Inactive boxes still exist on other frames, so hide them rather than removing them
    public bool Active;
}

public class HitboxPreviewManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private Dictionary<int, GameObject> hitboxPreviews = new();
    public GameObject HitboxPreviewPrefab;
    public HitboxEditorManager hitboxEditorManager;
    public HitboxEditorUI hitboxEditorUI;
    public void Start()
    {
        hitboxEditorManager.Changed += onHitboxEditorChanged;
    }

    public void onHitboxEditorChanged()
    {

        hitboxEditorManager.GetHitboxData( hitboxEditorUI.SelectedState, hitboxEditorUI.CurrentFrame, out int[] hitBoxKeys, out HitboxPreviewData[] hitboxData);

        foreach (var key in hitBoxKeys){
            if (hitboxPreviews.ContainsKey(key))
            {
                // update the value if its already rendered

            } else
            {
                
            }
        }

        foreach (var key in hitboxPreviews.Keys)
        {
            if (!hitBoxKeys.Contains(key))
            {
                RemovePreview(key);
            }
        }

    }

    public void AddPreview(int name)
    {
        if (hitboxPreviews.ContainsKey(name))
        {
            Debug.LogWarning($"Hitbox preview with name {name} already exists. Overwriting.");
            return;
        }
        var preview = Instantiate(HitboxPreviewPrefab);
        hitboxPreviews[name] = preview;
    }

    public void RemovePreview (int name)
    {
        if (!hitboxPreviews.ContainsKey(name))
        {
            return;
        }
        var a  = hitboxPreviews[name];
        hitboxPreviews.Remove(name);
        if (a == null)
        {
            return;
        }
        Destroy(a);
    }

    public void RemoveAllPreviews()
    {
        foreach (var key  in hitboxPreviews.Keys)
        {
            var a = hitboxPreviews[key];
            if (a == null)
            {
                continue;
            }
            Destroy(a);
        }
        hitboxPreviews.Clear();
        
    }
}


