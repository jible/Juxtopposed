using UnityEngine;

[ExecuteAlways]
public class DeterministicTransformManager: MonoBehaviour
{
    DeterministicTransformManager Instance;

    public void Start()
    {
        Instance = this;
    }

    // Main or the physics server wil call this method
    // It needs the right timing with other operations
    public void propogateGlobalChanges(Transform parent, DMVector parentGlobalPosition)
    {
        foreach(Transform child in parent)
        {
            if (child.TryGetComponent<DeterministicTransform>(out var deterministicTransform))
            {
                deterministicTransform.globalPosition = parentGlobalPosition + deterministicTransform.position;
                propogateGlobalChanges(child, deterministicTransform.globalPosition);
            }
        }
    }
}
