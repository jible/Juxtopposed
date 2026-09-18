using System;
using UnityEngine;

public struct ControllerState
{
    public enum ButtonTypes
    {
        SPECIAL,
        LIGHT,
        GRAB,
        JUMP,
        BLOCK,
        COUNT,
    }

    public enum StickTypes
    {
        LEFT,
        RIGHT,
        COUNT,
    }
    static readonly int ButtonCount = (int)ButtonTypes.COUNT;
    static readonly int ButtonBytes = (ButtonCount + 7) / 8;
    public static readonly int EncodedSize = (int)(ButtonBytes + StickTypes.COUNT);
    private uint ButtonStates;


    public StickState LeftStick;
    public StickState RightStick;

    public ControllerState(byte buttonState = 0)
    {
        LeftStick = new();
        RightStick = new();
        ButtonStates = buttonState;
    }

    public void SetButton(ButtonTypes Button, bool Value)
    {
        if (Value)
        {
            uint Mask = (uint)(1 << (int)Button);
            ButtonStates |= Mask;

        }
        else
        {
            uint Mask = ~(1u<< (int)Button);
            ButtonStates &= Mask;
        }
    }

    public bool GetButton (ButtonTypes Button)
    {
        uint Mask = (uint)(1 << (int)Button);
        return (ButtonStates & Mask) != 0;
    }

    public byte[] GetEncoded()
    {
        var Output = new byte[ButtonBytes + 2];
        int ByteIndex = 0;
        for ( ;  ByteIndex < ButtonBytes; ByteIndex++)
        {
            int shift = ByteIndex * 8;
            Output[ByteIndex] = (byte)(ButtonStates  >> shift); 

        }
        Output[ByteIndex] = LeftStick.GetEncoded();
        ByteIndex += 1;
        Output[ByteIndex] = RightStick.GetEncoded();

        
        return Output;
    }

    public static ControllerState FromEncoded(byte[] Encoded)
    {
        ControllerState output = new();
        int EncodedSize = (int)(ButtonBytes + StickTypes.COUNT);
        if (Encoded.Length != EncodedSize)
        {
            Debug.LogError("Received Input Data of incorrect size");
        }

        int Walker = 0;
        for (; Walker < ButtonBytes;  Walker ++)
        {
            int shift = 8 * Walker;
            output.ButtonStates |= (uint)Encoded [Walker] << shift;
        }
        var LeftStick = new StickState();
        LeftStick.Decoded(Encoded[Walker]);
        output.LeftStick = LeftStick;

        Walker++;
        var RightStick = new StickState();
        RightStick.Decoded(Encoded[Walker]);
        output.RightStick = RightStick;
        return output;
    }

    public override bool Equals(object obj)
    {
        if (obj is not ControllerState Other)return false;

        if (ButtonStates != Other.ButtonStates)return false;

        if (LeftStick != Other.LeftStick || RightStick != Other.RightStick) return false;
        
        return true;
    }
    
    // override object.GetHashCode
    public override int GetHashCode()
    {
        int output = 0;
        output = HashCode.Combine(LeftStick.GetHashCode(), output );
        output = HashCode.Combine(RightStick.GetHashCode(), output );

        output = HashCode.Combine(ButtonStates, output );
        
        return output;
    }

    public static bool operator ==(ControllerState left, ControllerState right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ControllerState left, ControllerState right)
    {
        return !left.Equals(right);
    }

    // Coppies properties from other state into this state
    public void CopyFromState( ControllerState OtherState)
    {
        ButtonStates = OtherState.ButtonStates;
        LeftStick = OtherState.LeftStick;
        RightStick = OtherState.RightStick;
    }
}
