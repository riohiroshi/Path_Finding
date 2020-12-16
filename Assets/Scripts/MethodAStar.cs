using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MethodAStar : IPathFindingMethod
{
    private AStarScore[,] _searchingMap;
    private List<MapPosition> _searchingList;

    private SearchMethod _methodType = SearchMethod.AStar;

    public SearchMethod MethodType => _methodType;
    public void ShowPath(PathFinding pathFinding)
    {
        var pos = pathFinding.EndPoint;

        while (!pos.Equals(pathFinding.StartPoint))
        {
            var gameObject = pathFinding.SearchingBlocksMap[pos.PosX, pos.PosY];
            gameObject.GetComponent<Renderer>().material = pathFinding.PathMaterial;

            pos = _searchingMap[pos.PosX, pos.PosY].Parent;
        }
    }
    public IEnumerator StartFinding(PathFinding pathFinding)
    {
        _searchingList = new List<MapPosition>();

        _searchingMap = new AStarScore[pathFinding.OriginMap.GetLength(0), pathFinding.OriginMap.GetLength(1)];
        _searchingMap[pathFinding.StartPoint.PosX, pathFinding.StartPoint.PosY] = new AStarScore(0, 0);

        _searchingList.Add(pathFinding.StartPoint);

        var checkingPos = _searchingList[0];
        _searchingList.RemoveAt(0);

        while (!checkingPos.Equals(pathFinding.EndPoint))
        {
            if (CheckPos(-1, 0)) { yield break; }
            if (CheckPos(1, 0)) { yield break; }
            if (CheckPos(0, -1)) { yield break; }
            if (CheckPos(0, 1)) { yield break; }

            _searchingList.Sort((MapPosition posA, MapPosition posB) =>
            {
                var aStarA = _searchingMap[posA.PosX, posA.PosY];
                var aStarB = _searchingMap[posB.PosX, posB.PosY];

                return aStarA.CompareTo(aStarB);
            });

            checkingPos = _searchingList[0];
            _searchingList.RemoveAt(0);

            yield return null;
        }

        bool CheckPos(int offsetX, int offsetY)
        {
            int nextX = checkingPos.PosX + offsetX;
            int nextY = checkingPos.PosY + offsetY;

            if (nextX < 0 || nextX >= PathFinding.MAP_HEIGHT) { return false; }
            if (nextY < 0 || nextY >= PathFinding.MAP_WIDTH) { return false; }

            var tempScore = _searchingMap[nextX, nextY];
            //if (tempScore != null) { return false; }
            var checkingScore = _searchingMap[checkingPos.PosX, checkingPos.PosY];
            var tempPos = new MapPosition(nextX, nextY);

            if (pathFinding.OriginMap[nextX, nextY] == PathFinding.POINT_END)
            {
                var a = new AStarScore(checkingScore.G + 1, 0);
                a.SetParent(checkingPos);
                _searchingMap[nextX, nextY] = a;

                UpdateSearchBlock(pathFinding, nextX, nextY);

                pathFinding.SetIsFound(true);
                _searchingList.Clear();
                return true;
            }

            if (pathFinding.OriginMap[nextX, nextY] == PathFinding.POINT_EMPTY)
            {
                if (tempScore == null)
                {
                    var a = new AStarScore(checkingScore.G + 1, MapPosition.AStarDistance(tempPos, pathFinding.EndPoint));
                    a.SetParent(checkingPos);
                    _searchingMap[nextX, nextY] = a;
                    _searchingList.Add(tempPos);
                }
                else if (tempScore.G > checkingScore.G + 1)
                {
                    tempScore.SetGScore(checkingScore.G + 1);
                    tempScore.SetParent(checkingPos);

                    if (!_searchingList.Contains(tempPos)) { _searchingList.Add(tempPos); }
                }

                UpdateSearchBlock(pathFinding, nextX, nextY);
            }

            return false;
        }
    }

    private void UpdateSearchBlock(PathFinding pathFinding, int nextX, int nextY)
    {
        var tempTextParent = pathFinding.SearchingBlocksMap[nextX, nextY].transform.GetChild(0);
        tempTextParent.GetChild(0).GetComponent<Text>().text = _searchingMap[nextX, nextY].G.ToString();
        tempTextParent.GetChild(1).GetComponent<Text>().text = _searchingMap[nextX, nextY].H.ToString();
        tempTextParent.GetChild(2).GetComponent<Text>().text = _searchingMap[nextX, nextY].F.ToString();

        var tempRenderer = pathFinding.SearchingBlocksMap[nextX, nextY].GetComponent<Renderer>();
        var tempMaterial = pathFinding.SearchMaterial;
        var block = new Material(tempMaterial);
        var colorID = Shader.PropertyToID("_Color");
        var colorOffset = _searchingMap[nextX, nextY].F * 1.4f;
        var colorOffsetR = ((tempMaterial.color.r * 255 + colorOffset) % 255f) / 255f;
        var colorOffsetG = ((tempMaterial.color.g * 255/* + colorOffset*/) % 255f) / 255f;
        var colorOffsetB = ((tempMaterial.color.b * 255 + colorOffset) % 255f) / 255f;
        var tempColor = new Color(colorOffsetR, colorOffsetG, colorOffsetB);
        block.SetColor(colorID, tempColor);
        tempRenderer.material = block;

        pathFinding.SearchingBlocksMap[nextX, nextY].SetActive(true);
    }
}
