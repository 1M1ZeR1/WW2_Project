using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowCanvasScript : MonoBehaviour
{
    [Header("Спрайт стрелки(Союзник)")]
    [SerializeField] private GameObject arrow_Allies;

    [Header("Спрайт стрелки(Противник)")]
    [SerializeField] private GameObject arrow_Enemys;

    [Header("Главный контроллер")]
    [SerializeField] private GameObject GameControllerObject;
    private GameController gameControllerScript;

    public delegate void UpdateSquadInfo(AbstractSquad squad);
    public event UpdateSquadInfo squadUpdateRequest;

    [SerializeField] private float timer;

    private void Start()
    {
        GameControllerObject.TryGetComponent<GameController>(out gameControllerScript);
    }

    public void CreateArrow(Vector3 startPosition, Vector3 endPosition, float speed,AbstractSquad squad)
    {
        GameObject newArrow;

        if (squad.Side == SideEnum.Allies)
        {
            newArrow = Instantiate(arrow_Allies, transform);
        }
        else
        {
            newArrow = Instantiate(arrow_Enemys, transform);
        }

        RectTransform rectTransform = newArrow.GetComponent<RectTransform>();
        rectTransform.position = (startPosition + endPosition) * 0.5f;

        ArrowScript arrowScript = newArrow.GetComponent<ArrowScript>();

        arrowScript.SetParameters(startPosition, endPosition, speed,squad);
    }
    public void SquadIsReached(GameObject arrow,AbstractSquad squad)
    {
        StartCoroutine(DestroyArrowAfterTime(arrow));
        squadUpdateRequest?.Invoke(squad);
    }
    private IEnumerator DestroyArrowAfterTime(GameObject arrow)
    {
        float otherTimer = timer;

        while (otherTimer >= 0)
        {
            if (PauseScript.CurrentGameState != GameState.Play) { yield return null; }

            yield return new WaitForSeconds(.01f);

            otherTimer -= .01f;
        }
        Destroy(arrow);
    }
}
