using System.Collections.Generic;
using UnityEngine;

// Resolves hits between characters each tick, from the boxes in their CharacterDefinitions.
// Separate from the physics server: boxes are frame data, not physics objects, and only share its overlap math.
// Ticked right after physics, so it sees every character's final position for the tick.
//
// Everything runs in player number order, then box order, so every machine resolves the same hits.
// All hits are found before any are applied, so two characters hitting each other on the same tick both land.
// Holds no state between ticks. What each hit group has already hit lives on the attacker, so it rolls back
public class HitboxManager : MonoBehaviour
{
    // Hit memory is one bit per hit group and target, see Character.HitMemory
    public static int MaxHitGroups => 64 / PlayerManager.MaxPlayerCount;

    private struct Hit
    {
        public Character Attacker;
        public Character Target;
        public HitGroupDefinition HitGroup;
    }

    // Indexed by player number, null for empty slots
    private Character[] characters;
    // Only lives for the duration of a tick
    private readonly List<Hit> hits = new();

    // Called by the play manager once characters are spawned
    public void Configure(Character[] characters)
    {
        this.characters = characters;
    }

    public void Tick()
    {
        if (characters == null) return;

        // A new state starts with nothing hit. TicksInState is only 0 here on the tick a state was entered
        foreach (var character in characters)
        {
            if (character != null && character.StateMachine.TicksInState == 0)
            {
                character.HitMemory.Value = 0;
            }
        }

        hits.Clear();
        foreach (var attacker in characters)
        {
            if (attacker != null) FindHits(attacker);
        }

        foreach (var hit in hits)
        {
            hit.Target.ReceiveHit(hit.Attacker, hit.HitGroup);
        }
    }

    private void FindHits(Character attacker)
    {
        StateDefinition state = attacker.Definition.GetState(attacker.StateMachine.CurrentStateId);
        if (state == null) return;
        int frame = state.FrameAt(attacker.StateMachine.TicksInState);
        DMVector root = attacker.DeterministicTransform.globalPosition;

        foreach (var box in state.Boxes)
        {
            // A hitbox needs a hit group to know what it deals
            if (box.Type != BoxType.Hitbox || box.HitGroupIndex < 0 || !box.IsActive(frame)) continue;
            if (box.HitGroupIndex >= MaxHitGroups)
            {
                Debug.LogWarning($"{attacker.name} {state.Name} box {box.Id} is in hit group {box.HitGroupIndex}, past the {MaxHitGroups} supported");
                continue;
            }

            DMVector center = root + box.CenterAt(frame, attacker.direction);
            DMVector size = box.SizeAt(frame);
            foreach (var target in characters)
            {
                if (target == null || target == attacker) continue;
                // One hit per group per target, however many of the group's boxes overlap it
                ulong bit = HitBit(box.HitGroupIndex, target.PlayerIndex);
                if ((attacker.HitMemory.Value & bit) != 0) continue;
                if (!TouchesHurtbox(target, center, size)) continue;

                attacker.HitMemory.Value |= bit;
                hits.Add(new Hit { Attacker = attacker, Target = target, HitGroup = state.HitGroups[box.HitGroupIndex] });
            }
        }
    }

    private static bool TouchesHurtbox(Character target, DMVector center, DMVector size)
    {
        StateDefinition state = target.Definition.GetState(target.StateMachine.CurrentStateId);
        if (state == null) return false;
        int frame = state.FrameAt(target.StateMachine.TicksInState);
        DMVector root = target.DeterministicTransform.globalPosition;

        foreach (var box in state.Boxes)
        {
            if (box.Type != BoxType.Hurtbox || !box.IsActive(frame)) continue;
            if (Overlap.Boxes(center, size, root + box.CenterAt(frame, target.direction), box.SizeAt(frame)))
            {
                return true;
            }
        }
        return false;
    }

    private static ulong HitBit(int hitGroupIndex, int targetPlayerIndex)
    {
        return 1UL << (hitGroupIndex * PlayerManager.MaxPlayerCount + targetPlayerIndex);
    }
}
