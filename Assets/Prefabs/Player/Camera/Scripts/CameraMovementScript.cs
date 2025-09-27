using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovementScript : MonoBehaviour
{
    private enum DirectionOfMovement
    {
        Left, Right, Up,Down,None,
        LeftDown, RightDown,
        LeftUp, RightUp
    }

    [SerializeField] private int procesents;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float focusSpeed;

    private float _screenWidth;
    private float _screenHeight;

    private List<Vector2> pointsPlayArea = new List<Vector2>();
    private DirectionOfMovement _directionOfMovement;

    protected bool _inSelectOption = false;

    protected Vector3 focusPosition;

    private void Start()
    {
        _screenWidth = Screen.width;
        _screenHeight = Screen.height;

        CalculateAreas();

        _directionOfMovement = DirectionOfMovement.None;
    }
    private void CalculateAreas()
    {
        float startPointX = _screenWidth / (100 / procesents);
        float startPointY = _screenHeight / (100 / procesents);

        pointsPlayArea.Add(new Vector2(startPointX, startPointY));

        float endPointX = _screenWidth - startPointX;
        float endPointY = _screenHeight - startPointY;

        pointsPlayArea.Add(new Vector2(startPointX,endPointY));
        pointsPlayArea.Add(new Vector2(endPointX, startPointY));
        pointsPlayArea.Add(new Vector2(endPointX, endPointY));
    }

    public static bool blocked = false;
    public static void BlockMovement() { blocked = true; }
    public static void UnBlockMovement() { blocked = false; }

    private void Update()
    {
        if (!CheckArea(Input.mousePosition) && !blocked)
        {
            MoveCamera();
        }
    }
    private bool CheckArea(Vector2 mousePosition)
    {
        if(mousePosition.x > pointsPlayArea[0].x && mousePosition.x < pointsPlayArea[3].x)
        {
            if(mousePosition.y > pointsPlayArea[3].y) { _directionOfMovement = DirectionOfMovement.Up; return false; }
            if(mousePosition.y < pointsPlayArea[0].y) { _directionOfMovement = DirectionOfMovement.Down; return false; }
        }
        else
        {
            if(mousePosition.x < pointsPlayArea[0].x && mousePosition.y < pointsPlayArea[0].y) { _directionOfMovement = DirectionOfMovement.LeftDown;return false; }
            if(mousePosition.x < pointsPlayArea[0].x && mousePosition.y > pointsPlayArea[3].y) { _directionOfMovement = DirectionOfMovement.LeftUp;return false; }
            if(mousePosition.x > pointsPlayArea[3].x && mousePosition.y < pointsPlayArea[0].y) { _directionOfMovement = DirectionOfMovement.RightDown;return false; }
            if(mousePosition.x > pointsPlayArea[3].x && mousePosition.y > pointsPlayArea[3].y) { _directionOfMovement = DirectionOfMovement.RightUp;return false; }
            if(mousePosition.x < pointsPlayArea[0].x) { _directionOfMovement= DirectionOfMovement.Left; return false;}
            if(mousePosition.x > pointsPlayArea[3].x) { _directionOfMovement = DirectionOfMovement.Right; return false;}

        }
        
        return true;
    }
    private void MoveCamera()
    {
        if(_directionOfMovement == DirectionOfMovement.Left)
        {
            transform.position = new Vector3(transform.position.x - movementSpeed * Time.deltaTime, transform.position.y,transform.position.z);
        }
        if(_directionOfMovement == DirectionOfMovement.Right)
        {
            transform.position = new Vector3(transform.position.x + movementSpeed * Time.deltaTime, transform.position.y, transform.position.z);
        }
        if(_directionOfMovement == DirectionOfMovement.Up)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + movementSpeed * Time.deltaTime);
        }
        if(_directionOfMovement == DirectionOfMovement.Down)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z - movementSpeed * Time.deltaTime);
        }
        if (_directionOfMovement == DirectionOfMovement.LeftDown)
        {
            transform.position = new Vector3(transform.position.x-movementSpeed * Time.deltaTime, transform.position.y, transform.position.z - movementSpeed * Time.deltaTime);
        }
        if (_directionOfMovement == DirectionOfMovement.RightDown)
        {
            transform.position = new Vector3(transform.position.x+movementSpeed * Time.deltaTime, transform.position.y, transform.position.z - movementSpeed * Time.deltaTime);
        }
        if (_directionOfMovement == DirectionOfMovement.LeftUp)
        {
            transform.position = new Vector3(transform.position.x-movementSpeed * Time.deltaTime, transform.position.y, transform.position.z + movementSpeed * Time.deltaTime);
        }
        if (_directionOfMovement == DirectionOfMovement.RightUp)
        {
            transform.position = new Vector3(transform.position.x + movementSpeed * Time.deltaTime, transform.position.y, transform.position.z + movementSpeed * Time.deltaTime);
        }
    }
    public void GetPositionToFocus(GameObject cell)
    {
        focusPosition = new Vector3(cell.transform.position.x,transform.position.y,cell.transform.position.z);

        //_inFocus = true;
    }
    public void FocusToCell()
    {
        transform.position = Vector3.MoveTowards(transform.position, focusPosition, focusSpeed);

        Vector2 currentPosition = new Vector2(transform.position.x, transform.position.z);
        Vector2 finishPosition = new Vector2(focusPosition.x, focusPosition.z);

        if (Vector2.Distance(currentPosition, finishPosition) <= 1f)
        {
            //_inFocus = false;
            focusPosition = Vector3.zero;
        }
    }
}
