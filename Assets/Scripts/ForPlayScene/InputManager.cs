using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public float DriftThreshold = .1f;

    [Header("Debug")]
    [Tooltip("Hard-codes routing: keyboard = player 1, any other device = player 2. Turn off to use PlayerManager's configured assignments.")]
    [SerializeField] private bool debugDeviceRouting = true;

    public UnregisteredSerializableData<ControllerState>[] Controllers;
    // public Action ButtonEventEventHandler(ControllerState.ButtonTypes Button, int PlayerNumber, bool Pressed);
    private ControllerState[] currentControllerStates;

    
    public void Start()
    {
        PlayerInput playerInput = GetComponent<PlayerInput>();
        playerInput.onActionTriggered += OnActionTriggered;
        Controllers = new UnregisteredSerializableData<ControllerState>[PlayerManager.MaxPlayerCount]; 
        currentControllerStates = new ControllerState[PlayerManager.MaxPlayerCount];
        for (int i =0 ; i <PlayerManager.MaxPlayerCount; i++)
        {
            // For each player slot, create a new serialized data object of controllers
            Controllers[i] = new();
            currentControllerStates[i] =new();
        }

    }
    private static readonly Dictionary<string, ControllerState.ButtonTypes> InputToButtonKey = new Dictionary<string, ControllerState.ButtonTypes>
    {
        {"LightAttack", ControllerState.ButtonTypes.LIGHT},
        {"Jump", ControllerState.ButtonTypes.JUMP},
        {"Block", ControllerState.ButtonTypes.BLOCK},
        {"SpecialAttack", ControllerState.ButtonTypes.SPECIAL},
        {"Grab", ControllerState.ButtonTypes.GRAB},
    };
    public void OnActionTriggered(InputAction.CallbackContext context)
    {
        string actionName = context.action.name;
        bool isPress = !context.canceled;
        int player = GetPlayerIndex(context.control.device);
        if (player < 0) return;

        if (context.action.type == InputActionType.Button)
        {
            // Handle the action
            // Write to the current controller state
            currentControllerStates[player].SetButton(InputToButtonKey[actionName], isPress );
        } else if (context.action.type == InputActionType.Value && context.valueType == typeof(Vector2))
        {
            // Otherwise, it is a stick input
            if (actionName== "Movement")
            {
                currentControllerStates[player].LeftStick.SetFromVector(context.action.ReadValue<Vector2>());
            }
        }
    }

    // Returns -1 if the device has no player assigned
    private int GetPlayerIndex(InputDevice device)
    {
        if (debugDeviceRouting)
        {
            return device is Keyboard ? 0 : 1;
        }
        return PlayerManager.DeviceIDToPlayerNumber.TryGetValue(device.deviceId, out int playerNumber) ? playerNumber : -1;
    }
    

    public void SaveInputs(int tickIndex)
    {
        for (int i = 0; i < Controllers.Length; i++)
        {
            // Write the current snap shot to the value and call save 
            Controllers[i].Value = currentControllerStates[i];
            Controllers[i].Save(tickIndex);
        }
    }

    public void LoadInputs(int TickIndex)
    {
        foreach(var controller in Controllers)
        {
            controller.Load(TickIndex);
        }
    }
    
    // public void GetInputs(int playernumber, int tickIndex)
    // {
    //     return Controllers[playernumber].
    // }
}



