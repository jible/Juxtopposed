// Anything in the simulation that other things need to refer to, like a hit group remembering who it hit.
// Ids are handed out once by the DeterministicWorld when it's built, in collection order, and never change after,
// so they aren't rollback state. Don't assume an id matches a player number
public interface IEntity
{
    byte EntityId { get; set; }
}
