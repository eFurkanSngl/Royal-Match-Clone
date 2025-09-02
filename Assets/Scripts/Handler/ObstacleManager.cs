using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class ObstacleManager : MonoBehaviour
{
    private Obstacle[,] _obstacles;
    private GridManager _gridManager;
    private int _obstacleCount = 4;
    private int _gridX;
    private int _gridY;

    [Inject] private ObstaclePool _obstaclePool;
    [Inject] private AnimationHandler _animationHandler;
    [Inject] private ObstaclePool _pool;

    [SerializeField] private Ease _destroyEase;
    [SerializeField] private float _swapDuration;

    public void Initialize(Obstacle[,] obstacles, GridManager gridManger,int gridX, int gridY,ObstaclePool pool)
    {
        _obstacles = obstacles;
        _gridManager = gridManger;
        _gridX = gridX;
        _gridY = gridY;
        _pool = pool;
    }
    public void CreateObstacle()
    {
        for(int i = 0; i <_gridX; i++)
        {
            for(int j = 0; j < _obstacleCount; j++)
            {
                Vector3 pos = new Vector3(transform.position.x + i, transform.position.y + j, 0f);
                GameObject obstacleObject = _pool.GetObstacle();
                obstacleObject.transform.position = pos;
                Obstacle obs = obstacleObject.GetComponent<Obstacle>();
                obs.Initialize(new Vector2Int(i, j));
                _obstacles[i, j] = obs;
            }
        }
    }

    public void ObstacleMatches(List<Tile> matchedTiles)
    {
        int gridX = _gridManager.GridX;
        int gridY = _gridManager.GridY;

        for (int i = 0; i < gridX; i++)
        {
            for (int j = 0; j < gridY; j++)
            {
                if (_obstacles[i, j] != null && !_obstacles[i, j].IsDestroy)
                {
                    foreach (Tile match in matchedTiles)
                    {
                        if (match == null) continue;
                        IsMatch(match, i, j);
                    }
                }
            }
        }
    }

    private void IsMatch(Tile tile, int gridX, int gridY)
    {
        if (tile == null) return;
        if (!_gridManager.IsValidGridPos(tile.GridPos)) return;
        if (_obstacles[gridX, gridY] == null || _obstacles[gridX, gridY].IsDestroy) return;

        Vector2Int obstaclePos = _obstacles[gridX, gridY].GridPos;
        Obstacle obstacle = _obstacles[gridX, gridY];

        if (tile.GridPos == obstaclePos ||
            tile.GridPos + Vector2Int.down == obstaclePos ||
            tile.GridPos + Vector2Int.left == obstaclePos ||
            tile.GridPos + Vector2Int.right == obstaclePos)
        {

            _animationHandler.DestroyAnim(obstacle.transform, _swapDuration, _destroyEase, () =>
            {
                if (_obstacles[gridX, gridY] == null || _obstacles[gridX, gridY].gameObject == null) return;

                _obstaclePool.ReturnObstacle(_obstacles[gridX, gridY].gameObject);
                _obstacles[gridX, gridY] = null;

            });
        }
    }

    public bool HasObstacle(Vector2Int pos)
    {
        return _gridManager.IsValidGridPos(pos) && _obstacles[pos.x, pos.y] != null &&! _obstacles[pos.x,pos.y].IsDestroy;
    }

    public void DestroyObstacleAt(Vector2Int pos)
    {
        if (!_gridManager.IsValidGridPos(pos))
        {
            return;
        }

        Obstacle obstacle = _obstacles[pos.x, pos.y];
        if (obstacle == null || obstacle.IsDestroy)
        {
            Debug.Log("obstacle bulunumadý ya da zaten yok edilmiþ");
            return;
        }
        _animationHandler.DestroyAnim(obstacle.transform, _swapDuration, _destroyEase, () =>
        {
            _obstaclePool.ReturnObstacle(obstacle.gameObject);
            _obstacles[pos.x, pos.y] = null;
            Debug.Log("Obstacle yok edildi havuza ");
        });
    }

  
}
