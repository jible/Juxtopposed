using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public float DriftThreshold = .1f;

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
        InputControl InputMaker = context.control;
        // If the player system was fully set up, the player manager would be able to map this to a player number

        if (context.action.type == InputActionType.Button)
        {
            // Handle the action
            // Write to the current controller state
            // Just accessing player 1 for now
            currentControllerStates[0].SetButton(InputToButtonKey[actionName], isPress );
        } else if (context.action.type == InputActionType.Value && context.valueType == typeof(Vector2))
        {
            // Otherwise, it is a stick input
            if (actionName== "Movement")
            {
                currentControllerStates[0].LeftStick.SetFromVector(context.action.ReadValue<Vector2>());
            }
        }
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



