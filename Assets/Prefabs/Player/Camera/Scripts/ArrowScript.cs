using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArrowScript : MonoBehaviour
{
    [SerializeField] private int speedMultiply;

    protected Vector3 _startPosition;
    protected Vector3 _endPosition;

    protected float _speed;

    protected bool _isMove = false;

    protected Image _image;
    protected float fillingField = 0f;

    protected AbstractSquad _squad;

    [SerializeField] private GameObject gameController;
    private GameController gameControllerScript;

    private void Start()
    {
        gameController.TryGetComponent<GameController>(out gameControllerScript);

        //gameControllerScript.oneSecondPassed += OtherTimerController;
    }
    private void Update()
    {
        if (PauseScript.CurrentGameState != GameState.Play) { return; }
        if (_isMove)
        {
            FillArrow();
        }
    }
    private void OtherTimerController()
    {
        if (PauseScript.CurrentGameState != GameState.Play) { return; }
        if(_isMove)
        {
            FillArrow();
        }
    }
    public void SetParameters(Vector3 startPosition, Vector3 endPosition, float speed,AbstractSquad squad)
    {
        _squad = squad;

        _startPosition = startPosition;
        _endPosition = endPosition;

        transform.right = (endPosition - startPosition).normalized;
        transform.Rotate(-90f, 0, 0);

        float newWidth = Mathf.Clamp(Vector3.Distance(startPosition,endPosition),20f,950f);

        RectTransform _transform = GetComponent<RectTransform>();

        Vector2 currentSize = _transform.sizeDelta;
        currentSize.x = newWidth;
        _transform.sizeDelta = currentSize;

        _speed = (speed * speedMultiply) / Vector3.Distance(startPosition,endPosition);

        _image = GetComponent<Image>();

        _isMove = true;
    }
    private void FillArrow()
    {
        _image.fillAmount = fillingField;

        fillingField += _speed  * Time.deltaTime;

        if (fillingField >= 1f) { 
            _isMove = false; 
            transform.parent.GetComponent<ArrowCanvasScript>().SquadIsReached(gameObject,_squad); 
        }
    }
}
