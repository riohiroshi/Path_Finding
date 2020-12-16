using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class GameMap : MonoBehaviour
{
    const int W = 30;
    const int H = 20;

    const int START = 8;
    const int END = 9;
    const int WALL = 1;
    const int EMPTY = -1;


    public GameObject prefabSearch;
    public GameObject prefabPath;
    public GameObject prefabObstacle;
    public GameObject prefabGround;
    public GameObject prefabPoint;

    public Material pathMat;
    public Material searchMat;

    public SearchMethod searchMethod = SearchMethod.BFS;


    int[,] map;
    int[,] search;

    AStarScore[,] astarSearch;

    MapPosition startPoint;
    GameObject startPointGameObject;
    MapPosition endPoint;
    GameObject endPointGameObject;

    Queue<MapPosition> searchingQueue;
    List<MapPosition> searchingList;

    GameObject[,] mapBlocks;
    GameObject[,] searchBlocks;

    State currentState;

    bool isFound;


    // Start is called before the first frame update
    void Start()
    {
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        FSM();
    }

    void Init()
    {
        mapBlocks = new GameObject[H, W];
        searchBlocks = new GameObject[H, W];
        map = new int[H, W];

        currentState = State.WaitingForStartPoint;
        isFound = false;

        ReadMap();

        InitMap();

        RefreshMap();
    }

    void FSM()
    {
        switch (currentState)
        {
            case State.WaitingForStartPoint:
                // if (Input.GetMouseButtonDown(0))
                // {
                //     startPoint = SetPoint();
                //     search[startPoint.PosX, startPoint.PosY] = 0;
                //     searchingQueue.Enqueue(startPoint);
                //     currentState = State.WaitingForEndPoint;
                // }
                if (SetPoint(START))
                {
                    currentState = State.WaitingForEndPoint;
                }
                break;
            case State.WaitingForEndPoint:
                // if (Input.GetMouseButtonDown(0))
                // {
                //     endPoint = SetPoint();
                //     currentState = State.StartPathFinding;
                // }
                if (SetPoint(END))
                {
                    currentState = State.StartPathFinding;
                }
                break;
            case State.StartPathFinding:
                if (searchMethod == SearchMethod.BFS)
                {
                    StartCoroutine(BFS());
                }
                else if (searchMethod == SearchMethod.DFS)
                {
                    StartCoroutine(DFS());
                }
                else if (searchMethod == SearchMethod.AStar)
                {
                    //StartCoroutine(AStar());
                    Astar();
                }
                currentState = State.CalculatingPath;
                break;
            case State.CalculatingPath:
                if (isFound)
                {
                    currentState = State.ShowPath;
                }
                break;
            case State.ShowPath:
                if (searchMethod == SearchMethod.BFS || searchMethod == SearchMethod.DFS)
                {
                    ShowPath_BFS();
                }
                // else if (searchMethod == SearchMethod.DFS)
                // {

                // }
                else if (searchMethod == SearchMethod.AStar)
                {
                    ShowPath_AStar();
                }
                currentState = State.Finish;
                break;
            case State.Finish:
                if (Input.GetMouseButtonDown(1))
                {
                    RestartFinding();
                    currentState = State.WaitingForStartPoint;
                }
                break;
            default:
                break;
        }
    }

    // Pos SetPoint1()
    // {
    //     Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    //     RaycastHit hit;
    //     if (Physics.Raycast(ray, out hit, float.MaxValue, LayerMask.GetMask("Ground")))
    //     {
    //         Pos pos = new Pos(Mathf.RoundToInt(-hit.point.z), Mathf.RoundToInt(hit.point.x), 0);
    //         Vector3 spawnPos = new Vector3(pos.PosY, hit.point.y, -pos.PosX);
    //         Instantiate(prefabPoint, spawnPos, Quaternion.identity);

    //         Debug.Log(pos.PosX + ", " + pos.PosY);
    //         return pos;
    //     }

    //     throw new System.Exception();
    //     //return null;
    // }

    bool SetPoint(int n)
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, float.MaxValue, LayerMask.GetMask("Ground")))
            {
                int x = Mathf.RoundToInt(hit.point.x);
                int y = Mathf.RoundToInt(-hit.point.z);

                map[y, x] = n;

                if (n == START)
                {
                    startPoint = new MapPosition(y, x/* , 0 */);
                    Vector3 spawnPos = new Vector3(startPoint.PosY, hit.point.y, -startPoint.PosX);
                    startPointGameObject = Instantiate(prefabPoint, spawnPos, Quaternion.identity);
                }
                else if (n == END)
                {
                    endPoint = new MapPosition(y, x/* , 0 */);
                    Vector3 spawnPos = new Vector3(endPoint.PosY, hit.point.y, -endPoint.PosX);
                    endPointGameObject = Instantiate(prefabPoint, spawnPos, Quaternion.identity);
                }
                //Debug.Log(y + ", " + x);

                return true;
            }
        }

        return false;
    }

    IEnumerator BFS()
    {
        searchingQueue = new Queue<MapPosition>();
        search = new int[map.GetLength(0), map.GetLength(1)];
        for (int i = 0; i < search.GetLength(0); i++)
        {
            for (int j = 0; j < search.GetLength(1); j++)
            {
                search[i, j] = EMPTY;
            }
        }

        search[startPoint.PosX, startPoint.PosY] = 0;
        //searchingQueue.Clear();
        searchingQueue.Enqueue(startPoint);

        MapPosition checkingPos = searchingQueue.Dequeue();

        while (!checkingPos.Equals(endPoint))
        {
            // int up = checkingPos.PosX - 1;
            // int down = checkingPos.PosX + 1;
            // int left = checkingPos.PosY - 1;
            // int right = checkingPos.PosY + 1;

            // if (CheckPos(up, checkingPos.PosY)) yield break;
            // if (CheckPos(down, checkingPos.PosY)) yield break;
            // if (CheckPos(checkingPos.PosX, left)) yield break;
            // if (CheckPos(checkingPos.PosX, right)) yield break;
            if (CheckPos(-1, 0)) yield break;
            if (CheckPos(1, 0)) yield break;
            if (CheckPos(0, -1)) yield break;
            if (CheckPos(0, 1)) yield break;

            checkingPos = searchingQueue.Dequeue();
            //Debug.Log(checkingPos.PosX + ", " + checkingPos.PosY);
            //yield return new WaitForSeconds(.01f);
            yield return null;
        }

        bool CheckPos(int offsetX, int offsetY)
        {
            int x = checkingPos.PosX + offsetX;
            int y = checkingPos.PosY + offsetY;

            if (x >= 0 && x < H && y >= 0 && y < W)
            {
                if (map[x, y] == END)
                {
                    //Debug.Log(x + ", " + y);

                    search[x, y] = search[checkingPos.PosX, checkingPos.PosY] + 1;

                    searchBlocks[x, y].transform.GetChild(0).GetChild(0).GetComponent<Text>().text = search[x, y].ToString();
                    searchBlocks[x, y].GetComponent<Renderer>().material = searchMat;
                    searchBlocks[x, y].SetActive(true);

                    isFound = true;
                    searchingQueue.Clear();
                    return true;
                }

                if (map[x, y] == EMPTY && search[x, y] == EMPTY)
                {
                    MapPosition temp = new MapPosition(x, y/* , checkingPos.step + 1 */);
                    //search[x, y] = temp.step;
                    search[x, y] = search[checkingPos.PosX, checkingPos.PosY] + 1;

                    //Vector3 spawnPos = new Vector3(temp.PosY, transform.position.y - 0.5f, -temp.PosX);
                    //Instantiate(prefabSearch, spawnPos, Quaternion.identity).transform.GetChild(0).GetChild(0).GetComponent<Text>().text = temp.step.ToString();
                    mapBlocks[x, y].SetActive(false);

                    searchBlocks[x, y].transform.GetChild(0).GetChild(0).GetComponent<Text>().text = search[x, y].ToString();
                    searchBlocks[x, y].GetComponent<Renderer>().material = searchMat;
                    searchBlocks[x, y].SetActive(true);

                    //Debug.Log(temp.PosX + ", " + temp.PosY);

                    // if (temp.Equals(endPoint))
                    // {
                    //     endPoint.step = temp.step;
                    //     isFound = true;
                    //     searchingQueue.Clear();
                    //     return true;
                    // }

                    searchingQueue.Enqueue(temp);
                }
            }

            return false;
        }
    }

    IEnumerator DFS()
    {
        //searchingQueue = new Queue<Pos>();
        searchingList = new List<MapPosition>();

        search = new int[map.GetLength(0), map.GetLength(1)];
        for (int i = 0; i < search.GetLength(0); i++)
        {
            for (int j = 0; j < search.GetLength(1); j++)
            {
                search[i, j] = EMPTY;
            }
        }

        search[startPoint.PosX, startPoint.PosY] = 0;

        //searchingQueue.Enqueue(startPoint);
        searchingList.Add(startPoint);

        //Pos checkingPos = searchingQueue.Dequeue();
        MapPosition checkingPos = searchingList[searchingList.Count - 1];
        searchingList.RemoveAt(searchingList.Count - 1);

        while (!checkingPos.Equals(endPoint))
        {
            if (CheckPos(-1, 0)) yield break;
            if (CheckPos(1, 0)) yield break;
            if (CheckPos(0, -1)) yield break;
            if (CheckPos(0, 1)) yield break;

            //checkingPos = searchingQueue.Dequeue();
            checkingPos = searchingList[searchingList.Count - 1];
            searchingList.RemoveAt(searchingList.Count - 1);

            yield return null;
        }

        bool CheckPos(int offsetX, int offsetY)
        {
            int x = checkingPos.PosX + offsetX;
            int y = checkingPos.PosY + offsetY;



            if (x >= 0 && x < H && y >= 0 && y < W)
            {
                Debug.Log(x + ", " + y);

                if (map[x, y] == END)
                {
                    search[x, y] = search[checkingPos.PosX, checkingPos.PosY] + 1;

                    searchBlocks[x, y].transform.GetChild(0).GetChild(0).GetComponent<Text>().text = search[x, y].ToString();
                    searchBlocks[x, y].GetComponent<Renderer>().material = searchMat;
                    searchBlocks[x, y].SetActive(true);

                    isFound = true;
                    //searchingQueue.Clear();
                    searchingList.Clear();
                    return true;
                }

                if (map[x, y] == EMPTY && (search[x, y] == EMPTY || search[x, y] > search[checkingPos.PosX, checkingPos.PosY]))
                {
                    MapPosition temp = new MapPosition(x, y);

                    search[x, y] = search[checkingPos.PosX, checkingPos.PosY] + 1;

                    mapBlocks[x, y].SetActive(false);

                    searchBlocks[x, y].transform.GetChild(0).GetChild(0).GetComponent<Text>().text = search[x, y].ToString();
                    searchBlocks[x, y].GetComponent<Renderer>().material = searchMat;
                    searchBlocks[x, y].SetActive(true);

                    searchingList.Add(temp);
                }
            }

            return false;
        }
    }

    IEnumerator AStar()
    {
        searchingList = new List<MapPosition>();
        astarSearch = new AStarScore[map.GetLength(0), map.GetLength(1)];

        astarSearch[startPoint.PosX, startPoint.PosY] = new AStarScore(0, 0);
        //astarSearch[startPoint.PosX, startPoint.PosY].closed = true;
        searchingList.Add(startPoint);

        MapPosition checkingPos = searchingList[0];
        searchingList.RemoveAt(0);

        while (!checkingPos.Equals(endPoint))
        {
            if (CheckPos(-1, 0)) yield break;
            if (CheckPos(1, 0)) yield break;
            if (CheckPos(0, -1)) yield break;
            if (CheckPos(0, 1)) yield break;

            searchingList.Sort((MapPosition pos1, MapPosition pos2) =>
            {
                AStarScore a1 = astarSearch[pos1.PosX, pos1.PosY];
                AStarScore a2 = astarSearch[pos2.PosX, pos2.PosY];

                return a1.CompareTo(a2);
            });

            checkingPos = searchingList[0];
            searchingList.RemoveAt(0);
            //astarSearch[checkingPos.PosX, checkingPos.PosY].closed = true;

            yield return null;
        }

        bool CheckPos(int offsetX, int offsetY)
        {
            int x = checkingPos.PosX + offsetX;
            int y = checkingPos.PosY + offsetY;

            if (x >= 0 && x < H && y >= 0 && y < W)
            {
                AStarScore tempScore = astarSearch[x, y];
                if (tempScore != null)//&& tempScore.closed)
                {
                    return false;
                }

                AStarScore checkingScore = astarSearch[checkingPos.PosX, checkingPos.PosY];
                MapPosition tempPos = new MapPosition(x, y);

                if (map[x, y] == END)
                {
                    ////////////////AScore a = new AScore(checkingScore._gScore + 1, 0);
                    ////////////////a.Parent = checkingPos;
                    ////////////////astarSearch[x, y] = a;

                    //////////////////searchBlocks[x, y].transform.GetChild(0).GetChild(0).GetComponent<Text>().text = astarSearch[x, y].g.ToString();
                    //////////////////searchBlocks[x, y].transform.GetChild(0).GetChild(1).GetComponent<Text>().text = astarSearch[x, y].h.ToString();
                    searchBlocks[x, y].transform.GetChild(0).GetChild(2).GetComponent<Text>().text = astarSearch[x, y].F.ToString();
                    searchBlocks[x, y].GetComponent<Renderer>().material = searchMat;
                    searchBlocks[x, y].SetActive(true);

                    isFound = true;
                    searchingList.Clear();
                    return true;
                }
                if (map[x, y] == EMPTY)
                {
                    //////////////////if (tempScore == null)
                    //////////////////{
                    //////////////////    AScore a = new AScore(checkingScore._gScore + 1, MapPosition.AStarDistance(tempPos, endPoint));
                    //////////////////    a.Parent = checkingPos;
                    //////////////////    astarSearch[x, y] = a;
                    //////////////////    searchingList.Add(tempPos);
                    //////////////////}
                    //////////////////else if (tempScore._gScore > checkingScore._gScore + 1)
                    //////////////////{
                    //////////////////    tempScore._gScore = checkingScore._gScore + 1;
                    //////////////////    tempScore.Parent = checkingPos;
                    //////////////////    //tempScore.closed = false;
                    //////////////////    if (!searchingList.Contains(tempPos))
                    //////////////////    {
                    //////////////////        searchingList.Add(tempPos);
                    //////////////////    }
                    //////////////////}

                    ////////////////////searchBlocks[x, y].transform.GetChild(0).GetChild(0).GetComponent<Text>().text = astarSearch[x, y].g.ToString();
                    ////////////////////searchBlocks[x, y].transform.GetChild(0).GetChild(1).GetComponent<Text>().text = astarSearch[x, y].h.ToString();
                    searchBlocks[x, y].transform.GetChild(0).GetChild(2).GetComponent<Text>().text = astarSearch[x, y].F.ToString();
                    searchBlocks[x, y].GetComponent<Renderer>().material = searchMat;
                    searchBlocks[x, y].SetActive(true);
                }
            }

            return false;
        }
    }

    void Astar()
    {
        searchingList = new List<MapPosition>();
        astarSearch = new AStarScore[map.GetLength(0), map.GetLength(1)];

        astarSearch[startPoint.PosX, startPoint.PosY] = new AStarScore(0, 0);

        searchingList.Add(startPoint);

        MapPosition checkingPos = searchingList[0];
        searchingList.RemoveAt(0);

        while (!checkingPos.Equals(endPoint))
        {
            if (CheckPos(-1, 0)) return;
            if (CheckPos(1, 0)) return;
            if (CheckPos(0, -1)) return;
            if (CheckPos(0, 1)) return;

            searchingList.Sort((MapPosition pos1, MapPosition pos2) =>
            {
                AStarScore a1 = astarSearch[pos1.PosX, pos1.PosY];
                AStarScore a2 = astarSearch[pos2.PosX, pos2.PosY];

                return a1.CompareTo(a2);
            });

            checkingPos = searchingList[0];
            searchingList.RemoveAt(0);
        }

        bool CheckPos(int offsetX, int offsetY)
        {
            int x = checkingPos.PosX + offsetX;
            int y = checkingPos.PosY + offsetY;

            if (x >= 0 && x < H && y >= 0 && y < W)
            {
                AStarScore tempScore = astarSearch[x, y];

                AStarScore checkingScore = astarSearch[checkingPos.PosX, checkingPos.PosY];
                MapPosition tempPos = new MapPosition(x, y);

                //////////////////if (map[x, y] == END)
                //////////////////{
                //////////////////    AScore a = new AScore(checkingScore._gScore + 1, 0);
                //////////////////    a.Parent = checkingPos;
                //////////////////    astarSearch[x, y] = a;

                //////////////////    searchBlocks[x, y].transform.GetChild(0).GetChild(0).GetComponent<Text>().text = astarSearch[x, y].g.ToString();
                //////////////////    searchBlocks[x, y].transform.GetChild(0).GetChild(1).GetComponent<Text>().text = astarSearch[x, y].h.ToString();
                //////////////////    searchBlocks[x, y].transform.GetChild(0).GetChild(2).GetComponent<Text>().text = astarSearch[x, y].F.ToString();
                //////////////////    searchBlocks[x, y].GetComponent<Renderer>().material = searchMat;
                //////////////////    searchBlocks[x, y].SetActive(true);

                //////////////////    isFound = true;
                //////////////////    searchingList.Clear();
                //////////////////    return true;
                //////////////////}
                //////////////////if (map[x, y] == EMPTY)
                //////////////////{
                //////////////////    if (tempScore == null)
                //////////////////    {
                //////////////////        AScore a = new AScore(checkingScore._gScore + 1, MapPosition.AStarDistance(tempPos, endPoint));
                //////////////////        a.Parent = checkingPos;
                //////////////////        astarSearch[x, y] = a;
                //////////////////        searchingList.Add(tempPos);
                //////////////////    }
                //////////////////    else if (tempScore._gScore > checkingScore._gScore + 1)
                //////////////////    {
                //////////////////        tempScore._gScore = checkingScore._gScore + 1;
                //////////////////        tempScore.Parent = checkingPos;

                //////////////////        if (!searchingList.Contains(tempPos))
                //////////////////        {
                //////////////////            searchingList.Add(tempPos);
                //////////////////        }
                //////////////////    }

                //////////////////    searchBlocks[x, y].transform.GetChild(0).GetChild(0).GetComponent<Text>().text = astarSearch[x, y].g.ToString();
                //////////////////    searchBlocks[x, y].transform.GetChild(0).GetChild(1).GetComponent<Text>().text = astarSearch[x, y].h.ToString();
                //////////////////    searchBlocks[x, y].transform.GetChild(0).GetChild(2).GetComponent<Text>().text = astarSearch[x, y].F.ToString();
                //////////////////    searchBlocks[x, y].GetComponent<Renderer>().material = searchMat;
                //////////////////    searchBlocks[x, y].SetActive(true);
                //////////////////}
            }

            return false;
        }
    }

    void ReadMap()
    {
        TextAsset mapText = Resources.Load<TextAsset>("map");
        if (mapText == null)
        {
            Debug.LogError("Map file cannot be found!");
            return;
        }

        string[] mapString = mapText.text.Split('\n');

        for (int i = 0; i < mapString.Length; i++)
        {
            char[] singleRow = mapString[i].ToCharArray();
            for (int j = 0; j < singleRow.Length; j++)
            {
                if (singleRow[j] == '1')
                {
                    map[i, j] = WALL;
                    //mapBlocks[i, j] = prefabObstacle;
                }
                if (singleRow[j] == ' ')
                {
                    map[i, j] = EMPTY;
                    //mapBlocks[i, j] = prefabGround;
                }
            }
        }
    }

    void InitMap()
    {
        Transform ground = GameObject.Find("GroundPlane").transform;
        for (int i = 0; i < mapBlocks.GetLength(0); i++)
        {
            for (int j = 0; j < mapBlocks.GetLength(1); j++)
            {
                // if (mapBlocks[i, j] != null)
                // {
                //     Vector3 spawnPos = new Vector3(transform.position.x + j, transform.position.y - 1, transform.position.z - i);
                //     Instantiate(mapBlocks[i, j], spawnPos, Quaternion.identity, ground);
                // }

                if (mapBlocks[i, j] == null)
                {
                    Vector3 spawnPos = new Vector3(transform.position.x + j, transform.position.y - 0.5f, transform.position.z - i);
                    mapBlocks[i, j] = Instantiate(prefabObstacle, spawnPos, Quaternion.identity, ground);
                    mapBlocks[i, j].name = "Obstacle" + "(" + i + ", " + j + ")";
                }
                mapBlocks[i, j].SetActive(false);
            }
        }

        for (int i = 0; i < searchBlocks.GetLength(0); i++)
        {
            for (int j = 0; j < searchBlocks.GetLength(1); j++)
            {
                if (searchBlocks[i, j] == null)
                {
                    Vector3 spawnPos = new Vector3(transform.position.x + j, transform.position.y - 1f, transform.position.z - i);
                    searchBlocks[i, j] = Instantiate(prefabSearch, spawnPos, Quaternion.identity, ground);
                    searchBlocks[i, j].name = "Search" + "(" + i + ", " + j + ")";
                }
                searchBlocks[i, j].SetActive(false);
            }
        }
    }

    void RefreshMap()
    {
        for (int i = 0; i < mapBlocks.GetLength(0); i++)
        {
            for (int j = 0; j < mapBlocks.GetLength(1); j++)
            {
                GameObject go = mapBlocks[i, j].gameObject;
                switch (map[i, j])
                {
                    case EMPTY:
                        go.SetActive(false);
                        break;
                    case WALL:
                        go.SetActive(true);
                        break;
                }
            }
        }
    }

    void RestartFinding()
    {
        Destroy(startPointGameObject);
        Destroy(endPointGameObject);

        map = new int[H, W];

        isFound = false;

        ReadMap();
        InitMap();
        RefreshMap();
    }

    void ShowPath_BFS()
    {
        int x = endPoint.PosX;
        int y = endPoint.PosY;

        for (int i = 0; i < search[endPoint.PosX, endPoint.PosY]; i++)
        {
            if (x >= 0 && x < H && y >= 0 && y < W)
            {
                //Vector3 spawnPos = new Vector3(y, transform.position.y, -x);
                //Instantiate(prefabPath, spawnPos, Quaternion.identity).transform.GetChild(0).GetChild(0).GetComponent<Text>().text = search[x, y].ToString();
                searchBlocks[x, y].GetComponent<Renderer>().material = pathMat;
                searchBlocks[x, y].transform.GetChild(0).GetChild(0).GetComponent<Text>().text = search[x, y].ToString();

                if (x - 1 >= 0 && search[x - 1, y] == search[x, y] - 1)
                {
                    x = x - 1;
                    continue;
                }
                if (x + 1 < H && search[x + 1, y] == search[x, y] - 1)
                {
                    x = x + 1;
                    continue;
                }
                if (y - 1 >= 0 && search[x, y - 1] == search[x, y] - 1)
                {
                    y = y - 1;
                    continue;
                }
                if (y + 1 < W && search[x, y + 1] == search[x, y] - 1)
                {
                    y = y + 1;
                    continue;
                }
            }
        }
    }

    void ShowPath_AStar()
    {
        MapPosition pos = endPoint;
        while (!pos.Equals(startPoint))
        {
            GameObject gameObject = searchBlocks[pos.PosX, pos.PosY];
            gameObject.GetComponent<Renderer>().material = pathMat;

            pos = astarSearch[pos.PosX, pos.PosY].Parent;
        }
    }

    void DebugSearch()
    {
        for (int i = 0; i < search.GetLength(0); i++)
        {
            string s = "";
            for (int j = 0; j < search.GetLength(1); j++)
            {
                s += search[i, j].ToString();
            }
            Debug.Log(s);
        }
    }

    void DebugMap()
    {
        for (int i = 0; i < map.GetLength(0); i++)
        {
            string s = "";
            for (int j = 0; j < map.GetLength(1); j++)
            {
                s += map[i, j].ToString();
            }
            Debug.Log(s);
        }
    }
}

enum State
{
    WaitingForStartPoint,
    WaitingForEndPoint,
    StartPathFinding,
    CalculatingPath,
    ShowPath,
    Finish
}
