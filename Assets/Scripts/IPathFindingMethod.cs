using System.Collections;

public interface IPathFindingMethod
{
    SearchMethod MethodType { get; }
    IEnumerator StartFinding(PathFinding pathFinding);
    void ShowPath(PathFinding pathFinding);
}

public enum SearchMethod
{
    BFS,
    DFS,
    AStar
}
