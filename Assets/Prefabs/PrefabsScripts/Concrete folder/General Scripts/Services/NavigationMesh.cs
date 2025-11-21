using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NavigationMesh
{
    private BiDictionary<GameObject, Vector2> cellToPosition_BiDictionary = new();

    public void AddCellToNavigationSystem(GameObject cell) { cellToPosition_BiDictionary.Add(cell, new Vector2(cell.transform.position.x,cell.transform.position.z)); }

    public GameObject GetCellByCoordinates(Vector2 coordinates) { return cellToPosition_BiDictionary.GetBySecond(coordinates); }
    public Vector2 GetCoordinatesByCell(GameObject cell) { return cellToPosition_BiDictionary.GetByFirst(cell); }

    public List<GameObject> GetAllCells()=>cellToPosition_BiDictionary.GetAllFirstKeys();
    public List<Vector2> GetAllCoordinates() => cellToPosition_BiDictionary.GetAllSecondKeys();
}

public class BiDictionary<T1,T2>
{
    private Dictionary<T1, T2> forward = new();
    private Dictionary<T2, T1> backward = new();

    public void Add(T1 key1, T2 key2)
    {
        forward[key1] = key2;
        backward[key2] = key1;
    }

    public T2 GetByFirst(T1 key) => forward[key];
    public T1 GetBySecond(T2 key) => backward[key];

    public List<T1> GetAllFirstKeys()=>forward.Keys.ToList();
    public List<T2> GetAllSecondKeys()=>backward.Keys.ToList();
}
