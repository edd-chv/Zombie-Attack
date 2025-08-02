using System.Collections.Generic;
using UnityEngine;

public class Detector : MonoBehaviour
{
    [SerializeField] private float _radius;
    [SerializeField] private SphereCollider _sphCollider;
    [SerializeField] private List<GameObject> _enemies;

    private void Awake()
    {
        _sphCollider.radius = _radius;
        _enemies = new List<GameObject>();
    }

    public void GetTarget<T>(ref T target)
    {
        if (_enemies.Count > 0)
        {
            for (int i = 0; i < _enemies.Count; i++)
            {
                if (_enemies[i] == null)
                {
                    _enemies.Remove(_enemies[i]);
                }
                else if (_enemies[i].TryGetComponent(out T t))
                {
                    target = t;
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        _enemies.Add(other.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        for (int i = 0; i < _enemies.Count; i++)
        {
            if (other.gameObject == _enemies[i])
            {
                _enemies.Remove(_enemies[i]);
            }
        }
    }

    private void Update()
    {
        
    }
}
