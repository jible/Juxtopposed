using System;
using UnityEngine;

public class PlayManager : MonoBehaviour
{
    // This script is in charge of setting up the play scene: it spawns everything the world needs,
    // then builds the deterministic world from it and drives it every frame

    [Serializable]
    private struct DebugPlayerSlot
    {
        public bool Used;
        public CharacterId Character;
    }

    InputManager inputManager;
    // Null until Start, so nothing ticks before every object exists
    DeterministicWorld world;
    [Tooltip("Everything under this is collected into the deterministic world. Defaults to this object")]
    [SerializeField]
    Transform worldRoot;
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
        inputManager = FindAnyObjectByType<InputManager>();
        if (worldRoot == null) worldRoot = transform;
        if (characterHolder == null) characterHolder = GetComponentInChildren<CharacterHolder>();
        if (inputManager == null || characterHolder == null)
        // stageHolder== null)
        {
            Debug.LogError("Manager Not found");
            // Skips Start and Update, so no world is built from a half set up scene
            enabled = false;
            return;
        }

        if (useDebugPlayers)
        {
            PopulateDebugPlayers();
        }
        // Clears rollback data left over from before this scene, so only properties made from here on are saved.
        // SerializableProperty registers when it's constructed, and the characters below are constructed after this
        SerializableDataManager.Reset();
        characterHolder.SpawnCharacters(inputManager);
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
        // Every object has run Awake and been configured by now, so the world can collect them
        world = new DeterministicWorld(worldRoot, inputManager);
    }

    public void Update()
    {
        world?.Update(Time.deltaTime);
    }

}
