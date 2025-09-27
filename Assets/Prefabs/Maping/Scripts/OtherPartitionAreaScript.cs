//using System.Collections.Generic;
//using UnityEngine;
//using SharpVoronoiLib;
//using System.Linq;
//using UnityEngine.UI;
//using System.Collections;

//public class VoronoiDiagramScript : MonoBehaviour
//{
//    [Header("Главный контроллер")]
//    [SerializeField] private GameObject gameControllerObject;
//    private GameController gameControllerScript;

//    [Header("Кол-во ячеек вороного")]
//    [SerializeField] private int numberOfPoints;
//    [SerializeField] private Material cellMaterial;

//    [Header("Ячейка вороного")]
//    [SerializeField]private GameObject voronoiCellSpawner;

//    [Header("Размер области создания")]
//    [SerializeField] private int areaSize;

//    [Header("Генератор карты")]
//    [SerializeField] private GameObject mapGenerator;
//    private LandscapeCreater landscapeCreater;

//    [Header("Алгоритм A*")]
//    [SerializeField] private GameObject algorithmTaker;
//    private AAlgorithm aAlgorithm;

//    protected Dictionary<VoronoiSite, List<VoronoiSite>> _voronoiSitesHeighboresHelper = new Dictionary<VoronoiSite, List<VoronoiSite>>();
//    protected Dictionary<VoronoiSite, GameObject> _siteToGameObjectDictionary = new Dictionary<VoronoiSite, GameObject>();
//    protected Dictionary<VoronoiSite, float> _hightes = new Dictionary<VoronoiSite, float>();

//    private void Start()
//    {
//        gameControllerObject.TryGetComponent(out gameControllerScript);

//        mapGenerator.TryGetComponent(out landscapeCreater);
//        algorithmTaker.TryGetComponent(out aAlgorithm);

//        GenerateVoronoi(GenerateRandomPoints());

//        PostMapping();

//        ProceduralGeneration();
//    }

//    private List<Vector2> GenerateRandomPoints()
//    {
//        //места спавна врагов 
//        List<Vector2> points = new List<Vector2>()
//        {
//            new Vector2(950,950),
//            //new Vector2(950,925),
//            //new Vector2(925,950),
//        };

//        for (int i = 0; i < numberOfPoints; i++)
//        {
//            float x = Random.Range(10f, 991f);
//            float y = Random.Range(10f, 991f);
//            points.Add(new Vector2(x, y));
//        }

//        return points;
//    }
//    private void GenerateVoronoi(List<Vector2> points)
//    {
//        List<VoronoiSite> voronoiSites = new List<VoronoiSite>()
//        {
//        };

//        foreach (var point in points)
//        {
//            voronoiSites.Add(new VoronoiSite(point.x, point.y));
//        }

//        List<VoronoiEdge> edges = VoronoiPlane.TessellateOnce(voronoiSites, 0, 0, areaSize, areaSize, BorderEdgeGeneration.MakeBorderEdges);
        
//        var siteToEdges = GroupEdgesBySites(edges);

//        CreateNeighboresDictionary(edges);

//        foreach(var site in siteToEdges.Keys)
//        {
//            List<Vector2> uniquePoints = GetUniquePoints(siteToEdges[site]);

//            List<Vector2> sortedPoints = SortPoints(uniquePoints);

//            CreateVoronoiCell(sortedPoints, new Vector2((float)site.X,(float)site.Y),site);
            
//        }
//    }
//    private void CreateNeighboresDictionary(List<VoronoiEdge> edges)
//    {
//        foreach(var edge in edges)
//        {
//            if(edge.Right != null && edge.Left != null)
//            {
//                if (!_voronoiSitesHeighboresHelper.ContainsKey(edge.Left))
//                {
//                    _voronoiSitesHeighboresHelper.Add(edge.Left,new List<VoronoiSite>());
//                }
//                _voronoiSitesHeighboresHelper[edge.Left].Add(edge.Right);

//                if (!_voronoiSitesHeighboresHelper.ContainsKey(edge.Right))
//                {
//                    _voronoiSitesHeighboresHelper.Add(edge.Right, new List<VoronoiSite>());
//                }
//                _voronoiSitesHeighboresHelper[edge.Right].Add(edge.Left);
//            }
//        }
//    }
//    private Dictionary<VoronoiSite,List<VoronoiEdge>> GroupEdgesBySites(List<VoronoiEdge> edges)
//    {
//        Dictionary<VoronoiSite, List<VoronoiEdge>> siteToEdges = new Dictionary<VoronoiSite, List<VoronoiEdge>>();

//        foreach(var edge in edges)
//        {
//            if (edge.Left != null)
//            {
//                if (!siteToEdges.ContainsKey(edge.Left))
//                {
//                    siteToEdges[edge.Left] = new List<VoronoiEdge>();
//                }
//                siteToEdges[edge.Left].Add(edge);
//            }
//            if (edge.Right != null)
//            {
//                if (!siteToEdges.ContainsKey(edge.Right))
//                {
//                    siteToEdges[edge.Right] = new List<VoronoiEdge>();
//                }
//                siteToEdges[edge.Right].Add(edge);
//            }
//        }
//        return siteToEdges;
//    }
//    private List<Vector2> GetUniquePoints(List<VoronoiEdge> edges)
//    {
//        HashSet<Vector2> uniquePoints = new HashSet<Vector2>();

//        foreach(var edge in edges)
//        {
//            uniquePoints.Add(new Vector2((float)edge.Start.X, (float)edge.Start.Y));
//            uniquePoints.Add(new Vector2((float)edge.End.X, (float)edge.End.Y));
//        }

//        return uniquePoints.ToList<Vector2>();
//    }
//    private List<Vector2> SortPoints(List<Vector2> points)
//    {
//        Vector2 center = new Vector2(
//            points.Average(k =>k.x),
//            points.Average(k =>k.y)
//            );

//        return points.OrderBy(p => Mathf.Atan2(p.y - center.y, p.x - center.x)).ToList();
//    }
//   private void CreateVoronoiCell(List<Vector2> sortedPoints, Vector2 positionSite, VoronoiSite site)
//   {
//        if (sortedPoints.Count < 3) return;

//        GameObject cell = Instantiate(voronoiCellSpawner);
//        cell.transform.position = new Vector3(positionSite.x,3f,positionSite.y);
//        MeshFilter meshFilter = cell.AddComponent<MeshFilter>();
//        MeshRenderer meshRenderer = cell.AddComponent<MeshRenderer>();
//        meshRenderer.material = cellMaterial;

//        Mesh mesh = new Mesh();

//        Vector3[] topVertices = sortedPoints.Select(p => new Vector3(p.x, 0, p.y)).ToArray();

//        Vector3[] vertices = new Vector3[topVertices.Length];

//        for(int i = 0; i < vertices.Length; i++)
//        {
//            vertices[i] = cell.transform.InverseTransformPoint(topVertices[i]);
//        }

//        List<int> triangles = new List<int>();
//        for (int i = 1; i < sortedPoints.Count - 1; i++)
//        {
//            triangles.Add(0);
//            triangles.Add(i);
//            triangles.Add(i + 1);
//        }
//        mesh.vertices = vertices;
//        mesh.triangles = triangles.ToArray();

//        InvertNormals(mesh);

//        meshFilter.mesh = mesh;
//        cell.AddComponent<MeshCollider>().sharedMesh = mesh;

//        cell.transform.SetParent(transform,false);
//        cell.gameObject.tag = "Interactable Cell";

//        _siteToGameObjectDictionary.Add(site, cell);

//        СonfigureCellScripts(cell);

//        if (positionSite == new Vector2(950, 950))
//        {
//            StartCoroutine(TestAreaColor(cell));
//        }
//        if (positionSite == new Vector2(925, 950))
//        {
//            StartCoroutine(TestAreaColor(cell));
//        }
//        if (positionSite == new Vector2(950, 925))
//        {
//            StartCoroutine(TestAreaColor(cell));
//        }

//        gameControllerScript.AddCellToGlobalList(cell);
//    }
//    private void InvertNormals(Mesh mesh)
//    {
//        Vector3[] normals = mesh.normals;

//        for (int i = 0; i < normals.Length; i++)
//        {
//            normals[i] = -normals[i];
//        }
//        mesh.normals = normals;

//        int[] triangles = mesh.triangles;
//        for (int i = 0; i < triangles.Length; i += 3)
//        {
//            int temp = triangles[i];
//            triangles[i] = triangles[i + 2];
//            triangles[i + 2] = temp;
//        }
//        mesh.triangles = triangles;

//        mesh.RecalculateNormals();
//        mesh.RecalculateBounds();
//    }

//    //private void PostMapping()
//    //{
//    //    foreach(var site in _siteToGameObjectDictionary)
//    //    {
//    //        CellAreaController cellAreaController = site.Value.GetComponent<CellAreaController>();

//    //        foreach(var cellObject in _voronoiSitesHeighboresHelper[site.Key])
//    //        {
//    //            cellAreaController.AddNeighborToList(_siteToGameObjectDictionary[cellObject]);
//    //        }
//    //    }
//    //}
//    //private void СonfigureCellScripts(GameObject cell)
//    //{
//    //    CellTypeScript cellTypeScript = cell.GetComponent<CellTypeScript>();

//    //    if (Random.Range(0, 2) == 0) 
//    //    {
//    //        cellTypeScript.SetTypeCell(CellTypes_enum.Plain);
//    //    }
//    //    else { cellTypeScript.SetTypeCell(CellTypes_enum.Forest);}
//    //}
//    //private IEnumerator TestAreaColor(GameObject cell)
//    //{
//    //    yield return new WaitForSeconds(5);

//    //    cell.GetComponent<EnemysSpawner>().enabled = true;
//    //    cell.GetComponent<CellAreaController>().RequestToControlCell(SideEnum.Enemys);
//    //}
//    //private void ProceduralGeneration()
//    //{
//    //    foreach (var site in _siteToGameObjectDictionary)
//    //    {
//    //        float height = Mathf.PerlinNoise((float)site.Key.X * .0001f, (float)site.Key.Y * .0001f);

//    //        _hightes.Add(site.Key, height);
//    //    }

//    //    Dictionary<GameObject, CellTypeScript> cellToType = new Dictionary<GameObject, CellTypeScript>();
//    //    Dictionary<GameObject, CellAreaController> cellToArea = new Dictionary<GameObject, CellAreaController>();

//    //    foreach(var site in _siteToGameObjectDictionary)
//    //    {
//    //        cellToType.Add(site.Value,site.Value.GetComponent<CellTypeScript>());
//    //        cellToArea.Add(site.Value, site.Value.GetComponent<CellAreaController>());

//    //        cellToType[site.Value].SetHeight(_hightes[site.Key]);
//    //    }

//    //    landscapeCreater.CreateLandScape(cellToType, cellToArea);
//    //    aAlgorithm.SetDictionary(cellToArea);
//    //}
//}
