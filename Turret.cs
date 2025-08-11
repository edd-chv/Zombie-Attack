using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Turret : DefensiveStructure
{
    [SerializeField] private ShootingMode _shootingMode;
    [SerializeField] private float _normalRateFire;
    [SerializeField] private float _bulletSpeed;
    [SerializeField] private float _damage;
    [SerializeField] private BulletPool _bPool;

    [Space(10)]
    [Header("DETECTOR")]
    [Space(5)]
    [SerializeField] private Detector _detector;
    [SerializeField] private Enemy _currentTarget = null;

    [Space(10)]
    [Header("TOWER")]
    [Space(5)]
    [SerializeField] private TowerTurret _towerTurret;
    [SerializeField] private Shutter _shutter;

    [Space(10)]
    [Header("SOUND")]
    [Space(5)]
    [SerializeField] private AudioSource _shotSound;
    [SerializeField] private float _minPitchShotSound;
    [SerializeField] private float _maxPitchShotSound;

    [Space(10)]
    [Header("CHANCE FIALURE")]
    [Space(5)]
    [SerializeField] private FailureTurret _failureTurret;

    private Coroutine _shoot;

    private void OnEnable()
    {
        _detector._updateTagret.AddListener(() => {
            _detector.GetTarget(ref _currentTarget); 
        });
    }

    private void OnDisable()
    {
        _detector._updateTagret.RemoveAllListeners();
    }

    void Update()
    {
        if (_currentTarget == null)
        {
            _towerTurret.DefaultPose();
        }
        else
        {
            if (!_failureTurret.TryFailure(_shootingMode))
            {
                _towerTurret.WatchTarget(_currentTarget);
                _shoot ??= StartCoroutine(Shoot());
            }
            else
            {
                if (_shoot != null)
                {
                    StopCoroutine(_shoot);
                    _shoot = null;
                }
            }
        }
    }

    private IEnumerator Shoot()
    {
        while (_currentTarget != null)
        {
            _shotSound.pitch = Random.Range(_minPitchShotSound, _maxPitchShotSound);
            _shotSound.Play();
            _shutter.Sliding();
            _bPool.StartReleaseBullet(_bulletSpeed, _damage, _currentTarget);
            
            yield return new WaitForSecondsRealtime(_normalRateFire / (int)_shootingMode);
        }
        _detector._updateTagret.Invoke();
        _shoot = null;
    }
}