using System.Collections.Generic;
using UnityEngine;

// Draws a sprite character through the SpriteRenderer under it.
// Each animation is its own sheet at Resources/<folder>/<animation>, sliced in Unity's Sprite Editor.
// The sheet's frames are spread evenly over the state's length
public class SpriteCharacterView : MonoBehaviour, ICharacterView
{
    // Used when the state has no authored length, so new art plays before its state is set up
    private const int UnauthoredTicksPerSprite = 6;

    private SpriteRenderer spriteRenderer;
    private string folder;
    // Stands in for any animation with no sheet
    private string defaultAnimation;
    // Loaded on first use. Null entries are animations with no sheet, so they are only looked up once
    private readonly Dictionary<string, Sprite[]> animations = new();

    public void Configure(string folder, string defaultAnimation = null)
    {
        this.folder = folder;
        this.defaultAnimation = defaultAnimation;
        // Sheets from a previous folder don't apply any more
        animations.Clear();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError($"{name} has no SpriteRenderer to draw with");
        }
    }

    public void Show(string animation, int frame, int length, Character.Direction facing)
    {
        if (spriteRenderer == null) return;

        // Art is drawn facing right
        spriteRenderer.flipX = facing == Character.Direction.Left;

        Sprite[] sprites = ResolveSprites(animation);
        // No sheet and no default keeps whatever was showing, so a missing animation is visible but not blank
        if (sprites == null) return;

        int index = length > 0
            ? frame * sprites.Length / length
            : frame / UnauthoredTicksPerSprite % sprites.Length;
        spriteRenderer.sprite = sprites[Mathf.Clamp(index, 0, sprites.Length - 1)];
    }

    // Sprites Show spreads over the state for this animation, or 0 when there is no sheet and no default
    public int SpriteCount(string animation)
    {
        Sprite[] sprites = ResolveSprites(animation);
        return sprites != null ? sprites.Length : 0;
    }

    private Sprite[] ResolveSprites(string animation) => GetSprites(animation) ?? GetSprites(defaultAnimation);

    private Sprite[] GetSprites(string animation)
    {
        if (string.IsNullOrEmpty(animation)) return null;
        if (animations.TryGetValue(animation, out Sprite[] sprites)) return sprites;

        sprites = Resources.LoadAll<Sprite>($"{folder}/{animation}");
        if (sprites.Length == 0)
        {
            Debug.LogWarning($"No sprite sheet at Resources/{folder}/{animation}");
            sprites = null;
        }
        else
        {
            // Load order isn't guaranteed, slices are named <sheet>_<number>
            System.Array.Sort(sprites, (a, b) => SliceNumber(a).CompareTo(SliceNumber(b)));
        }
        animations[animation] = sprites;
        return sprites;
    }

    private static int SliceNumber(Sprite sprite)
    {
        string spriteName = sprite.name;
        int underscore = spriteName.LastIndexOf('_');
        return underscore >= 0 && int.TryParse(spriteName.Substring(underscore + 1), out int number) ? number : 0;
    }
}
