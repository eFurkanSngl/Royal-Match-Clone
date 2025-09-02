using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TerrainUtils;
using Zenject;

public class TilePool : MonoBehaviour
{
    [SerializeField] private GameObject[] _tilePrefabs;
    [SerializeField] private int _poolSize;
    [Inject] private GridManager _gridManager;
    private Transform _transform;
    public GameObject[] tilePrefabs => _tilePrefabs;
    private Dictionary<int, Queue<GameObject>> _tilePools;
    private void Awake()
    {
        _tilePools = new Dictionary<int, Queue<GameObject>>();
        _transform = _gridManager.transform;

        for (int i = 0; i < _tilePrefabs.Length; i++)
        {
            var queue = new Queue<GameObject>();
            for (int j = 0; j < _poolSize; j++)
            {
                GameObject obj = Instantiate(_tilePrefabs[i]);
                obj.SetActive(false);
                if (_transform != null)
                {
                    obj.transform.SetParent(_transform, false);

                }
                else
                {
                    obj.transform.SetParent(transform, false);

                }
                queue.Enqueue(obj);
            }
            _tilePools.Add(i, queue);
        }
    }
    public GameObject GetTile(int id)
    {
        GameObject tile;

        if (_tilePools.ContainsKey(id) && _tilePools[id].Count > 0)
        {
            tile = _tilePools[id].Dequeue();
            if (tile == null) return tile;
            tile.gameObject.SetActive(true);
        }
        else
        {
            tile = Instantiate(_tilePrefabs[id]);
        }
        tile.gameObject.SetActive(true);
        ResetTile(tile);
        return tile;
    }

    public void ReturnTile(GameObject tile)
    {
        if (tile == null) return;

        int id = tile.GetComponent<Tile>().TileId;
        tile.SetActive(false);

        ResetTileSr(tile);

        if (_tilePools.ContainsKey(id))
        {
            _tilePools[id].Enqueue(tile);
        }
        else
        {
            Debug.LogWarning("ID is unknown");
        }
    }

    private void ResetTile(GameObject tile)
    {
        Vector3 startPos = new Vector3(1f, 1f, 1f);
        tile.transform.localScale = startPos;
        tile.transform.rotation = Quaternion.identity;

        if (_transform != null && tile.transform.parent != _transform)
        {
            tile.transform.SetParent(_transform, false);
        }

        ResetTileSr(tile);
    }
    private void ResetTileSr(GameObject tile)
    {
        SpriteRenderer sr = tile.GetComponent<SpriteRenderer>();
        Color currentColor = sr.color;

        if (sr != null)
        {
            sr.color = new Color(currentColor.r, currentColor.g, currentColor.b, 1f);
        }

        tile.transform.DOKill();

    }
}