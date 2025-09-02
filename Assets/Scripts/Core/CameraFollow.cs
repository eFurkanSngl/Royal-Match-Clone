using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private float _zoomMulti = 1.2f;
    [SerializeField] private Camera _mainCam;
    
    private void AdjustCamera(int gridWidth, int gridHeight)
    {
        if (_mainCam == null) return;

        float aspect = (float)Screen.width / Screen.height;
        float mainCamSizeWidth = (gridWidth / aspect) / 2;
        float mainCamSizeHeight = gridHeight / 2;

        _mainCam.orthographicSize = Mathf.Max(mainCamSizeWidth, mainCamSizeHeight)* _zoomMulti;

        float centerX = gridWidth / 2.3f;
        float centerY = gridHeight / 2.2f;
        float centerZ = -10f;
        _mainCam.transform.position = new Vector3(centerX, centerY,centerZ);
    }

    private void OnEnable()
    {
        GridManager.GridManagerEvent += AdjustCamera;  
    }

    private void OnDisable()
    {
        GridManager.GridManagerEvent -= AdjustCamera;
    }
}
