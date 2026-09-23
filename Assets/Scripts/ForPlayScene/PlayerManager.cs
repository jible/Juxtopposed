using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.iOS;

public static class PlayerManager
{
    /*
    The player manager is in charge of deciding how many players there are, what character that player has queued as,
    and what controller that player controls through. 
    */
    public static readonly int MaxPlayerCount = 4;
    public static int PlayerCount = 0;
    // This is a debug setting- probably should not be a feature in the game.
    public static readonly bool AllowPlayerFindingDuringPlay = true;
    public static readonly PlayerProfile[] players = new PlayerProfile[MaxPlayerCount];
    public static readonly Dictionary<int, int> DeviceIDToPlayerNumber = new();
    static PlayerManager()
    {
        // When the player manager is initialized, 
        // Add listener for any input
        // InputSystem.onAnyButtonPress.Call(OnPlayerInput);
    }
    public static int RegisterController(int deviceID)
    {
        // If the controller is already used, return
        if (DeviceIDToPlayerNumber.ContainsKey(deviceID)) return -1;
        // If we are at max player count we can't register more controllers, return -1
        if (PlayerCount == MaxPlayerCount) return -1;

        // Register the controller to an open player number
        for (int playerNumber = 0; playerNumber < MaxPlayerCount; playerNumber++)
        {
            if (players[playerNumber] == null)
            {
                // This player slot is empty, Register to this slot
                players[playerNumber] = new PlayerProfile(playerNumber, deviceID);
                PlayerCount += 1;
                return playerNumber;
            }
        }
        return -1;
    }
    

}

public class PlayerProfile
{
    int PlayerNumber;
    // Add Control Scheme at some point
    // input device
    int DeviceID;

    public PlayerProfile(int playerNumber, int deviceID)
    {
        PlayerNumber = playerNumber;
        DeviceID = deviceID;
    }

}
