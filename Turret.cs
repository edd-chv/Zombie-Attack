using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;
using WiresMiniGame;

public enum ShootingMode : int
{
    normal = 1,
    fast = 2,
    superFast = 3
}

public class Turret : DefensiveStructure
{
    //private Coroutine _attack = null;
    [Space(10)]
    [Header("ATTACKa")]
    [Space(5)]
    private float _defaultAttackSpeed;
    [SerializeField] private float _damage;
    [SerializeField] private float _attackSpeed;
    [SerializeField] private ShootingMode _shootingMode;
    [SerializeField] private Transform _bullet;
    [SerializeField] private float _bulletSpeed;
    private Vector3 _bulletDefaultPosition;

    [Space(10)]
    [Header("DETECTOR")]
    [Space(5)]
    [SerializeField] private float _radiusDetectionTarget;
    [SerializeField] private SphereCollider _radiusCollider;
    [SerializeField] private EnemiesDetector _detector;
    [SerializeField] private Enemy _currentTarget = null;
    [SerializeField] private float _watchTargetSpeed;

    [Space(10)]
    [Header("MUZZLE")]
    [Space(5)]
    [SerializeField] private AnimationCurve _animMuzzle;
    [SerializeField] private ParticleSystem _muzzleFire;
    [SerializeField] private Transform _muzzle;
    [SerializeField] private ParticleSystem _smokeTurretFailure;
    private Vector3 _defaultPosMuzzle;
    [SerializeField] private Transform _body;
    [SerializeField] private float _bodyFailureXdegress;
    [SerializeField] private float _durationRotationInFailure;
    private Quaternion _defaultRotBody;
    [SerializeField] private Transform _rotaryDrive;
    private Quaternion _defaultRotRotaryDrive;

    [Space(10)]
    [Header("SOUND")]
    [Space(5)]
    [SerializeField] private AudioSource _shotSound;
    [SerializeField] private float _minPitchShotSound;
    [SerializeField] private float _maxPitchShotSound;

    [Space(10)]
    [Header("CHANCE FIALURE")]
    [Space(5)]
    [SerializeField] private float _chanceFailureNormalMode;
    [SerializeField] private float _chanceFailureFastMode;
    [SerializeField] private float _chanceFailureSuperFastMode;
    [SerializeField] private float _calculationFailureSeconds;

    [SerializeField] private bool _isNeedRepaired = false;

    private void Awake()
    {
    }

    void Start()
    {
        _bulletDefaultPosition = _bullet.localPosition;
        _defaultPosMuzzle = _muzzle.position;
        _defaultRotBody = _body.rotation;
        _defaultRotRotaryDrive = _rotaryDrive.rotation;
        _radiusCollider.radius = _radiusDetectionTarget;
        _defaultAttackSpeed = _attackSpeed;
    }

    void Update()
    {
        AttackSpeedCalculation();
        WatchTarget();

        if (_currentTarget == null && !_isNeedRepaired)
        {
            _currentTarget = _detector?.GetCurrentTarget();
            StartCoroutine(Attack(_currentTarget));
            StartCoroutine(FailureCalculation());
        }
    }

    private void AttackSpeedCalculation()
    {
        if (_shootingMode == ShootingMode.normal)
        {
            _attackSpeed = _defaultAttackSpeed / (int)ShootingMode.normal;
        }
        if (_shootingMode == ShootingMode.fast)
        {
            _attackSpeed = _defaultAttackSpeed / (int)ShootingMode.fast;
        }
        if (_shootingMode == ShootingMode.superFast)
        {
            _attackSpeed = _defaultAttackSpeed / (int)ShootingMode.superFast;
        }
    }

    private void WatchTarget()
    {
        if (!_isNeedRepaired)
        {
            Debug.Log("watch");
            if (_currentTarget != null)
            {
                Vector3 directionVectorBody = _currentTarget.transform.position - _body.position;
                Vector3 rotationBody = Quaternion.LookRotation(directionVectorBody).eulerAngles;
                float _axisXWatchTarget = _body.rotation.eulerAngles.x;
                _axisXWatchTarget = Mathf.MoveTowardsAngle(_axisXWatchTarget, rotationBody.x, _watchTargetSpeed * Time.deltaTime);
                _body.rotation = Quaternion.Euler(_axisXWatchTarget, _body.rotation.eulerAngles.y, _body.rotation.eulerAngles.z);

                Vector3 directionVectorRotary = _currentTarget.transform.position - _rotaryDrive.position;
                Vector3 rotationRotary = Quaternion.LookRotation(directionVectorRotary).eulerAngles;
                float _axisYWatchTarget = _rotaryDrive.rotation.eulerAngles.y;
                _axisYWatchTarget = Mathf.MoveTowardsAngle(_axisYWatchTarget, rotationRotary.y, _watchTargetSpeed * Time.deltaTime);
                _rotaryDrive.rotation = Quaternion.Euler(_rotaryDrive.rotation.eulerAngles.x, _axisYWatchTarget, _rotaryDrive.rotation.eulerAngles.z);
            }
            else
            {
                float bodyAxisX = _body.rotation.eulerAngles.x;
                bodyAxisX = Mathf.MoveTowardsAngle(_body.rotation.eulerAngles.x, _defaultRotBody.eulerAngles.x, _watchTargetSpeed * Time.deltaTime);
                _body.rotation = Quaternion.Euler(bodyAxisX, _body.rotation.eulerAngles.y, _body.rotation.eulerAngles.z);

                float rotaryAxisY = _rotaryDrive.rotation.eulerAngles.y;
                rotaryAxisY = Mathf.MoveTowardsAngle(_rotaryDrive.rotation.eulerAngles.y, _defaultRotRotaryDrive.eulerAngles.y, _watchTargetSpeed * Time.deltaTime);
                _rotaryDrive.rotation = Quaternion.Euler(_rotaryDrive.rotation.eulerAngles.x, rotaryAxisY, _rotaryDrive.rotation.eulerAngles.z);
            }
        }
    }

    private IEnumerator Attack(Enemy z)
    {
        while (_currentTarget != null)
        {
            yield return new WaitForSeconds(_attackSpeed);
            _shotSound.pitch = UnityEngine.Random.Range(_minPitchShotSound, _maxPitchShotSound);
            _shotSound.Play();
            StartCoroutine(BulletMove());
            if (_currentTarget != null)
            {
                _currentTarget.TakeDamage(_damage);
            }
            StartCoroutine(MuzzleAnimation());
            _muzzleFire.Play();
        }
    }

    private IEnumerator MuzzleAnimation()
    {
        float time = 0;
        while (time <= _animMuzzle.keys[^1].time)
        {
            time += Time.deltaTime;
            _muzzle.localPosition = new(_muzzle.localPosition.x, _muzzle.localPosition.y, _animMuzzle.Evaluate(time));
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator BulletMove()
    {
        _bullet.localPosition = _bulletDefaultPosition;
        while (_currentTarget != null)
        {
            _bullet.position = Vector3.MoveTowards(_bullet.position, _currentTarget.transform.position, _bulletSpeed);
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator FailureCalculation()
    {
        float chance = 0;
        while (!_isNeedRepaired)
        {
            yield return new WaitForSeconds(_calculationFailureSeconds);
            chance = UnityEngine.Random.Range(0.0f, 1.0f);
            if (chance <= _chanceFailureNormalMode && _shootingMode == ShootingMode.normal)
            {
                Debug.Log("Failure/normal");
                Failure();

            }
            if (chance <= _chanceFailureFastMode && _shootingMode == ShootingMode.fast)
            {
                Debug.Log("Failure/fast");
                Failure();

            }
            if (chance <= _chanceFailureSuperFastMode && _shootingMode == ShootingMode.superFast)
            {
                Debug.Log("Failure/superFast");
                Failure();
            }
        }

        void Failure()
        {
            StopCoroutine("Attack");
            _currentTarget = null;
            _isNeedRepaired = true;
            _smokeTurretFailure.Play();
            StartCoroutine(RotateXOverTime(_bodyFailureXdegress, _durationRotationInFailure));
        }
    }


    IEnumerator RotateXOverTime(float targetAngle, float duration)
    {
        Quaternion startRotation = _body.localRotation;
        float startXAngle = startRotation.eulerAngles.x;
        float endXAngle = targetAngle;

        endXAngle = NormalizeAngle(endXAngle);
        startXAngle = NormalizeAngle(startXAngle);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            float currentX = Mathf.Lerp(startXAngle, endXAngle, t);

            _body.localRotation = Quaternion.Euler(currentX, startRotation.eulerAngles.y, startRotation.eulerAngles.z);

            yield return null;
        }
        _body.localRotation = Quaternion.Euler(endXAngle, _body.localEulerAngles.y, _body.localEulerAngles.z);
    }

    float NormalizeAngle(float angle)
    {
        angle = angle % 360;
        if (angle > 180) angle -= 360;
        else if (angle < -180) angle += 360;
        return angle;
    }
}