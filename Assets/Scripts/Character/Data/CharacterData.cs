using System;

// Every character shares this shape, only the values differ.
// Defaults live on the fields, so a character only lists what it changes.
// Velocities are in units per second, accelerations in units per second per second.
// Only the physics server converts velocity to units per tick.
[Serializable]
public class CharacterData
{
    public GroundMovementData Ground = new();
    public AirMovementData Air = new();
    public JumpData Jump = new();
    public VisualData Visuals = new();
    // Ticks a state lasts when it has no authored data, so states that end on their animation still end
    public int DefaultStateLength = 30;
}

public enum VisualKind
{
    // One sheet per animation, sliced into sprites in frame order
    Sprite,
    // Not supported yet
    Model,
}

// How the character is drawn. Display only, nothing here affects the game.
// The art lives with the character's other files, see CharacterFiles
[Serializable]
public class VisualData
{
    public VisualKind Kind = VisualKind.Sprite;
    // Played for any animation the character has no art for yet. Null shows nothing new
    public string DefaultAnimation;
}

[Serializable]
public class GroundMovementData
{
    public DM64 Acceleration = new DM64(30);
    public DM64 MaxVelocity = new DM64(2);
    // Fraction of horizontal velocity lost per second, applied a tick's share at a time
    public DM64 Friction = new DM64(6);
    // Acceleration is multiplied by this when pushing against the current velocity
    public DM64 TurnaroundMultiplier = new DM64(5);
}

[Serializable]
public class AirMovementData
{
    public DM64 Acceleration = new DM64(20);
    public DM64 MaxVelocity = new DM64(2);
    // Gravity
    public DM64 FallAcceleration = new DM64(10);
    public DM64 MaxFallVelocity = new DM64(2);
    // Fraction of horizontal velocity lost per second, applied a tick's share at a time
    public DM64 Friction = new DM64(2);
    public DM64 TurnaroundMultiplier = new DM64(5);
}

[Serializable]
public class JumpData
{
    public DM64 Velocity = new DM64(50);
    public int AirJumps = 2;
    // Horizontal push in the stick's direction on an air jump, capped by the air max velocity
    public DM64 AirHorizontalImpulse = new DM64(2);
}
