using UnityEngine;

public class CharacterHolder : MonoBehaviour
{
    // This script is in charge of instancing the player(s) and holding it in the play scene

    [SerializeField]
    private Character characterPrefab;
    [SerializeField]
    DMVector[] spawnPositions = new DMVector[PlayerManager.MaxPlayerCount];

    // Indexed by player number, null for empty slots
    public readonly Character[] Characters = new Character[PlayerManager.MaxPlayerCount];

    // Spawns in player number order so the tick order is deterministic
    public void SpawnCharacters(InputManager inputManager)
    {
        for (int playerNumber = 0; playerNumber < PlayerManager.MaxPlayerCount; playerNumber++)
        {
            PlayerProfile profile = PlayerManager.players[playerNumber];
            if (profile == null) continue;

            // Parented here so its deterministic transform finds the manager above it
            Character character = Instantiate(characterPrefab, transform);
            character.name = $"Player {playerNumber + 1} ({profile.Character})";
            DMVector spawnPosition = playerNumber < spawnPositions.Length ? spawnPositions[playerNumber] : DMVector.zero;
            character.Configure(profile.Character, playerNumber, inputManager, spawnPosition);
            Characters[playerNumber] = character;
        }
    }
}
