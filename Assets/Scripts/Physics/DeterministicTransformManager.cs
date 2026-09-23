using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[ExecuteAlways]
public class DeterministicTransformManager: MonoBehaviour
{
    [DoNotSerialize]
    List<DeterministicTransform>  allTransforms = new();
    HashSet<DeterministicTransform> seenTransform = new();

    public void RegisterTransform(DeterministicTransform dt)
    {
        // Maybe over kill to have a set and list but whatever
        if (seenTransform.Contains(dt)) return;
        seenTransform.Add(dt);
        allTransforms.Add(dt);
    }

    public void UpdateTransforms()
    {
        foreach (var dt in allTransforms)
        {
            dt.UpdateNormalTransformPosition();
        }
    }
}
