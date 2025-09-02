using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridBackground : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _sr;
    [SerializeField] private float _padding = 0.5f;

    private void Start()
    {
        _sr = GetComponent<SpriteRenderer>();
    }

    private void AdjustGridBackground(int gridX , int gridY)
    {
        if(_sr == null)
        {
            Debug.Log("Spirte empty");
        }

        transform.position = new Vector3(gridX / 2f - 0.5f, gridY / 2f - _padding, 1f);
        _sr.size = new Vector3(gridX + 0.5f, gridY + 0.5f, 1f);
    }

    private void OnEnable() => RegisterEvents();
   
    private void OnDisable() => UnRegisterEvents();


    private void RegisterEvents() => GridManager.GridManagerEvent += AdjustGridBackground;
    private void UnRegisterEvents() => GridManager.GridManagerEvent -= AdjustGridBackground;
}
