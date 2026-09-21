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
    public void Awake()
    {
        // Establish references
        physicsServer = transform.GetComponent<PhysicsServer>();
        tickManager = GetComponent<TickManager>();
        if (physicsServer== null || tickManager == null )
        // characterHolder== null || stageHolder== null)
        {
            Debug.LogError("Manager Not found");
            return;
        }



    }

    public void Start()
    {
        // Every physics object has registered by now, so bucket them by layer
        physicsServer.RegisterPhysicsObjectsByLayer();
    }

    public void Update()
    {
        tickManager.Tick();
    }
}
