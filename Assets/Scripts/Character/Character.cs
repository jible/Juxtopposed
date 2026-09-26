using Unity.VisualScripting;
using UnityEngine;

// Owns a character's state machine and holds every reference its states need
[RequireComponent(typeof(DeterministicTransform), typeof(PhysicsObject))]
public class Character : MonoBehaviour, ITickable
{
    [HideInInspector,DoNotSerialize]
    public InputManager inputManager;
    public int PlayerIndex { get; private set; }
    public CharacterData Data { get; private set; }

    public PhysicsObject PhysicsObject { get; private set; }
    public DeterministicTransform DeterministicTransform { get; private set; }
    public CharacterStateMachine StateMachine { get; private set; }

    public ControllerState Controller => inputManager.Controllers[PlayerIndex].Value;

    [HideInInspector,DoNotSerialize]
    public CharacterMovement characterMovement;

    // Inspector display only, the real state lives in the state machine
    [SerializeField]
    private CharacterStateId currentState;

    public void Awake()
    {
        PhysicsObject = GetComponent<PhysicsObject>();
        DeterministicTransform = GetComponent<DeterministicTransform>();
        characterMovement = GetComponent<CharacterMovement>();
    }

    // Called by the character holder right after instancing
    public void Configure(CharacterId characterId, int playerIndex, InputManager inputManager, DMVector spawnPosition)
    {
        Data = Roster.Get(characterId);
        PlayerIndex = playerIndex;
        this.inputManager = inputManager;
        DeterministicTransform.position = spawnPosition;
        StateMachine = new CharacterStateMachine(this, CharacterStateId.Idle);
        characterMovement.Config(new(playerIndex, inputManager));

    }

    public void Tick()
    {
        StateMachine.Tick();
        currentState = StateMachine.CurrentStateId;
    }

    public DMVector GetStick(ControllerState.StickTypes stick)
    {
        return inputManager.Controllers[PlayerIndex].Value.LeftStick.ToVector();
    }
}
