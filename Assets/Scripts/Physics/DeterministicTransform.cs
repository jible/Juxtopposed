using UnityEngine;
using System.Collections.Generic;
public class DeterministicTransform : MonoBehaviour
{
    [SerializeField, HideInInspector]
    private bool globalPositionIsDirty = false;

    [SerializeField]
    private DMVector _position;
    public DMVector position
    {
        get
        {
            return _position;
        }
        set
        {
            _position = value;
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
                _position = _globalPosition - parent.globalPosition;
            }
            else
            {
                _position = _globalPosition;
            }


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
