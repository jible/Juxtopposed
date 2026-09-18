using System;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public float DriftThreshold = .1f;

    public UnregisteredSerializableData<ControllerState>[] Controllers;
    // public Action ButtonEventEventHandler(ControllerState.ButtonTypes Button, int PlayerNumber, bool Pressed);


    
    public void Ready()
    {
        PlayerInput playerInput = GetComponent<PlayerInput>();
        playerInput.onActionTriggered += OnActionTriggered;
        UnregisteredSerializableData<ControllerState>[] controllers = new UnregisteredSerializableData<ControllerState>[PlayerManager.MaxPlayerCount]; 
        for (int i =0 ; i <PlayerManager.MaxPlayerCount; i++)
        {
            // For each player slot, create a new serialized data object of controllers
            controllers[i] = new();
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

        if (context.action.type == InputActionType.Value)
        {
            // Handle the action
            // Write to the current controller state
            // Just accessing player 1 for now
            Controllers[0].Value.SetButton(InputToButtonKey[actionName], isPress );
        } else if (context.action.type == InputActionType.Value && context.valueType == typeof(Vector2))
        {
            // Otherwise, it is a stick input
            if (actionName== "Movement")
            {
                Controllers[0].Value.LeftStick.SetFromVector(context.action.ReadValue<Vector2>());
            }
        }
    }
    

    public void SaveInputs(int tickIndex)
    {
        foreach (var controller in Controllers)
        {
            controller.Save(tickIndex);
        }
    }

    public void LoadInputs(int TickIndex)
    {
        foreach(var controller in Controllers)
        {
            controller.Load(TickIndex);
        }
    }
    
}



