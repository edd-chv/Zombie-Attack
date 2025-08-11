using UnityEngine;

public class Sparkles : MonoBehaviour
{
    [SerializeField] private float _minDelay;
    [SerializeField] private float _maxDelay;
    [SerializeField] private FlashEffect _flashEffect;
    private ParticleSystem _ps;
    private float _time = 0;
    private float _random = 0;

    private void Awake()
    {
        _ps = GetComponent<ParticleSystem>();
    }

    void Update()
    {
        _time += Time.deltaTime;
        if (_time >= _random)
        {
            _flashEffect.StartEffect(_ps.totalTime);
            _random = Random.Range(_minDelay, _maxDelay);
            _ps.Play();
            _time = 0;
        }
    }
}