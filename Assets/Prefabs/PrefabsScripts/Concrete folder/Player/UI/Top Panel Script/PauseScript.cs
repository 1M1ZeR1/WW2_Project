using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseScript : MonoBehaviour
{
    [Header("Состояние игры")]
    [SerializeField] private GameObject gameStatePanel;
    protected static TextMeshProUGUI gameStateText;

    public static GameState CurrentGameState { get; private set; } = GameState.Pause;

    private void Start()
    {
        gameStatePanel.TryGetComponent(out gameStateText);

        CurrentGameState = GameState.Pause;
    }

    public void OnSpaceButtonClick(InputAction.CallbackContext context)
    {
        if(context.performed)
        {      
            if(CurrentGameState == GameState.Play) { CurrentGameState = GameState.Pause; gameStateText.text = "На паузе"; }
            else { CurrentGameState = GameState.Play; gameStateText.text = "В игре"; }
        }
    }
    public void OnPauseButtonClick()
    {
        if (CurrentGameState == GameState.Play) { CurrentGameState = GameState.Pause; gameStateText.text = "На паузе"; }
        else { CurrentGameState = GameState.Play; gameStateText.text = "В игре"; }
    }
    public static void SetGameState(GameState state)
    {
        CurrentGameState = state;

        if (CurrentGameState == GameState.Play) { gameStateText.text = "В игре"; }
        else { gameStateText.text = "На паузе"; }
    }
}
