using System;
using Unity.VisualScripting;
using UnityEngine;

public class PlayManager : MonoBehaviour
{
    // This script is in charge of syncing all of the other scripts,
    // ensuring they are configured in the correct order and waits to call updates
    // until they are all configured

    [Serializable]
    private struct DebugPlayerSlot
    {
        public bool Used;
        public CharacterId Character;
    }

    PhysicsServer physicsServer;
    TickManager tickManager;
    InputManager inputManager;
    [SerializeField]
    StageHolder stageHolder;
    [SerializeField]
    CharacterHolder characterHolder;

    [Tooltip("Replace the player manager's players with the slots below. Turn off once player data comes from the menus.")]
    [SerializeField]
    bool useDebugPlayers = true;
    [SerializeField]
    DebugPlayerSlot[] debugPlayers = new DebugPlayerSlot[PlayerManager.MaxPlayerCount];

    public void Awake()
    {
        // Establish references
        physicsServer = transform.GetComponent<PhysicsServer>();
        tickManager = GetComponent<TickManager>();
        inputManager = FindAnyObjectByType<InputManager>();
        if (characterHolder == null) characterHolder = GetComponentInChildren<CharacterHolder>();
        if (physicsServer== null || tickManager == null || inputManager == null || characterHolder == null)
        // stageHolder== null)
        {
            Debug.LogError("Manager Not found");
            return;
        }

        if (useDebugPlayers)
        {
            PopulateDebugPlayers();
        }
        characterHolder.SpawnCharacters(inputManager);
        // Characters are tickable, so collect after they exist
        tickManager.CollectTickables();
    }

    private void PopulateDebugPlayers()
    {
        PlayerManager.Clear();
        for (int playerNumber = 0; playerNumber < Math.Min(debugPlayers.Length, PlayerManager.MaxPlayerCount); playerNumber++)
        {
            if (debugPlayers[playerNumber].Used)
            {
                PlayerManager.AddPlayer(playerNumber, debugPlayers[playerNumber].Character);
            }
        }
    }

    public void Start()
    {
        // Every physics object has registered by now, so bucket them by layer
        physicsServer.RegisterPhysicsObjectsByLayer();
        tickManager.Ready = true;
    }

}
