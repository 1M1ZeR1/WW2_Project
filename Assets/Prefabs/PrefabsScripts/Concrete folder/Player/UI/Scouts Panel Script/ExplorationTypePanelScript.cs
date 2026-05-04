using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExplorationTypePanelScript : MonoBehaviour
{
    [SerializeField] private GameObject buttonStartExplorationObject;
    [SerializeField] private GameObject buttonUpdateExplorationObject;

    protected Button _buttonStartExploration;
    protected Button _buttonUpdateExploration;

    public delegate void PlayerChosed(int playerChosed);
    public event PlayerChosed playerChosed;

    private void Awake()
    {
        buttonStartExplorationObject.TryGetComponent(out _buttonStartExploration);
        buttonUpdateExplorationObject.TryGetComponent(out _buttonUpdateExploration);
    }

    public void OpenExplorationTypePanel(bool canUpdate)
    {
        PauseScript.SetGameState(GameState.Pause);

        gameObject.SetActive(true);

        if(!canUpdate) { _buttonUpdateExploration.interactable = false; }
        else { _buttonUpdateExploration.interactable = true; }
    }

    public void PlayerChoise(int playerChosed)
    {
        PauseScript.SetGameState(GameState.Play);

        this.playerChosed.Invoke(playerChosed);

        gameObject.SetActive(false);
    }
}
