// Placeholder platformer character
public static partial class Roster
{
    public static readonly CharacterData Platty = new()
    {
        Ground = { MaxVelocity = new DM64(3) / 60 },
        Jump = { Velocity = new DM64(1) / 8 },
    };
}
