using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Zenject;

public class Tile : MonoBehaviour
{
    [SerializeField] private int _tileId;
    [Inject] private GridManager _manager;
    [Inject] private PowerUpHandler _powerUphandler;

    private SpriteRenderer _spriteRenderer;
    private Color _originColor;
    private Vector3 _mouseStartPos;
    private Vector2Int _gridPos;
    private bool _isDragging = false;
    public bool IsBusy { get; set; } = false;
    public bool IsTriggered { get; set; } = false;

    [SerializeField] private float minSwapRight = 0.01f;
    [SerializeField] private float minSwapLeft = 0.002f;
    [SerializeField] private float minSwapVertical = 0.01f;
    [SerializeField] private float maxCross = 0.4f;

    public int TileId => _tileId;
    public Vector2Int GridPos => _gridPos;
    public PowerUpType _PowerUpType = PowerUpType.None;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _originColor = _spriteRenderer.color;
    }

    public void SetPowerUp(PowerUpType type)
    {
        _PowerUpType = type;
        Debug.Log("Power up type " + type);
    }

    public void SetPowerUpHandler(PowerUpHandler handler)
    {
        _powerUphandler = handler;
    }
    public void Triggered() => IsTriggered = true;
    public void PowerUpEffect(Tile targetTile)
    {
        if(IsTriggered) return;
        Triggered();

        switch (_PowerUpType)
        {
            case PowerUpType.HorizontalRocket:
                _powerUphandler.HorizontalRocket(GridPos.y);
                break;
            case PowerUpType.VerticalRocket:
                _powerUphandler.VerticalRocket(GridPos.x);
                break;
            case PowerUpType.Bomb:
                _powerUphandler.Bomb(_gridPos);
                break;
            case PowerUpType.LightBomb:
                if (targetTile != null && targetTile != this)
                    _powerUphandler.OnLightBombSwapped(this, targetTile);
                else
                    _powerUphandler.OnLightBombClicked(this);
                break;
            case PowerUpType.Propeller:
                _powerUphandler.Propellar(this);
                break;
        }
    }

    public void Initialize(int x, int y, GridManager manager)
    {
        _gridPos = new Vector2Int(x, y);
        _manager = manager;
    }

    public void SetTileId(int id) => _tileId = id;
    public void SetGridPos(Vector2Int gridPos) => _gridPos = gridPos;
    public Vector2Int GetGridPos() => _gridPos;

    private void OnMouseDown()
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = 10f;
        _mouseStartPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        SelectedTile();
        _isDragging = false;
    }

    private void OnMouseDrag()
    {
        if (IsBusy) return;

        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = 10f;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);

        float deltaX = mouseWorldPos.x - _mouseStartPos.x;
        float deltaY = mouseWorldPos.y - _mouseStartPos.y;

        bool passHorizontal =
            (deltaX > 0 && Mathf.Abs(deltaX) > minSwapRight) ||
            (deltaX < 0 && Mathf.Abs(deltaX) > minSwapLeft);

        bool passVertical = Mathf.Abs(deltaY) > minSwapVertical;

        if (!_isDragging && (passHorizontal || passVertical))
        {
            _isDragging = true;

            Vector2 direction = mouseWorldPos - _mouseStartPos;
            Vector2Int moveDirection = Vector2Int.zero;

            if (direction.x > 0 && direction.x > minSwapRight && Mathf.Abs(direction.y) < maxCross)
                moveDirection = Vector2Int.right;
            else if (direction.x < 0 && Mathf.Abs(direction.x) > minSwapLeft && Mathf.Abs(direction.y) < maxCross)
                moveDirection = Vector2Int.left;
            else if (Mathf.Abs(direction.y) > minSwapVertical && Mathf.Abs(direction.x) < maxCross)
                moveDirection = direction.y > 0 ? Vector2Int.up : Vector2Int.down;

            Vector2Int targetGridPos = _gridPos + moveDirection;

            if (IsValidSwap(targetGridPos))
            {
                _manager.SwapTile(_gridPos, targetGridPos);
            }
        }
    }

    private void OnMouseUp()
    {
        DeSelectedTile();

        if (!_isDragging && _PowerUpType != PowerUpType.None)
        {
            PowerUpEffect(this);
        }

        _isDragging = false;
    }

    private bool IsValidSwap(Vector2Int targetPos)
    {
        if (targetPos.x < 0 || targetPos.x >= _manager.GridX || targetPos.y < 0 || targetPos.y >= _manager.GridY)
            return false;

        if (_manager.tiles[targetPos.x, targetPos.y] == null)
            return false;

        if (_manager.obstacles != null && _manager.obstacles[targetPos.x, targetPos.y] != null)
            return false;

        return true;
    }

    private void SelectedTile()
    {
        _spriteRenderer.DOFade(0.7f, 0.1f);
    }

    private void DeSelectedTile()
    {
        _spriteRenderer.DOFade(1f, 0.2f);
    }
}

public enum PowerUpType
{
    None,
    HorizontalRocket,
    VerticalRocket,
    Bomb,
    LightBomb,
    Propeller,
}
