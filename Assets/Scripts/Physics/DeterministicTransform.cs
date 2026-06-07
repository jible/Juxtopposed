using UnityEngine;

public class DeterministicTransform : MonoBehaviour
{
    [SerializeField]
    public DMVector position;
    

    [SerializeField, HideInInspector]
    public DMVector globalPosition;

}
