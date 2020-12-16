using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class PathFinding : MonoBehaviour
{
    public const int MAP_WIDTH = 30;
    public const int MAP_HEIGHT = 20;
    public const int POINT_START = 8;
    public const int POINT_END = 9;
    public const int POINT_WALL = 1;
    public const int POINT_EMPTY = -1;

    [SerializeField] private GameObject _prefabSearch = default;
    [SerializeField] private GameObject _prefabPath = default;
    [SerializeField] private GameObject _prefabObstacle = default;
    [SerializeField] private GameObject _prefabGround = default;
    [SerializeField] private GameObject _prefabPoint = default;
    [SerializeField] private Material _pathMaterial = default;
    [SerializeField] private Material _searchMaterial = default;
    [SerializeField] private SearchMethod _searchMethod = SearchMethod.DFS;

    private int[,] _originMap;
    private GameObject[,] _originBlocksMap;
    private GameObject[,] _searchingBlocksMap;
    private MapPosition _startPoint;
    private MapPosition _endPoint;
    private GameObject _startPointGameObject;
    private GameObject _endPointGameObject;
    private FindingState _currentState;
    private IPathFindingMethod _currentMethod;
    private bool _isFound;
    private Dictionary<SearchMethod, IPathFindingMethod> _methodDict = new Dictionary<SearchMethod, IPathFindingMethod>();

    public int[,] OriginMap => _originMap;
    public GameObject[,] OriginBlocksMap => _originBlocksMap;
    public GameObject[,] SearchingBlocksMap => _searchingBlocksMap;
    public MapPosition StartPoint => _startPoint;
    public MapPosition EndPoint => _endPoint;
    public Material SearchMaterial => _searchMaterial;
    public Material PathMaterial => _pathMaterial;

    public void SetIsFound(bool value) => _isFound = value;


    // Start is called before the first frame update
    void Start()
    {
        Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        FiniteStateMachine();
    }

    private void Initialize()
    {
        InitializeMethodDict();

        _originBlocksMap = new GameObject[MAP_HEIGHT, MAP_WIDTH];
        _searchingBlocksMap = new GameObject[MAP_HEIGHT, MAP_WIDTH];

        _currentState = FindingState.WaitingForStartPoint;

        InitializeMap();
    }

    private void InitializeMap()
    {
        _originMap = new int[MAP_HEIGHT, MAP_WIDTH];
        _isFound = false;

        LoadMap();
        InitializeOriginBlocksMap();
        InitializeSearchingBlocksMap();
        RefreshMap();
    }
    private void LoadMap()
    {
        var mapText = Resources.Load<TextAsset>("map");
        if (mapText == null) { Debug.LogError("Map file cannot be found!"); return; }

        var mapString = mapText.text.Split('\n');
        for (int i = 0; i < mapString.Length; i++)
        {
            var singleRow = mapString[i].ToCharArray();

            for (int j = 0; j < singleRow.Length; j++)
            {
                if (singleRow[j] == '1') { _originMap[i, j] = POINT_WALL; continue; }
                if (singleRow[j] == ' ') { _originMap[i, j] = POINT_EMPTY; continue; }
            }
        }
    }
    private void InitializeOriginBlocksMap()
    {
        var ground = GameObject.Find("GroundPlane").transform;
        for (int i = 0; i < _originBlocksMap.GetLength(0); i++)
        {
            for (int j = 0; j < _originBlocksMap.GetLength(1); j++)
            {
                if (_originBlocksMap[i, j] == null)
                {
                    var spawnPos = new Vector3(transform.position.x + j, transform.position.y - 0.5f, transform.position.z - i);
                    _originBlocksMap[i, j] = Instantiate(_prefabObstacle, spawnPos, Quaternion.identity, ground);
                    _originBlocksMap[i, j].name = "Obstacle" + "(" + i + ", " + j + ")";
                }
                _originBlocksMap[i, j].SetActive(false);
            }
        }
    }
    private void InitializeSearchingBlocksMap()
    {
        var ground = GameObject.Find("GroundPlane").transform;
        for (int i = 0; i < _searchingBlocksMap.GetLength(0); i++)
        {
            for (int j = 0; j < _searchingBlocksMap.GetLength(1); j++)
            {
                if (_searchingBlocksMap[i, j] == null)
                {
                    var spawnPos = new Vector3(transform.position.x + j, transform.position.y - 1f, transform.position.z - i);
                    _searchingBlocksMap[i, j] = Instantiate(_prefabSearch, spawnPos, Quaternion.identity, ground);
                    _searchingBlocksMap[i, j].name = "Search" + "(" + i + ", " + j + ")";
                }
                _searchingBlocksMap[i, j].SetActive(false);
            }
        }
    }
    private void RefreshMap()
    {
        for (int i = 0; i < _originBlocksMap.GetLength(0); i++)
        {
            for (int j = 0; j < _originBlocksMap.GetLength(1); j++)
            {
                var tempGameObject = _originBlocksMap[i, j].gameObject;
                tempGameObject.SetActive(_originMap[i, j] == POINT_WALL);
            }
        }
    }

    private void FiniteStateMachine()
    {
        switch (_currentState)
        {
            case FindingState.WaitingForStartPoint:
                if (SetStartPoint()) { _currentState = FindingState.WaitingForEndPoint; }
                break;

            case FindingState.WaitingForEndPoint:
                if (SetEndPoint()) { _currentState = FindingState.StartPathFinding; }
                break;

            case FindingState.StartPathFinding:
                _currentMethod = _methodDict[_searchMethod];
                StartCoroutine(_currentMethod.StartFinding(this));
                _currentState = FindingState.CalculatingPath;
                break;

            case FindingState.CalculatingPath:
                if (_isFound) { _currentState = FindingState.ShowPath; }
                break;

            case FindingState.ShowPath:
                _currentMethod.ShowPath(this);
                _currentState = FindingState.Finish;
                break;

            case FindingState.Finish:
                if (Input.GetMouseButtonDown(1))
                {
                    RestartFinding();
                    _currentState = FindingState.WaitingForStartPoint;
                }
                break;

            default: break;
        }
    }
    private bool SetStartPoint()
    {
        if (!Input.GetMouseButtonDown(0)) { return false; }

        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (!Physics.Raycast(ray, out hit, float.MaxValue, LayerMask.GetMask("Ground"))) { return false; }

        int x = Mathf.RoundToInt(hit.point.x);
        int y = Mathf.RoundToInt(-hit.point.z);

        _originMap[y, x] = POINT_START;

        _startPoint = new MapPosition(y, x);
        var spawnPos = new Vector3(_startPoint.PosY, hit.point.y, -_startPoint.PosX);
        _startPointGameObject = Instantiate(_prefabPoint, spawnPos, Quaternion.identity);

        return true;
    }
    private bool SetEndPoint()
    {
        if (!Input.GetMouseButtonDown(0)) { return false; }

        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (!Physics.Raycast(ray, out hit, float.MaxValue, LayerMask.GetMask("Ground"))) { return false; }

        int x = Mathf.RoundToInt(hit.point.x);
        int y = Mathf.RoundToInt(-hit.point.z);

        _originMap[y, x] = POINT_END;

        _endPoint = new MapPosition(y, x);
        var spawnPos = new Vector3(_endPoint.PosY, hit.point.y, -_endPoint.PosX);
        _endPointGameObject = Instantiate(_prefabPoint, spawnPos, Quaternion.identity);

        return true;
    }
    private void InitializeMethodDict()
    {
        var assembly = Assembly.GetAssembly(typeof(IPathFindingMethod));
        var allMethodTypes = assembly.GetTypes().Where(t => typeof(IPathFindingMethod).IsAssignableFrom(t) && t.IsInterface == false);

        foreach (var methodType in allMethodTypes)
        {
            IPathFindingMethod method = Activator.CreateInstance(methodType) as IPathFindingMethod;
            _methodDict.Add(method.MethodType, method);
        }
    }
    private void RestartFinding()
    {
        Destroy(_startPointGameObject);
        Destroy(_endPointGameObject);
        InitializeMap();
    }
}

enum FindingState
{
    WaitingForStartPoint,
    WaitingForEndPoint,
    StartPathFinding,
    CalculatingPath,
    ShowPath,
    Finish
}
