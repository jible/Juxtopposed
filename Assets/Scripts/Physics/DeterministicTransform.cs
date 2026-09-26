using UnityEngine;
using System.Collections.Generic;

[ExecuteAlways]
public class DeterministicTransform : MonoBehaviour
{
    [SerializeField, HideInInspector]
    private bool globalPositionIsDirty = false;

    [SerializeField]
    private SerializableProperty<DMVector> serializedPosition = new();
    public DMVector position
    {
        get
        {
            return serializedPosition.Value;
        }
        set
        {
            serializedPosition.Value = value;
            if (!Application.isPlaying)
            {
                UpdateNormalTransformPosition();
            }
            SetDirty();
        }
    }

    public DMVector PositionAtTickIndex(uint tickIndex)
    {
        return serializedPosition.GetDataFromFrame(tickIndex);
    }


    [SerializeField, HideInInspector]
    private DMVector _globalPosition;
    public DMVector globalPosition
    {
        get
        {
            if (globalPositionIsDirty){
                DMVector newGlobalPosition = new();
                DeterministicTransform parent = TryGetParent();
                if (parent != null)
                {
                    newGlobalPosition += parent.globalPosition;
                }
                newGlobalPosition += position;
                _globalPosition = newGlobalPosition;

                globalPositionIsDirty = false;
            }
            return _globalPosition;
        }
        set
        {
            _globalPosition = value;
            // Update the local position to match the global position
            DeterministicTransform parent = TryGetParent();
            if (parent != null)
            {
                serializedPosition.Value = _globalPosition - parent.globalPosition;
            }
            else
            {
                serializedPosition.Value = _globalPosition;
            }
            globalPositionIsDirty = false;

            // Set the children as dirty?
            foreach (var child in getChildren())
            {
                child.SetDirty();
            }
        }
    }

    public void Awake()
    {
        serializedPosition.OnLoaded = SetDirty;
        Register();

    }

    private void Register()
    {
        // Prefab mode has no manager to join
        if (EditorContext.IsInPrefabStage(gameObject)) return;

        // Walk up the tree and register to the deterministic manager
        // If you ever use this variable more than once, serialize it. For now im just doing this
        var manager = GetComponentInParent<DeterministicTransformManager>();
        if (manager == null)
        {
            Debug.LogError("Deterministic Transform does not have Manager ancestor");
            return;
        }
        manager.RegisterTransform(this);
    }

    public void OnValidate()
    {
        // Inspector edits write straight to serializedPosition's backing field and never
        // go through the position setter, so push the transform update from here too.
        UpdateNormalTransformPosition();
        SetDirty();
    }

    public void OnTransformParentChanged()
    {
        globalPositionIsDirty = true;
    }
    public void SetDirty (){
        if (globalPositionIsDirty) return;
        globalPositionIsDirty = true;
        foreach (var child in getChildren())
        {
            child.SetDirty();
        }
    }

// TODO: Maybe cache these objects if the tree does not change (currently to be determinied)
    public DeterministicTransform TryGetParent()
    {
        if (transform.parent == null)
        {
            return null;
        }
        DeterministicTransform parent = null;
        transform.parent.gameObject.TryGetComponent<DeterministicTransform>(out parent);
        return parent;
    }
    public List<DeterministicTransform> getChildren()
    {
        List<DeterministicTransform> children = new();
        foreach (Transform child in transform)
        {

            DeterministicTransform t = null;
            child.gameObject.TryGetComponent<DeterministicTransform>(out t);
            if (t!= null)
            {
                children.Add(t);
            }
        }
        return children;
    }

    public void UpdateNormalTransformPosition()
    {
        Vector2 a = position.ToStandardVector();
        transform.localPosition = new(a.x,a.y,0);
    }
}
