using DG.Tweening;
using ModestTree;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

public class GridManager : MonoBehaviour
{
    public static event UnityAction<int, int> GridManagerEvent;

    [Header("Grid Settings")]
    [SerializeField] private int _gridX;
    [SerializeField] protected int _gridY;
    public int GridX => _gridX;
    public int GridY => _gridY;

    [Header("Anim Settings")]
    [SerializeField] private Ease _destroyEase = Ease.InBounce;
    [SerializeField] private Ease _swapEase = Ease.OutQuad;
    [SerializeField] private float _swapDuration = 0.15f;

    private WaitForSeconds _wait = new WaitForSeconds(0.35f);
    private Vector3 _startPos = new Vector3(1f, 1f, 1f);
    private Vector2Int _lastSwapDirection = Vector2Int.right;

    public Tile[,] tiles => _tiles;
    public Obstacle[,] obstacles => _obstacles;
    public PowerUpHandler GetPowerUpHandler() => _powerUpHandler;

    private Coroutine _hintRoutine;
    [SerializeField] private float _hintDelay = 3f;
    [SerializeField] private float _repeatDelay = 3f;
    [SerializeField] private float _firstHintDelay = 3f;
    private bool _hintShown = false;

    private Tile[,] _tiles;
    private Obstacle[,] _obstacles;
    private TilePool _tilePool;
    private CheckMatch _checkMatch;
    private ObstaclePool _obstaclePool;
    private AnimationHandler _animationHandler;
    private ObstacleManager _obstacleManager;
    private PowerUp _powerUp;
    private PowerUpHandler _powerUpHandler;

    [SerializeField] private TextMeshProUGUI _shaypeText;

    [Inject]
    public void structInject(TilePool tilePool, CheckMatch checkMatch, ObstaclePool obstaclePool, AnimationHandler animationHandler, ObstacleManager obstacleManager,
        PowerUp powerUp, PowerUpHandler powerUpHandler)
    {
        _tilePool = tilePool;
        _checkMatch = checkMatch;
        _obstaclePool = obstaclePool;
        _animationHandler = animationHandler;
        _obstacleManager = obstacleManager;
        _powerUp = powerUp;
        _powerUpHandler = powerUpHandler;
    }

    private void Start()
    {
        _obstacles = new Obstacle[_gridX, _gridY];
        _obstacleManager.Initialize(_obstacles, this, _gridX, _gridY, _obstaclePool);
        Initialize(_gridX, _gridY, _obstacles);
        _obstacleManager.CreateObstacle();
        GenerateGrid();
        GridManager.GridManagerEvent?.Invoke(_gridX, _gridY);
        _powerUpHandler.Initilaize(_gridX, _gridY);
        ResetHintTime();

    }

    public void Initialize(int gridX, int gridY, Obstacle[,] obstacles)
    {
        _gridX = gridX;
        _gridY = gridY;
        _obstacles = obstacles;
    }
    private void ResetHintTime()
    {
        if (_hintRoutine != null)
            StopCoroutine(_hintRoutine);

        float delay = _hintShown ? _repeatDelay : _firstHintDelay;
        _hintRoutine = StartCoroutine(HintTimerRoutine(delay));
    }

    private IEnumerator HintTimerRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        ShowHint();
        _hintShown = true;
    }
    private void HighlightTiles(List<Tile> tiles)
    {
        foreach (var tile in tiles)
        {
            if (tile != null && tile.gameObject.activeInHierarchy)
            {
                tile.transform.DOShakeScale(0.5f, 0.3f, 10, 90, false);
            }
        }
    }
    private void SwapTilesHint(Tile tile1, Tile tile2)
    {
        if (tile1 == null || tile2 == null) return;

        Vector2Int pos1 = tile1.GridPos;
        Vector2Int pos2 = tile2.GridPos;

        _tiles[pos1.x, pos1.y] = tile2;
        _tiles[pos2.x, pos2.y] = tile1;

        tile1.SetGridPos(pos2);
        tile2.SetGridPos(pos1);
    }
    private void ShowHint()
    {
        for (int x = 0; x < _gridX; x++)
        {
            for (int y = 0; y < _gridY; y++)
            {
                Tile tile = _tiles[x, y];
                if (tile == null) continue;

                Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
                foreach (var dir in directions)
                {
                    Vector2Int swapPos = tile.GridPos + dir;
                    if (!IsValidGridPos(swapPos)) continue;
                    Tile swapTile = _tiles[swapPos.x, swapPos.y];
                    if (swapTile == null) continue;

                    SwapTilesHint(tile, swapTile);

                    var match1 = _checkMatch.FindTileMatches(_tiles, tile.GridPos);
                    var match2 = _checkMatch.FindTileMatches(_tiles, swapTile.GridPos);

                    if (match1.Count >= 3 || match2.Count >= 3)
                    {
                        HighlightTiles(new List<Tile> { tile, swapTile });
                        SwapTilesHint(tile, swapTile);

                        _hintShown = true;
                        ResetHintTime();
                        return;
                    }

                    SwapTilesHint(tile, swapTile);
                }
            }
        }
    }


    public IEnumerator RainDownRoutine()
    {
        yield return new WaitForSeconds(0.2f);
        List<Tile> fallenTiles = new List<Tile>();
        for (int i = 0; i < _gridX; i++)
        {
            for (int j = 0; j < _gridY; j++)
            {
                if (_tiles[i, j] == null && _obstacles[i, j] == null)
                {
                    for (int newY = j + 1; newY < _gridY; newY++)
                    {
                        Tile newMoveTile = _tiles[i, newY];
                        if (newMoveTile != null && !IsObstacle(i, newY, j))
                        {
                            _tiles[i, newY] = null;
                            _tiles[i, j] = newMoveTile;
                            newMoveTile.Initialize(i, j, this);
                            newMoveTile.SetGridPos(new Vector2Int(i, j));
                            Vector3 targetPos = new Vector3(i, j, 0);
                            _animationHandler.SwapAnim(newMoveTile.transform, targetPos, 0.3f, Ease.OutBounce);
                            fallenTiles.Add(newMoveTile);
                            break;
                        }
                    }
                    if (_tiles[i, j] == null && _obstacles[i, j] == null && !IsObstacle(i, GridY, j))
                    {
                        CreateNewTile(i, j);
                        fallenTiles.Add(_tiles[i, j]);
                    }
                }
            }
        }
        yield return new WaitForSeconds(0.2f);
        if (fallenTiles.Count > 0)
        {
            yield return new WaitForSeconds(0.22f);
            HasAnyMatches(fallenTiles);
            ResetHintTime();
        }
    }


    private bool IsObstacle(int x, int startY, int endY)
    {
        for (int y = startY - 1; y > endY; y--) // end dahil değil.
        {
            if (_obstacles[x, y] != null && !_obstacles[x, y].IsDestroy)
            {
                return true;
            }
        }
        return false;
    }
    private void CreateNewTile(int x, int y)
    {
        if (_tiles[x, y] != null && _tiles[x, y].gameObject == null)
        {
            _tiles[x, y] = null;
        }

        Vector3 startPos = new Vector3(x, _gridY + 1, 0);
        Vector3 targetPos = new Vector3(x, y, 0);
        int randomIndex = StartMatchCheck(x, y);
        GameObject newTileObject = _tilePool.GetTile(randomIndex);

        newTileObject.SetActive(true);
        newTileObject.transform.position = startPos;
        newTileObject.transform.localScale = _startPos;
        newTileObject.transform.SetParent(transform);

        Tile tile = newTileObject.GetComponent<Tile>();
        tile.Initialize(x, y, this);
        tile.SetPowerUpHandler(_powerUpHandler);
        _tiles[x, y] = tile;
        _animationHandler.SwapAnim(newTileObject.transform, targetPos, 0.3f, Ease.OutBounce);
    }

    public void HasAnyMatches(List<Tile> tiles)
    {
        StartCoroutine(HasAnyMatchesRoutine(tiles));

    }
    private IEnumerator HasAnyMatchesRoutine(List<Tile> tiles)
    {
        bool foundMatch = false;
        HashSet<Tile> matchedTiles = new HashSet<Tile>();

        for (int x = 0; x < _gridX; x++)
        {
            for (int y = 0; y < _gridY; y++)
            {
                Tile startTile = _tiles[x, y];

                if (startTile == null || matchedTiles.Contains(startTile))
                    continue;

                // Flood fill ile bağlı aynı renkteki tüm taşları bul
                List<Tile> sameTile = new List<Tile>();
                Queue<Tile> tileList = new Queue<Tile>();
                tileList.Enqueue(startTile);
                matchedTiles.Add(startTile);
                int targetId = startTile.TileId;

                while (tileList.Count > 0)
                {
                    Tile current = tileList.Dequeue();
                    sameTile.Add(current);
                    Vector2Int pos = current.GetGridPos();
                    Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
                    foreach (var dir in directions)
                    {
                        int nx = pos.x + dir.x;
                        int ny = pos.y + dir.y;
                        if (nx >= 0 && nx < _gridX && ny >= 0 && ny < _gridY)
                        {
                            Tile neighbor = _tiles[nx, ny];
                            if (neighbor != null && !matchedTiles.Contains(neighbor) && neighbor.TileId == targetId)
                            {
                                tileList.Enqueue(neighbor);
                                matchedTiles.Add(neighbor);
                            }
                        }
                    }
                }

                if (sameTile.Count >= 3)
                {
                    MatchShapeType maxShape = MatchShapeType.None;
                    List<Tile> maxMatch = new List<Tile>();
                    Tile originTile = null;

                    foreach (Tile tile in sameTile)
                    {
                        var match = _checkMatch.FindTileMatches(_tiles, tile.GetGridPos());
                        if (match.Count >= 3)
                        {
                            // L/T varsa rocket'ı asla öncelikli yapma!
                            if (_checkMatch.shapeType == MatchShapeType.LShape || _checkMatch.shapeType == MatchShapeType.TShape)
                            {
                                if (maxShape != MatchShapeType.LShape && maxShape != MatchShapeType.TShape)
                                {
                                    maxShape = _checkMatch.shapeType;
                                    maxMatch = match;
                                    originTile = tile;
                                }
                            }
                            // L/T yoksa, normal karşılaştırma
                            else if (maxShape != MatchShapeType.LShape && maxShape != MatchShapeType.TShape)
                            {
                                if ((int)_checkMatch.shapeType > (int)maxShape || ((int)_checkMatch.shapeType == (int)maxShape && match.Count > maxMatch.Count))
                                {
                                    maxShape = _checkMatch.shapeType;
                                    maxMatch = match;
                                    originTile = tile;
                                }
                            }
                        }
                    }

                    if (maxMatch.Count >= 3)
                    {
                        foundMatch = true;

                        // Power-up oluşacaksa animasyon çalışsın
                        if (maxShape != MatchShapeType.ThreeMatch && maxShape != MatchShapeType.None)
                        {
                            yield return StartCoroutine(_animationHandler.GatherMatchedTileRoutine(maxMatch, originTile, 0.25f));
                        }

                        Debug.Log($"[Match Check] maxMatch Count: {maxMatch.Count}");
                        var distinct = maxMatch.Distinct().ToList();
                        Debug.Log($"[Match Check] Distinct Count: {distinct.Count}");
                        if (maxMatch.Count != distinct.Count)
                        {
                            Debug.LogWarning("❗ Duplicate tiles found in maxMatch list before destruction.");
                        }

                        // Power-up oluşacaksa, originTile'ı yok etme listesinden çıkar
                        if (maxShape != MatchShapeType.None && maxShape != MatchShapeType.ThreeMatch && originTile != null)
                        {
                            maxMatch.Remove(originTile);
                        }
                       
                        // Her zaman yok et!
                        foreach (Tile destroyTile in maxMatch)
                        {
                            ReturnThePoolAnim(destroyTile);
                        }
                        _shaypeText.text = maxShape.ToString();

                        // Power-up sadece 3'lüden büyüklerde oluşsun
                        if (maxShape != MatchShapeType.None && maxShape != MatchShapeType.ThreeMatch)
                        {
                            _powerUp.CreatePowerUpTile(originTile, maxShape, _lastSwapDirection, this);
                        }

                        _obstacleManager.ObstacleMatches(maxMatch);
                    }
                }
            }
        }

        yield return new WaitForSeconds(0.2f);

        if (foundMatch)
        {
            ResetHintTime();
            StartCoroutine(RainDownRoutine());
        }
    }





    public void ReturnThePoolAnim(Tile tile)
    {
        if (tile == null) return;
        Vector2Int gridPos = tile.GridPos;


        // Grid'deki referansı animasyon başlamadan hemen önce sıfırla  
        if (IsValidGridPos(gridPos) && _tiles[gridPos.x, gridPos.y] == tile)
        {
            _tiles[gridPos.x, gridPos.y] = null;
        }

        DOTween.Kill(tile.transform);
        tile.transform.DOKill();
        _animationHandler.DestroyAnim(tile.transform, 0.25f, _destroyEase, () =>
        {
            if (tile != null && tile.gameObject != null)
            {
                _tilePool.ReturnTile(tile.gameObject);
            }
        });
    }
    private void CheckMatchAndHandle(Tile tile1, Tile tile2, Vector2Int start, Vector2Int end, Vector3 pos, Vector3 pos1)
    {
        List<Tile> matchedList = _checkMatch.FindTileMatches(_tiles, tile1.GridPos);
        List<Tile> matchedList2 = _checkMatch.FindTileMatches(_tiles, tile2.GridPos);

        var AllMatch = new HashSet<Tile>(matchedList);
        AllMatch.UnionWith(matchedList2);

        if (AllMatch.Count == 0)
        {

            _tiles[start.x, start.y] = tile1;
            _tiles[end.x, end.y] = tile2;

            tile1.SetGridPos(start);
            tile2.SetGridPos(end);

            //tile1.GetGridPos();
            //tile2.GetGridPos();
            _animationHandler.SwapAnim(tile1.transform, pos, _swapDuration, _swapEase);
            _animationHandler.SwapAnim(tile2.transform, pos1, _swapDuration, _swapEase);
           
        }
        else
        {
            HasAnyMatches(AllMatch.ToList());
        }
       
    }
    public void SwapTile(Vector2Int Start, Vector2Int End)
    {
        Tile tile1 = _tiles[Start.x, Start.y];
        Tile tile2 = _tiles[End.x, End.y];

        _tiles[Start.x, Start.y] = tile2;
        _tiles[End.x, End.y] = tile1;

        tile1.SetGridPos(End);
        tile2.SetGridPos(Start);

        Vector3 pos = tile1.transform.position;
        Vector3 pos2 = tile2.transform.position;

        _lastSwapDirection = End - Start;
        _hintShown = false;
        ResetHintTime();
        _animationHandler.SwapAnim(tile1.transform, pos2, _swapDuration, _swapEase);
        _animationHandler.SwapAnim(tile2.transform, pos, _swapDuration, _swapEase, () =>
        {
            if (tile1._PowerUpType == PowerUpType.LightBomb)
            {
                _powerUpHandler.OnLightBombSwapped(tile1, tile2);
                return;
            }
            if (tile2._PowerUpType == PowerUpType.LightBomb)
            {
                _powerUpHandler.OnLightBombSwapped(tile2, tile1);
                return;
            }

            bool tile1Power = tile1._PowerUpType != PowerUpType.None;
            bool tile2Power = tile2._PowerUpType != PowerUpType.None;
            if (tile1Power || tile2Power)
            {
                if (tile1Power) tile1.PowerUpEffect(tile2);
                if (tile2Power) tile2.PowerUpEffect(tile1);
                return;
            }
            CheckMatchAndHandle(tile1, tile2, Start, End, pos, pos2);
        });
    }

    private int StartMatchCheck(int x, int y)
    {
        int tryCount = 0;
        int randomIndex;
        int currentId;
        do
        {
            randomIndex = Random.Range(0, _tilePool.tilePrefabs.Length);
            currentId = _tilePool.tilePrefabs[randomIndex].GetComponent<Tile>().TileId;
            // Satır ve sütun kontrolleri
            bool left2Same = (x > 1 && _tiles[x - 1, y] != null && _tiles[x - 2, y] != null &&
                              _tiles[x - 1, y].TileId == currentId && _tiles[x - 2, y].TileId == currentId);
            bool left3Same = (x > 2 && _tiles[x - 1, y] != null && _tiles[x - 2, y] != null && _tiles[x - 3, y] != null &&
                              _tiles[x - 1, y].TileId == currentId && _tiles[x - 2, y].TileId == currentId && _tiles[x - 3, y].TileId == currentId);
            bool down2Same = (y > 1 && _tiles[x, y - 1] != null && _tiles[x, y - 2] != null &&
                              _tiles[x, y - 1].TileId == currentId && _tiles[x, y - 2].TileId == currentId);
            bool down3Same = (y > 2 && _tiles[x, y - 1] != null && _tiles[x, y - 2] != null && _tiles[x, y - 3] != null &&
                              _tiles[x, y - 1].TileId == currentId && _tiles[x, y - 2].TileId == currentId && _tiles[x, y - 3].TileId == currentId);
            // Kare (2x2 square) kontrolü
            bool squareSame = (x > 0 && y > 0 &&
                _tiles[x - 1, y] != null && _tiles[x, y - 1] != null && _tiles[x - 1, y - 1] != null &&
                _tiles[x - 1, y].TileId == currentId &&
                _tiles[x, y - 1].TileId == currentId &&
                _tiles[x - 1, y - 1].TileId == currentId);
            if (!left2Same && !left3Same && !down2Same && !down3Same && !squareSame)
                break;
            tryCount++;
        } while (tryCount < 50);
        return randomIndex;
    }
    private void GenerateGrid()
    {
        int tileCount = 0;
        _tiles = new Tile[_gridX, _gridY];

        for (int i = 0; i < _gridX; i++)
        {
            for (int j = 0; j < _gridY; j++)
            {
                if (_obstacles[i, j] != null) continue;

                Vector3 pos = new Vector3(transform.position.x + i, transform.position.y + j, 0);
                int randomIndex = StartMatchCheck(i, j);
                GameObject newTileObject = _tilePool.GetTile(randomIndex);
                newTileObject.gameObject.SetActive(true);
                newTileObject.transform.position = pos;
                newTileObject.transform.localScale = _startPos;
                newTileObject.transform.SetParent(transform);

                Tile tile = newTileObject.GetComponent<Tile>();
                tile.Initialize(i, j, this);
                tile.SetPowerUpHandler(_powerUpHandler);
                _tiles[i, j] = tile;

                tileCount++;
            }
        }

        Debug.Log("Tile Count: " + tileCount);
    }

    public void UpdateTileGrid(Vector2Int gridPos, Tile tile)
    {
        _tiles[gridPos.x, gridPos.y] = tile;
    }

    public bool IsValidGridPos(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < _gridX && pos.y >= 0 && pos.y < _gridY;
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < _gridX; i++)
        {
            for (int j = 0; j < _gridY; j++)
            {
                Vector3 pos = new Vector3(transform.position.x + i, transform.position.y + j, 0);
                Vector3 size = new Vector3(1, 1, 1);
                Gizmos.DrawWireCube(pos, size);
            }
        }
    }
}