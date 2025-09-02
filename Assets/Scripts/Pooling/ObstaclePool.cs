using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstaclePool : MonoBehaviour
{
    [SerializeField] private GameObject _obstacle;
    [SerializeField] private int _obstacleCount = 20;
    private Queue<GameObject> _obstacleList = new Queue<GameObject>();
    private void Awake()
    {
      for(int i = 0; i < _obstacleCount; i++)
        {
            GameObject obs = Instantiate(_obstacle,transform);
            obs.SetActive(false);
            _obstacleList.Enqueue(obs); 
        }
    }

    public GameObject GetObstacle()
    {
       if( _obstacleList.Count > 0)
        {
            GameObject obs = _obstacleList.Dequeue();
            obs.SetActive(true);
            return obs;
        }
        else
        {
            GameObject newObs = Instantiate(_obstacle);
            return newObs;
        }
    }

    public void ReturnObstacle(GameObject obj)
    {
        _obstacleList.Enqueue(obj);
        obj.SetActive(false);
        obj.transform.DOKill();
    }

}
