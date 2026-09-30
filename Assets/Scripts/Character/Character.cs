using Unity.VisualScripting;
using UnityEngine;

// Owns a character's state machine and holds every reference its states need
[RequireComponent(typeof(DeterministicTransform), typeof(PhysicsObject))]
public class Character : MonoBehaviour, ITickable
{
    [HideInInspector,DoNotSerialize]
    public InputManager inputManager;
    public InputManager.InputGetter inputGetter;
    public int PlayerIndex { get; private set; }
    public CharacterData Data { get; private set; }

    public PhysicsObject PhysicsObject { get; private set; }
    public DeterministicTransform DeterministicTransform { get; private set; }
    public CharacterStateMachine StateMachine { get; private set; }
    public enum Direction
    {
        Left,Right
    }
    // Set when the character is instanced
    private SerializableProperty<Direction> _direction = new();
    public Direction direction // Each state directly handles manipulating direction
    {
        get
        {
            return _direction.Value;
        }
        set
        {
            _direction.Value = value;
        }
    }
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
        _direction.OnLoaded += onDirectionUpdated;
    }

    // Called by the character holder right after instancing
    public void Configure(CharacterId characterId, int playerIndex, InputManager inputManager, DMVector spawnPosition, Direction direction = Direction.Left)
    {
        Data = Roster.Get(characterId);
        PlayerIndex = playerIndex;
        this.inputManager = inputManager;
        DeterministicTransform.position = spawnPosition;
        StateMachine = new CharacterStateMachine(this, CharacterStateId.Idle);
        inputGetter = new(playerIndex, inputManager);
        characterMovement.Config(inputGetter);
        this.direction = direction;

    }

    public void Tick()
    {
        StateMachine.Tick();
        characterMovement.Tick();
        currentState = StateMachine.CurrentStateId;
    }

    public void onDirectionUpdated()
    {
        // Update the sprite/model
        

    }
}
