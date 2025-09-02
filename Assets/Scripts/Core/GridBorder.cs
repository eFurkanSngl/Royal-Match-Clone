using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GridBorder : MonoBehaviour
{
    [Header("Grid Border Settings")]
    [SerializeField] private GameObject _borderTop;
    [SerializeField] private GameObject _borderRight;
    [SerializeField] private GameObject _borderLeft;
    [SerializeField] private GameObject _borderBottom;

    private List<GameObject> _borderList = new List<GameObject>();

    private void AdjustGridBorder(int gridX , int gridY)
    {
        foreach(GameObject obj in _borderList)
        {
            Destroy(obj);
        }

        _borderList.Clear();

        float startX = transform.position.x;
        float startY = transform.position.y;

        for(int i = 0; i< gridX ; i++)
        {
            GameObject borderTop = Instantiate(_borderTop, new Vector3(startX + i, startY+(gridY - 0.2f)),Quaternion.identity,transform);
            _borderList.Add(borderTop);
            borderTop.transform.Rotate(0, 0, 90f);

            GameObject borderBottom = Instantiate(_borderBottom, new Vector3(startX + i ,startY -0.81f),Quaternion.identity,transform);
            _borderList.Add(borderBottom);
            borderBottom.transform.Rotate(0, 0, 90f);


        }

        for (int i = 0; i < gridY; i++)
        {
            GameObject borderLeft  = Instantiate(_borderLeft, new Vector3(startX - 0.8f , startY+i,0),Quaternion.identity,transform); 
            _borderList.Add(borderLeft); ;

            GameObject borderRight = Instantiate(_borderRight, new Vector3(startY + gridX - 0.2f, startY + i ,0),Quaternion.identity,transform);
            _borderList.Add(borderRight);
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

    private void RegisterEvents() => GridManager.GridManagerEvent += AdjustGridBorder;
    private void UnRegisterEvents()=> GridManager.GridManagerEvent -= AdjustGridBorder;
}
