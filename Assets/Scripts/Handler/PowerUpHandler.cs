using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
public class PowerUpHandler : MonoBehaviour
{
    [Inject] private GridManager _gridManager;
    [Inject] private ObstacleManager _obstacleManager;
    [Inject] private TilePool _tilePool;

    [SerializeField] private GameObject _rocketTrialDown;
    [SerializeField] private GameObject _rocketTrialUp;
    [SerializeField] private GameObject _rocketTrialRight;
    [SerializeField] private GameObject _rocketTrialLeft;
    [SerializeField] private GameObject _propellarAnim;
    [SerializeField] private GameObject _bombAnim;

    private int _gridX;
    private int _gridY;

    public void Initilaize(int gridX, int gridY)
    {
        _gridX = gridX;
        _gridY = gridY;
    }

    public void Propellar(Tile propellerTile)
    {
        List<Vector2Int> obstaclePositions = new List<Vector2Int>();

        for (int x = 0; x < _gridX; x++)
        {
            for (int y = 0; y < _gridY; y++)
            {
                if (_gridManager.obstacles[x, y] != null)
                {
                    obstaclePositions.Add(new Vector2Int(x, y));
                }
            }
        }
        if (obstaclePositions.Count > 0 && propellerTile != null)
        {
            Vector2Int randomObstaclePos = obstaclePositions[Random.Range(0, obstaclePositions.Count)];
            StartCoroutine(PropellarAnimRoutine(propellerTile, randomObstaclePos));
        }
        else if (obstaclePositions.Count > 0)
        {
            Vector2Int randomObstaclePos = obstaclePositions[Random.Range(0, obstaclePositions.Count)];
            _obstacleManager.DestroyObstacleAt(randomObstaclePos);
            _gridManager.obstacles[randomObstaclePos.x, randomObstaclePos.y] = null;
            StartCoroutine(_gridManager.RainDownRoutine());
        }
        StartCoroutine(_gridManager.RainDownRoutine());

    }

    private IEnumerator PropellarAnimRoutine(Tile propellerTile, Vector2Int targetGridPos)
    {
        if (propellerTile == null || propellerTile.gameObject == null) yield break;

        Vector3 propellerStartPos = propellerTile.transform.position;
        Vector3 targetWorldPos = new Vector3(targetGridPos.x, targetGridPos.y, 0);
        Vector2Int propellerPos = propellerTile.GridPos;

        if (propellerTile.transform != null)
        {
            DOTween.Kill(propellerTile.transform);
            propellerTile.transform.DOKill();
        }

        SpriteRenderer spriteRenderer = propellerTile.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            DOTween.Kill(spriteRenderer);
        }

        _gridManager.tiles[propellerPos.x, propellerPos.y] = null;
        //Destroy(propellerTile.gameObject);
        _tilePool.ReturnTile(propellerTile.gameObject);

        if (_propellarAnim != null)
        {
            GameObject propellerObj = Instantiate(_propellarAnim, propellerStartPos, Quaternion.identity);

            float flyDuration = 0.5f;
            propellerObj.transform.DOMove(targetWorldPos, flyDuration).SetEase(Ease.InOutQuad);
            yield return new WaitForSeconds(flyDuration);

            Destroy(propellerObj);
        }

        _obstacleManager.DestroyObstacleAt(targetGridPos);
        _gridManager.obstacles[targetGridPos.x, targetGridPos.y] = null;

        yield return new WaitForSeconds(0.1f);
        yield return StartCoroutine(_gridManager.RainDownRoutine());
    }

    public void OnLightBombSwapped(Tile lightBombTile, Tile targetTile)
    {
        if (targetTile == null) return;
        int targetId = targetTile.TileId;
        StartCoroutine(LightBombRoutine(targetId, lightBombTile));
    }
    private IEnumerator LightBombRoutine(int tileId, Tile lightBombTile)
    {
        List<Tile> destroyTiles = new List<Tile>();

        lightBombTile.transform.DOScale(1.2f, 0.3f).SetEase(Ease.OutBack);
        yield return new WaitForSeconds(0.2f);

        for (int x = 0; x < _gridX; x++)
        {
            for (int y = 0; y < _gridY; y++)
            {
                Tile tile = _gridManager.tiles[x, y];
                if (tile != null && tile.TileId == tileId)
                {
                    DOTween.Kill(tile.transform);
                    tile.transform.DOKill();
                    _gridManager.tiles[x, y] = null;
                    destroyTiles.Add(tile);
                    tile.transform.DOScale(1.2f, 0.3f).SetEase(Ease.OutBack);
                }
            }
        }

        yield return new WaitForSeconds(0.2f);
        ReturnToPool(destroyTiles);
        ReturnToPool(new List<Tile> { lightBombTile });

        yield return new WaitForSeconds(0.2f);
        yield return StartCoroutine(_gridManager.RainDownRoutine());
    }
    public void OnLightBombClicked(Tile lightBombTile)
    {
        List<int> possibleIds = new List<int>();
        for (int x = 0; x < _gridManager.GridX; x++)
        {
            for (int y = 0; y < _gridManager.GridY; y++)
            {
                Tile tile = _gridManager.tiles[x, y];
                if (tile != null && tile._PowerUpType == PowerUpType.None)
                    possibleIds.Add(tile.TileId);
            }
        }
        if (possibleIds.Count == 0) return;

        int randomId = possibleIds[Random.Range(0, possibleIds.Count)];
        StartCoroutine(LightBombRoutine(randomId, lightBombTile));
    }


    public void Bomb(Vector2Int center)
    {
        StartCoroutine(BombEffectRoutine(center));
    }

    private IEnumerator BombEffectRoutine(Vector2Int center)
    {
        Tile bombTile = _gridManager.tiles[center.x, center.y];
        if (bombTile != null && bombTile.gameObject != null && bombTile.gameObject.activeInHierarchy)
        {
            bombTile.transform.DOScale(0.8f, 0.25f).SetEase(Ease.OutBack);
            yield return new WaitForSeconds(0.2f);

            bombTile.transform.DOShakePosition(0.7f, 0.5f, 10, 90, false, true);
            yield return new WaitForSeconds(0.35f);

            bombTile.transform.DOScale(0f, 0.12f).SetEase(Ease.InBack);
            yield return new WaitForSeconds(0.12f);
        }

        BombEffect(center);

        if (bombTile != null)
        {
            _gridManager.tiles[center.x, center.y] = null;
            DOTween.Kill(bombTile.transform);
            bombTile.transform.DOKill();
            _tilePool.ReturnTile(bombTile.gameObject);
            bombTile.gameObject.SetActive(false);
        }

        if (bombTile != null && bombTile.gameObject != null)
        {
            GameObject newBomb = Instantiate(_bombAnim, bombTile.transform.position, Quaternion.identity);
            Destroy(newBomb, 0.3f);
        }

        yield return new WaitForSeconds(0.1f);
        yield return StartCoroutine(_gridManager.RainDownRoutine());
    }

    private void BombEffect(Vector2Int center)
    {
        List<Tile> destroyTiles = new List<Tile>();
        for (int centerX = -1; centerX <= 1; centerX++)
        {
            for (int centerY = -1; centerY <= 1; centerY++)
            {
                int tileX = center.x + centerX;
                int tileY = center.y + centerY;
                if (_gridManager.IsValidGridPos(new Vector2Int(tileX, tileY)) && _gridManager.tiles[tileX, tileY] != null)
                {
                    Tile tile = _gridManager.tiles[tileX, tileY];
                    DOTween.Kill(tile.transform);
                    tile.transform.DOKill();

                    _gridManager.tiles[tileX, tileY].transform.DOKill();
                    _gridManager.tiles[tileX, tileY] = null;

                    destroyTiles.Add(tile);
                }
                Vector2Int pos = new Vector2Int(tileX, tileY);
                if (_obstacleManager.HasObstacle(pos))
                {
                    _obstacleManager.DestroyObstacleAt(pos);
                }
            }
        }
        ReturnToPool(destroyTiles);
    }

    public void VerticalRocket(int gridX)
    {
        StartCoroutine(VerticalRocketRoutine(gridX));
    }

    public IEnumerator VerticalRocketRoutine(int gridX)
    {
        yield return StartCoroutine(VerticalRocketAnim(gridX));

        List<Tile> destroyTiles = new List<Tile>();
        for (int y = 0; y < _gridY; y++)
        {
            if (_gridManager.tiles[gridX, y] != null)
            {
                destroyTiles.Add(_gridManager.tiles[gridX, y]);
                _gridManager.tiles[gridX, y] = null;
            }

            Vector2Int pos = new Vector2Int(gridX, y);
            if (_obstacleManager.HasObstacle(pos))
            {
                _obstacleManager.DestroyObstacleAt(pos);
            }
        }
        ReturnToPool(destroyTiles);

        yield return new WaitForSeconds(0.1f);
        yield return StartCoroutine(_gridManager.RainDownRoutine());
        Debug.Log("vertical rocket is worked");
    }

    private IEnumerator VerticalRocketAnim(int gridX)
    {
        Tile rocketTile = null;
        for (int y = 0; y < _gridY; y++)
        {
            if (_gridManager.tiles[gridX, y] != null && _gridManager.tiles[gridX, y]._PowerUpType == PowerUpType.VerticalRocket)
            {
                rocketTile = _gridManager.tiles[gridX, y];
                break;
            }
        }
        if (rocketTile != null && rocketTile.gameObject != null && rocketTile.gameObject.activeInHierarchy)
        {
            rocketTile.transform.DOScale(1.2f, 0.15f).SetEase(Ease.OutBack);
            yield return new WaitForSeconds(0.15f);

            if (rocketTile != null && rocketTile.gameObject != null && rocketTile.gameObject.activeInHierarchy)
            {
                rocketTile.transform.DOShakePosition(0.18f, new Vector3(0, 0.2f, 0), 10, 90, false, true);
                yield return new WaitForSeconds(0.18f);
            }

            if (rocketTile != null && rocketTile.gameObject != null && rocketTile.gameObject.activeInHierarchy && _rocketTrialUp != null && _rocketTrialDown != null)
            {
                GameObject upRocket = Instantiate(_rocketTrialUp, rocketTile.transform.position, Quaternion.identity);
                float upTargetY = rocketTile.transform.position.y + (_gridManager.GridY - rocketTile.GridPos.y + 2);
                upRocket.transform.DOMoveY(upTargetY, 0.25f);
                Destroy(upRocket, 0.3f);

                GameObject downRocket = Instantiate(_rocketTrialDown, rocketTile.transform.position, Quaternion.identity);
                float downTargetY = rocketTile.transform.position.y - rocketTile.GridPos.y - 2;
                downRocket.transform.DOMoveY(downTargetY, 0.25f);
                Destroy(downRocket, 0.3f);
            }

            if (rocketTile != null && rocketTile.gameObject != null && rocketTile.gameObject.activeInHierarchy)
            {
                rocketTile.transform.DOScale(0f, 0.12f).SetEase(Ease.InBack);
                yield return new WaitForSeconds(0.12f);
            }
        }
    }
    private IEnumerator HorizontalRocketAnim(int gridY)
    {
        Tile rocketTile = null;
        for (int x = 0; x < _gridX; x++)
        {
            if (_gridManager.tiles[x, gridY] != null && _gridManager.tiles[x, gridY]._PowerUpType == PowerUpType.HorizontalRocket)
            {
                rocketTile = _gridManager.tiles[x, gridY];
                break;
            }
        }
        if (rocketTile != null && rocketTile.gameObject != null && rocketTile.gameObject.activeInHierarchy)
        {
            rocketTile.transform.DOScale(1.2f, 0.15f).SetEase(Ease.OutBack);
            yield return new WaitForSeconds(0.15f);

            rocketTile.transform.DOShakePosition(0.18f, new Vector3(0.2f, 0, 0), 10, 90, false, true);
            yield return new WaitForSeconds(0.18f);

            if (_rocketTrialRight != null && _rocketTrialLeft != null)
            {
                GameObject rightRocket = Instantiate(_rocketTrialRight, rocketTile.transform.position, Quaternion.identity);
                float rightTargetX = rocketTile.transform.position.x + (_gridManager.GridX - rocketTile.GridPos.x + 2);
                rightRocket.transform.DOMoveX(rightTargetX, 0.25f);
                Destroy(rightRocket, 0.3f);

                GameObject leftRocket = Instantiate(_rocketTrialLeft, rocketTile.transform.position, Quaternion.identity);
                float leftTargetX = rocketTile.transform.position.x - rocketTile.GridPos.x - 2;
                leftRocket.transform.DOMoveX(leftTargetX, 0.25f);
                Destroy(leftRocket, 0.3f);
            }

            rocketTile.transform.DOScale(0f, 0.12f).SetEase(Ease.InBack);
            yield return new WaitForSeconds(0.12f);
        }
    }
    public void HorizontalRocket(int gridY)
    {
        StartCoroutine(HorizontalRocketRoutine(gridY));
    }

    public IEnumerator HorizontalRocketRoutine(int gridY)
    {
        yield return StartCoroutine(HorizontalRocketAnim(gridY));

        List<Tile> destroyTiles = new List<Tile>();
        for (int x = 0; x < _gridX; x++)
        {
            if (_gridManager.tiles[x, gridY] != null)
            {
                destroyTiles.Add(_gridManager.tiles[x, gridY]);
                _gridManager.tiles[x, gridY] = null;
            }
            Vector2Int pos = new Vector2Int(x, gridY);
            if (_obstacleManager.HasObstacle(pos))
                _obstacleManager.DestroyObstacleAt(pos);
        }
        ReturnToPool(destroyTiles);

        yield return new WaitForSeconds(0.1f);
        yield return StartCoroutine(_gridManager.RainDownRoutine());
        Debug.Log("horizontal rocket worked");
    }

    private void ReturnToPool(List<Tile> destroyTiles)
    {
        foreach (Tile tile in destroyTiles)
        {
            if (tile == null) continue;

            if (tile._PowerUpType != PowerUpType.None && tile._PowerUpType != PowerUpType.LightBomb)
            {
                if (!tile.IsTriggered)
                {
                    tile.PowerUpEffect(null);
                    tile.IsTriggered = true;
                }
            }
        }

        foreach (Tile tile in destroyTiles)
        {
            if (tile == null) continue;
            Vector2Int gridPos = tile.GridPos;

            if (_gridManager.IsValidGridPos(gridPos) && _gridManager.tiles[gridPos.x, gridPos.y] == tile)
            {
                _gridManager.tiles[gridPos.x, gridPos.y] = null;
            }

            DOTween.Kill(tile.transform);
            tile.transform.DOKill();
            SpriteRenderer spriteRenderer = tile.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                DOTween.Kill(spriteRenderer);
            }
            if (tile.gameObject != null)
            {
                _tilePool.ReturnTile(tile.gameObject);
                tile.IsTriggered = false;
                tile.gameObject.SetActive(false);
            }
        }
    }

}