using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class InputManager : MonoBehaviour
{
    public float DriftThreshold = .1f;


    public UnregisteredSerializableData<ControllerState>[] Controllers;
    // public Action ButtonEventEventHandler(ControllerState.ButtonTypes Button, int PlayerNumber, bool Pressed);
    private ControllerState[] currentControllerStates;
    
    public void Start()
    {
        // If there are no players registered and you can't register during runtime, go back to the player select

        if (!PlayerManager.AllowPlayerFindingDuringPlay && PlayerManager.PlayerCount == 0)
        {
            SceneManager.LoadScene("LevelSelect");
        }

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
        if (player < 0)
        {
            // If it's allowed, just register the player
            if (!PlayerManager.AllowPlayerFindingDuringPlay || PlayerManager.PlayerCount == PlayerManager.MaxPlayerCount)
            {
                return;
            }
            player = PlayerManager.RegisterController(context.control.device.deviceId);
        }
        

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
        return PlayerManager.DeviceIDToPlayerNumber.TryGetValue(device.deviceId, out int playerNumber) ? playerNumber : -1;
    }
    

    public void SaveInputs(uint tickIndex)
    {
        for (int i = 0; i < Controllers.Length; i++)
        {
            // Write the current snap shot to the value and call save 
            Controllers[i].Value = currentControllerStates[i];
            Controllers[i].Save(tickIndex);
        }
    }

    public void LoadInputs(uint TickIndex)
    {
        foreach(var controller in Controllers)
        {
            controller.Load(TickIndex);
        }
    }

    public struct InputGetter
    {
        public int PlayerNumber;
        private InputManager inputManager;
        public InputGetter(int pn, InputManager _inputManager)
        {
            PlayerNumber = pn;
            inputManager =_inputManager;
        }

        private ControllerState GetControllerState()
        {
            return inputManager.Controllers[PlayerNumber].Value;
        }

        public DMVector LeftStick(){
            return GetControllerState().LeftStick.ToVector();
        }
        public DMVector RightStick(){
            return GetControllerState().LeftStick.ToVector();
        }
        public bool isDown(ControllerState.ButtonTypes b){ return GetControllerState().GetButton(b);}
        public bool justPressed(ControllerState.ButtonTypes b){ return GetControllerState().GetButton(b);}
        public bool justReleased(ControllerState.ButtonTypes b){ return GetControllerState().GetButton(b);}
    }
}





