using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellPanelHelper : MonoBehaviour
{
    protected GameObject _cell;
    protected AbstractSquad _squad;

    public void SetCell(GameObject cell)
    {
        _cell = cell;
    }
    public GameObject GetCell() { return _cell; }

    public void SetSquad(AbstractSquad squad)
    {
        _squad = squad;
    }
    public AbstractSquad GetSquad() { return _squad;}
}
