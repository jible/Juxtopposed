// Draws a character. Display only: it is told what to show after the ticks run, and never
// keeps its own clock, so a rollback is fixed up by the next Show with no extra work.
// One implementation per VisualKind
public interface ICharacterView
{
    // frame is a tick within the animation, already looped or held by the state.
    // length is the state's length in ticks, or 0 when the state has no authored data
    void Show(string animation, int frame, int length, Character.Direction facing);
}
