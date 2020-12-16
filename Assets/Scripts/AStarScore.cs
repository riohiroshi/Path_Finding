public class AStarScore
{
    private int _gScore = 0;   //G 是從起點出發的步數
    private int _hScore = 0;   //H 是估算的離終點距離
    private MapPosition _parent;

    public int G => _gScore;
    public int H => _hScore;
    public int F => _gScore + _hScore;
    public MapPosition Parent => _parent;

    public AStarScore(int gScore, int hScore)
    {
        _gScore = gScore;
        _hScore = hScore;
    }

    public void SetParent(MapPosition value) => _parent = value;
    public void SetGScore(int value) => _gScore = value;
    public int CompareTo(AStarScore aScore) => F.CompareTo(aScore.F);
    public bool Equals(AStarScore aScore) => F.Equals(aScore.F);
}
