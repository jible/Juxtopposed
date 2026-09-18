using Unity.VisualScripting;
using UnityEngine;

public class PlayManager : MonoBehaviour
{
    // This script is in charge of syncing all of the other scripts,
    // ensuring they are configured in the correct order and waits to call updates
    // until they are all configured

    PhysicsServer physicsServer;
    TickManager tickManager;
    [SerializeField]
    StageHolder stageHolder;
    [SerializeField]
    CharacterHolder characterHolder;
    public void Ready()
    {
        // Establish references
        physicsServer = GetComponent<PhysicsServer>();
        tickManager = GetComponent<TickManager>();
        if (! physicsServer || !tickManager || !characterHolder || !stageHolder)
        {
            Debug.LogError("Manager Not found");
            return;
        }
    }
}
