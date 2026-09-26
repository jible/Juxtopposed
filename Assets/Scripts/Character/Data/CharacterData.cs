using System;

// Every character shares this shape, only the values differ.
// Defaults live on the fields, so a character only lists what it changes.
// Velocities are in units per tick, accelerations in units per tick per tick.
[Serializable]
public class CharacterData
{
    public GroundMovementData Ground = new();
    public AirMovementData Air = new();
    public JumpData Jump = new();
}

[Serializable]
public class GroundMovementData
{
    public DM64 Acceleration = new DM64(1) / 120;
    public DM64 MaxVelocity = new DM64(2) / 60;
    // Fraction of horizontal velocity lost each tick
    public DM64 Friction = new DM64(1) / 10;
}

[Serializable]
public class AirMovementData
{
    public DM64 Acceleration = new DM64(1) / 180;
    public DM64 MaxVelocity = new DM64(2) / 60;
    // Gravity
    public DM64 FallAcceleration = new DM64(1) / 120;
}

[Serializable]
public class JumpData
{
    public DM64 Velocity = new DM64(1) / 10;
}
