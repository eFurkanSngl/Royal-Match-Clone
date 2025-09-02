using System.Collections.Generic;
using UnityEngine;

public class CheckMatch : MonoBehaviour
{
    public MatchShapeType shapeType { get; private set; } = MatchShapeType.None;

    public List<Tile> FindTileMatches(Tile[,] tiles, Vector2Int origin)
    {
        shapeType = MatchShapeType.None;

        if (!IsValid(origin, tiles) || !IsActiveTile(tiles[origin.x, origin.y]))
            return new List<Tile>();

        Tile originTile = tiles[origin.x, origin.y];
        int id = originTile.TileId;

        int upCount = CheckDirection(tiles, origin, Vector2Int.up, id);
        int downCount = CheckDirection(tiles, origin, Vector2Int.down, id);
        int leftCount = CheckDirection(tiles, origin, Vector2Int.left, id);
        int rightCount = CheckDirection(tiles, origin, Vector2Int.right, id);

        HashSet<Tile> matchedTiles = new HashSet<Tile>();

        // Horizontal eşleşme
        List<Tile> horizontal = new List<Tile> { originTile };
        for (int i = 1; i <= leftCount; i++) horizontal.Add(tiles[origin.x - i, origin.y]);
        for (int i = 1; i <= rightCount; i++) horizontal.Add(tiles[origin.x + i, origin.y]);
        if (horizontal.Count >= 3)
            foreach (Tile t in horizontal) matchedTiles.Add(t);

        // Vertical eşleşme
        List<Tile> vertical = new List<Tile> { originTile };
        for (int i = 1; i <= upCount; i++) vertical.Add(tiles[origin.x, origin.y + i]);
        for (int i = 1; i <= downCount; i++) vertical.Add(tiles[origin.x, origin.y - i]);
        if (vertical.Count >= 3)
            foreach (Tile t in vertical) matchedTiles.Add(t);

        // Square kontrolü ayrı tutulur
        List<Tile> squareMatch = new List<Tile>();
        bool hasSquare = false;
        Vector2Int[] offsets = {
    new Vector2Int(0,0), new Vector2Int(1,0),
    new Vector2Int(0,1), new Vector2Int(1,1)
};
        foreach (var offset in offsets)
        {
            Vector2Int basePos = origin - offset;
            if (IsSquareMatch(tiles, basePos, id))
            {
                squareMatch.Add(tiles[basePos.x, basePos.y]);
                squareMatch.Add(tiles[basePos.x + 1, basePos.y]);
                squareMatch.Add(tiles[basePos.x, basePos.y + 1]);
                squareMatch.Add(tiles[basePos.x + 1, basePos.y + 1]);
                hasSquare = true;
            }
        }
        if (hasSquare)
            foreach (Tile t in squareMatch) matchedTiles.Add(t);


        // Diğer şekil kontrolleri
        if (horizontal.Count == 5 || vertical.Count == 5)
        {
            shapeType = MatchShapeType.FiveMatch;
        }
        else if (IsLShapeSimple(upCount, downCount, leftCount, rightCount))
        {
            shapeType = MatchShapeType.LShape;
        }
        else if (IsTShapeSimple(upCount, downCount, leftCount, rightCount))
        {
            shapeType = MatchShapeType.TShape;
        }
        
        else if ((horizontal.Count == 4 || vertical.Count == 4) && matchedTiles.Count == 4)
        {
            shapeType = MatchShapeType.FourMatch;
        }
        else if (hasSquare)
        {
            shapeType = MatchShapeType.SquareShape;
        }
        //else if (matchedTiles.Count == 3)
        //{
        //    shapeType = MatchShapeType.ThreeMatch;
        //}

        return matchedTiles.Count >= 3 ? new List<Tile>(matchedTiles) : new List<Tile>();
    }

    private int CheckDirection(Tile[,] tiles, Vector2Int origin, Vector2Int dir, int id)
    {
        int count = 0;
        Vector2Int current = origin + dir;
        while (IsValid(current, tiles))
        {
            Tile tile = tiles[current.x, current.y];
            if (!IsActiveTile(tile) || tile.TileId != id || tile._PowerUpType != PowerUpType.None)
                break;
            count++;
            current += dir;
        }
        return count;
    }

    public bool IsSquareMatch(Tile[,] tiles, Vector2Int basePos, int id)
    {

        if (!IsValid(basePos, tiles) ||
            !IsValid(basePos + Vector2Int.right, tiles) ||
            !IsValid(basePos + Vector2Int.up, tiles) ||
            !IsValid(basePos + Vector2Int.right + Vector2Int.up, tiles))
            return false;

        Tile t1 = tiles[basePos.x, basePos.y];
        Tile t2 = tiles[basePos.x + 1, basePos.y];
        Tile t3 = tiles[basePos.x, basePos.y + 1];
        Tile t4 = tiles[basePos.x + 1, basePos.y + 1];

        return (IsActiveTile(t1) && IsActiveTile(t2) && IsActiveTile(t3) && IsActiveTile(t4)
          && t1.TileId == id && t1._PowerUpType == PowerUpType.None
          && t2.TileId == id && t2._PowerUpType == PowerUpType.None
          && t3.TileId == id && t3._PowerUpType == PowerUpType.None
          && t4.TileId == id && t4._PowerUpType == PowerUpType.None);
    }

    private bool IsTShapeSimple(int up, int down, int left, int right)
    {
        if ((left + right + 1) >= 3 && (up >= 2 || down >= 2))
            return true;
        if ((up + down + 1) >= 3 && (left >= 2 || right >= 2))
            return true;
        return false;
    }

    private bool IsLShapeSimple(int up, int down, int left, int right)
    {
        if (up >= 2 && right >= 2) return true;
        if (up >= 2 && left >= 2) return true;
        if (down >= 2 && right >= 2) return true;
        if (down >= 2 && left >= 2) return true;
        return false;
    }

    private bool IsValid(Vector2Int pos, Tile[,] tiles)
    {
        int width = tiles.GetLength(0);
        int height = tiles.GetLength(1);
        return pos.x >= 0 && pos.y >= 0 && pos.x < width && pos.y < height;
    }

    private bool IsActiveTile(Tile tile)
    {
        return tile != null && tile.gameObject.activeSelf;
    }
}

public enum MatchShapeType
{
    None,
    TShape,
    LShape,
    SquareShape,
    FourMatch,
    ThreeMatch,
    FiveMatch,
}