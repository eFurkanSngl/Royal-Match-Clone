using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public Vector2Int GridPos { get; set;}
    public bool IsDestroy { get;private set; }

    public void Initialize(Vector2Int gridPos)
    {
        GridPos = gridPos;
    }

    private void OnMouseDown()
    {
        transform.DOShakePosition(0.5f, 0.2f);
           
    }

   
}
