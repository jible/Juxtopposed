using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class StateDisplay : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField]
    public Character character;
    private CharacterStateMachine sm;
    private TextMeshPro text;
    void Start()
    {
        sm = character.StateMachine;
        sm.StateChanged += OnStateChanged;
        text = GetComponent<TextMeshPro>();

    }

    public void OnStateChanged(CharacterStateId id)
    {
        text.text = id.ToString();
        return;
    }



}
