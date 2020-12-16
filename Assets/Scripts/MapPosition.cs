using UnityEngine;

public struct MapPosition
{
    public int PosX;
    public int PosY;

    public MapPosition(int x, int y)
    {
        PosX = x;
        PosY = y;
    }

    public static int AStarDistance(MapPosition p1, MapPosition p2) => Mathf.Abs(p1.PosX - p2.PosX) + Mathf.Abs(p1.PosY - p2.PosY);

    public bool Equals(MapPosition p) => PosX == p.PosX && PosY == p.PosY;
}