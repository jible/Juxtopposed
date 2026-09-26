using UnityEngine;

// Not in the Editor folder, since runtime components call it
public static class EditorContext
{
    // True while this object is open in prefab editing mode, where no managers exist
    public static bool IsInPrefabStage(GameObject gameObject)
    {
#if UNITY_EDITOR
        return UnityEditor.SceneManagement.PrefabStageUtility.GetPrefabStage(gameObject) != null;
#else
        return false;
#endif
    }
}
