using UnityEngine;
using System.Collections.Generic;
public class DeterministicTransform : MonoBehaviour
{
    [SerializeField, HideInInspector]
    private bool globalPositionIsDirty = false;

    [SerializeField]
    private SerializableData<DMVector> serializedPosition = new();
    public DMVector position
    {
        get
        {
            return serializedPosition.Current;
        }
        set
        {
            serializedPosition.Current = value;
            SetDirty();
        }
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
                serializedPosition.Current = _globalPosition - parent.globalPosition;
            }
            else
            {
                serializedPosition.Current = _globalPosition;
            }
            globalPositionIsDirty = false;

            // Set the children as dirty?
            foreach (var child in getChildren())
            {
                child.SetDirty();
            }
        }
    }

    public void OnValidate()
    {
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
}
