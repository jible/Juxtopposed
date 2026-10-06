using System.Collections.Generic;
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

// Draws the selected state's boxes on the current frame, one prefab instance per box id.
// This object's transform is the character root, so previews are placed relative to it
public class HitboxPreviewManager : MonoBehaviour
{
    // Unselected boxes are drawn fainter, so the one being edited stands out
    private const float UnselectedAlpha = 0.35f;
    private const float SelectedAlpha = 0.7f;
    // The selected box is still drawn on frames it's inactive, so a new box can be placed before it's turned on
    private const float InactiveAlpha = 0.12f;

    private Dictionary<int, SpriteRenderer> hitboxPreviews = new();
    public GameObject HitboxPreviewPrefab;
    public HitboxEditorManager hitboxEditorManager;
    public HitboxEditorUI hitboxEditorUI;

    public Color HurtboxColor = new Color(1f, 0.85f, 0.1f);
    public Color HitboxColor = new Color(1f, 0.15f, 0.15f);
    public Color PushboxColor = new Color(0.2f, 0.5f, 1f);

    // The UI refreshes after every edit, frame change and selection change, so that covers everything the preview shows
    public void OnEnable()
    {
        hitboxEditorUI.Refreshed += onHitboxEditorChanged;
    }

    public void OnDisable()
    {
        hitboxEditorUI.Refreshed -= onHitboxEditorChanged;
    }

    public void onHitboxEditorChanged()
    {
        hitboxEditorManager.GetHitboxData(hitboxEditorUI.SelectedState, hitboxEditorUI.CurrentFrame, out int[] hitBoxKeys, out HitboxPreviewData[] hitboxData);

        var shown = new HashSet<int>(hitBoxKeys);
        for (int i = 0; i < hitBoxKeys.Length; i++)
        {
            int key = hitBoxKeys[i];
            if (!hitboxPreviews.TryGetValue(key, out SpriteRenderer preview) || preview == null)
            {
                preview = AddPreview(key);
            }
            UpdatePreview(preview, hitboxData[i], key == hitboxEditorUI.SelectedBoxId);
        }

        // Copied, since removing while iterating the keys throws
        foreach (var key in new List<int>(hitboxPreviews.Keys))
        {
            if (!shown.Contains(key))
            {
                RemovePreview(key);
            }
        }
    }

    public SpriteRenderer AddPreview(int name)
    {
        RemovePreview(name);
        var preview = Instantiate(HitboxPreviewPrefab, transform).GetComponent<SpriteRenderer>();
        preview.name = $"Hitbox Preview {name}";
        hitboxPreviews[name] = preview;
        return preview;
    }

    private void UpdatePreview(SpriteRenderer preview, HitboxPreviewData data, bool selected)
    {
        bool shown = data.Active || selected;
        preview.gameObject.SetActive(shown);
        if (!shown) return;

        preview.transform.localPosition = data.Center;
        // Scaled so the sprite covers exactly the box, whatever size the sprite is
        Vector2 spriteSize = preview.sprite.bounds.size;
        preview.transform.localScale = new Vector3(data.Size.x / spriteSize.x, data.Size.y / spriteSize.y, 1);

        Color color = data.Type switch
        {
            BoxType.Hitbox => HitboxColor,
            BoxType.Pushbox => PushboxColor,
            _ => HurtboxColor,
        };
        color.a = !data.Active ? InactiveAlpha : selected ? SelectedAlpha : UnselectedAlpha;
        preview.color = color;
    }

    public void RemovePreview(int name)
    {
        if (!hitboxPreviews.TryGetValue(name, out SpriteRenderer preview))
        {
            return;
        }
        hitboxPreviews.Remove(name);
        if (preview != null)
        {
            Destroy(preview.gameObject);
        }
    }

    public void RemoveAllPreviews()
    {
        foreach (var preview in hitboxPreviews.Values)
        {
            if (preview != null)
            {
                Destroy(preview.gameObject);
            }
        }
        hitboxPreviews.Clear();
    }
}
