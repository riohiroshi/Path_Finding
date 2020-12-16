using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MethodDFS : IPathFindingMethod
{
    private int[,] _searchingMap;
    private List<MapPosition> _searchingList;

    private SearchMethod _methodType = SearchMethod.DFS;

    public SearchMethod MethodType => _methodType;
    public void ShowPath(PathFinding pathFinding)
    {
        int x = pathFinding.EndPoint.PosX;
        int y = pathFinding.EndPoint.PosY;

        for (int i = 0; i < _searchingMap[pathFinding.EndPoint.PosX, pathFinding.EndPoint.PosY]; i++)
        {
            if (x >= 0 && x < PathFinding.MAP_HEIGHT && y >= 0 && y < PathFinding.MAP_WIDTH)
            {
                pathFinding.SearchingBlocksMap[x, y].GetComponent<Renderer>().material = pathFinding.PathMaterial;
                pathFinding.SearchingBlocksMap[x, y].transform.GetChild(0).GetChild(0).GetComponent<Text>().text = _searchingMap[x, y].ToString();

                if (x - 1 >= 0 && _searchingMap[x - 1, y] == _searchingMap[x, y] - 1)
                {
                    x = x - 1;
                    continue;
                }
                if (x + 1 < PathFinding.MAP_HEIGHT && _searchingMap[x + 1, y] == _searchingMap[x, y] - 1)
                {
                    x = x + 1;
                    continue;
                }
                if (y - 1 >= 0 && _searchingMap[x, y - 1] == _searchingMap[x, y] - 1)
                {
                    y = y - 1;
                    continue;
                }
                if (y + 1 < PathFinding.MAP_WIDTH && _searchingMap[x, y + 1] == _searchingMap[x, y] - 1)
                {
                    y = y + 1;
                    continue;
                }
            }
        }
    }
    public IEnumerator StartFinding(PathFinding pathFinding)
    {
        _searchingList = new List<MapPosition>();

        _searchingMap = new int[pathFinding.OriginMap.GetLength(0), pathFinding.OriginMap.GetLength(1)];
        InitializeSearchingMap();
        _searchingMap[pathFinding.StartPoint.PosX, pathFinding.StartPoint.PosY] = 0;

        _searchingList.Add(pathFinding.StartPoint);

        var checkingPos = _searchingList[_searchingList.Count - 1];
        _searchingList.RemoveAt(_searchingList.Count - 1);

        while (!checkingPos.Equals(pathFinding.EndPoint))
        {
            if (CheckPos(-1, 0)) { yield break; }
            if (CheckPos(1, 0)) { yield break; }
            if (CheckPos(0, -1)) { yield break; }
            if (CheckPos(0, 1)) { yield break; }

            checkingPos = _searchingList[_searchingList.Count - 1];
            _searchingList.RemoveAt(_searchingList.Count - 1);

            yield return null;
        }

        bool CheckPos(int offsetX, int offsetY)
        {
            int nextX = checkingPos.PosX + offsetX;
            int nextY = checkingPos.PosY + offsetY;

            if (nextX < 0 || nextX >= PathFinding.MAP_HEIGHT) { return false; }
            if (nextY < 0 || nextY >= PathFinding.MAP_WIDTH) { return false; }

            if (pathFinding.OriginMap[nextX, nextY] == PathFinding.POINT_END)
            {
                _searchingMap[nextX, nextY] = _searchingMap[checkingPos.PosX, checkingPos.PosY] + 1;

                UpdateSearchBlock(pathFinding, nextX, nextY);

                pathFinding.SetIsFound(true);
                _searchingList.Clear();
                return true;
            }

            if (pathFinding.OriginMap[nextX, nextY] == PathFinding.POINT_EMPTY && (_searchingMap[nextX, nextY] == PathFinding.POINT_EMPTY || _searchingMap[nextX, nextY] > _searchingMap[checkingPos.PosX, checkingPos.PosY]))
            {
                var tempPos = new MapPosition(nextX, nextY);
                _searchingMap[nextX, nextY] = _searchingMap[checkingPos.PosX, checkingPos.PosY] + 1;
                pathFinding.OriginBlocksMap[nextX, nextY].SetActive(false);

                UpdateSearchBlock(pathFinding, nextX, nextY);

                _searchingList.Add(tempPos);
            }

            return false;
        }
    }

    private void InitializeSearchingMap()
    {
        for (int i = 0; i < _searchingMap.GetLength(0); i++)
        {
            for (int j = 0; j < _searchingMap.GetLength(1); j++)
            {
                _searchingMap[i, j] = PathFinding.POINT_EMPTY;
            }
        }
    }
    private void UpdateSearchBlock(PathFinding pathFinding, int nextX, int nextY)
    {
        var tempTextParent = pathFinding.SearchingBlocksMap[nextX, nextY].transform.GetChild(0);
        tempTextParent.GetChild(0).GetComponent<Text>().text = _searchingMap[nextX, nextY].ToString();
        tempTextParent.GetChild(1).GetComponent<Text>().text = "0";
        tempTextParent.GetChild(2).GetComponent<Text>().text = "0";

        var tempRenderer = pathFinding.SearchingBlocksMap[nextX, nextY].GetComponent<Renderer>();
        var tempMaterial = pathFinding.SearchMaterial;
        var block = new Material(tempMaterial);
        var colorID = Shader.PropertyToID("_Color");
        var colorOffset = _searchingMap[nextX, nextY] * 1.4f;
        var colorOffsetR = ((tempMaterial.color.r * 255 + colorOffset) % 255f) / 255f;
        var colorOffsetG = ((tempMaterial.color.g * 255/* + colorOffset*/) % 255f) / 255f;
        var colorOffsetB = ((tempMaterial.color.b * 255 + colorOffset) % 255f) / 255f;
        var tempColor = new Color(colorOffsetR, colorOffsetG, colorOffsetB);
        block.SetColor(colorID, tempColor);
        tempRenderer.material = block;

        pathFinding.SearchingBlocksMap[nextX, nextY].SetActive(true);
    }
}
