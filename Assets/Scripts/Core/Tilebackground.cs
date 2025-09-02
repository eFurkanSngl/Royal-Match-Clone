using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class Tilebackground : MonoBehaviour
{
    [Inject] private GridManager _manager;
    private Transform _transform;    

    [SerializeField] private GameObject _tileBackground;
    private GameObject[,] _bgTiles;

    private void Awake()
    {
        _transform = _manager.transform;   
    }
    private void CreateTileBackground(int gridX, int gridY)
    {
        _bgTiles = new GameObject[gridX, gridY];
        for(int i = 0; i < gridX; i++)
        {
            for(int j = 0; j < gridY; j++)
            {
                Vector3 pos = new Vector3(transform.position.x + i, transform.position.y + j, 0);
                GameObject tileBg = Instantiate(_tileBackground,pos,Quaternion.identity); 
                _bgTiles[i,j] = tileBg;
                tileBg.transform.parent = _transform;
            }
        }
    }

    private void OnEnable()
    {
        RegisterEvents();
    }

    private void OnDisable()
    {
        UnRegisterEvents();
    }

    private void RegisterEvents() => GridManager.GridManagerEvent += CreateTileBackground;
    private void UnRegisterEvents () => GridManager.GridManagerEvent -= CreateTileBackground;
}
