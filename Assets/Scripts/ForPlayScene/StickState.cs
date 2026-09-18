using System;
using System.Collections.Generic;
using UnityEngine;


public struct StickState
{
    public enum Axis
    {
        X,
        Y
    }

    public static readonly int MAX_STICK_AXIS_VALUE = 7;
    const int Buckets= 7;
    private static readonly byte XMagnitudeMask = 0x70; 
    private static readonly byte XSignMask = 0x80;
    private static readonly byte YMagnitudeMask = 0x07;
    private static readonly byte YSignMask = 0x08;

    // How Data is stored:
    // [X-Sign, 2nd bit, 1st bit, 0th bit, Y-Sign, 2nd bit, 1st bit, 0th bit]
    byte Data;

    public StickState(byte _data = 0)
    {
        Data = _data;
    }

    public void SetFromVector(Vector2 V)
    {
        int XSign = V.x < 0 ? 1: 0;
        int YSign = V.y < 0 ? 1: 0;
        int XBits = (int)Math.Round(Math.Abs(V.x) * Buckets);
        int YBits = (int)Math.Round(Math.Abs(V.y) * Buckets);
        Data = (byte)(
            XSign << 7 |
            XBits << 4 | 
            YSign << 3|
            YBits 
            );
    }

    public void SetAxis(Axis axis, float Value)
    {
        Value = Math.Clamp(Value, -1f , 1f);
        if (axis == Axis.X)
        {
            Data = (byte)(  (Data & YMagnitudeMask) | (Data & YSignMask) | ((byte)Math.Round(Value * Buckets) << 4));
        } else
        {
            Data = (byte)( (Data & XMagnitudeMask) | (Data & XSignMask) |((byte)Math.Round(Value* Buckets)));
        }
    }

    public DMVector ToVector()
    {
         
        int UX = (Data & XMagnitudeMask) >> 4;
        int XSign = (Data & XSignMask) == 0 ? 1 : -1;
        int UY = Data & YMagnitudeMask;
        int YSign = (Data & YSignMask) == 0 ? 1 : -1;
        DM64 X = new DM64(UX) * XSign;
        DM64 Y = new DM64(UY) * YSign;
        return new DMVector( X,  Y);
    }

    // Returns input vector, who's magnitude is squished into the range [0,1] (Not normalized)
    public DMVector ToRangedVector()
    {
         
        DMVector LargeVector =  ToVector();
        return LargeVector/7;
    }

    public byte GetEncoded()
    {
        return Data;
    }

    public void Decoded(byte Coded)
    {
        Data = Coded;
    }



    public override bool Equals(object obj)
    {
        //
        // See the full list of guidelines at
        //   http://go.microsoft.com/fwlink/?LinkID=85237
        // and also the guidance for operator== at
        //   http://go.microsoft.com/fwlink/?LinkId=85238
        //
        
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }
        
        // TODO: write your implementation of Equals() here
        return Data == ((StickState)obj).Data;
    }
    
    // override object.GetHashCode
    public override int GetHashCode()
    {
        // TODO: write your implementation of GetHashCode() here
        return Data;
    }

    public static bool operator ==(StickState left, StickState right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(StickState left, StickState right)
    {
        return !(left == right);
    }
}
