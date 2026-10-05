using System.Collections.Generic;
using UnityEngine;

public class HitboxPreviewManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private Dictionary<int, GameObject> hitboxPreviews = new();
    public GameObject HitboxPreviewPrefab;
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


