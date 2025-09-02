using DG.Tweening;
using UnityEngine;
using Zenject;
using System.Collections;

public class PowerUp : MonoBehaviour
{
    [Header("Power-Up Prefabs")]
    [SerializeField] private GameObject _horizontalPowerUpPrefab;
    [SerializeField] private GameObject _verticalPowerUpPrefab;
    [SerializeField] private GameObject _bombPowerUpPrefab;
    [SerializeField] private GameObject _lightBombPowerUpPrefab;
    [SerializeField] private GameObject _propellerPowerUpPrefab;

    [Inject] AnimationHandler _animationHandler;
    [Inject] GridManager _gridManager;
    [Inject] TilePool _tilePool;

    GameObject prefabToSpawn = null;
    PowerUpType type = PowerUpType.None;

    public void CreatePowerUpTile(Tile originTile, MatchShapeType shapeType, Vector2Int swapDir, GridManager gridManager)
    {
        if (shapeType == MatchShapeType.FourMatch)
        {
            if (swapDir.x != 0)
            {
                prefabToSpawn = _horizontalPowerUpPrefab;
                type = PowerUpType.HorizontalRocket;
            }
            else
            {
                prefabToSpawn = _verticalPowerUpPrefab;
                type = PowerUpType.VerticalRocket;
            }
        }
        else if (shapeType == MatchShapeType.FiveMatch)
        {
            prefabToSpawn = _lightBombPowerUpPrefab;
            type = PowerUpType.LightBomb;
        }
        else if (shapeType == MatchShapeType.LShape)
        {
            prefabToSpawn = _bombPowerUpPrefab;
            type = PowerUpType.Bomb;
        }
        else if (shapeType == MatchShapeType.TShape)
        {
            prefabToSpawn = _bombPowerUpPrefab;
            type = PowerUpType.Bomb;
        }
        else if (shapeType == MatchShapeType.SquareShape)
        {
            prefabToSpawn = _propellerPowerUpPrefab;
            type = PowerUpType.Propeller;
        }
        else if (shapeType == MatchShapeType.ThreeMatch)
        {
            type = PowerUpType.None;
        }

        if (prefabToSpawn != null)
        {
            CreatePowerUp(originTile);
        }
    }

    private void CreatePowerUp(Tile originTile)
    {
        if (originTile == null || originTile.gameObject == null) return;

        DOTween.Kill(originTile.transform);
        originTile.transform.DOKill();
        Vector2Int gridPos = originTile.GridPos;

        if (_gridManager.tiles[gridPos.x, gridPos.y] != null && _gridManager.tiles[gridPos.x, gridPos.y] == originTile)
        {
            _gridManager.tiles[gridPos.x, gridPos.y] = null;
        }

        if (prefabToSpawn != null)
        {
            Vector3 spawnPos = new Vector3(gridPos.x, gridPos.y, 0f);
            GameObject newTileObj = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);

            newTileObj.transform.DOScale(0.8f, 0.3f).SetEase(Ease.OutBack);
            newTileObj.transform.DORotate(new Vector3(0, 0, 360), 0.6f, RotateMode.FastBeyond360);

            Tile newTile = newTileObj.GetComponent<Tile>();
            newTile.SetPowerUp(type);
            newTile.Initialize(gridPos.x, gridPos.y, _gridManager);
            newTile.SetPowerUpHandler(_gridManager.GetPowerUpHandler());
            _gridManager.tiles[gridPos.x, gridPos.y] = newTile;

            originTile.transform.DOKill();
            Destroy(originTile.gameObject);
            //_tilePool.ReturnTile(originTile.gameObject);
            //originTile.gameObject.SetActive(false);
        }
    }
}