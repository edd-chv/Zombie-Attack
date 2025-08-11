using System.Collections;
using UnityEngine;

public class FlashEffect : MonoBehaviour
{
    private SpriteRenderer _flashSprite;
    private Vector3 _defaultScale;
    private Transform _startParent;

    void Start()
    {
        _flashSprite = GetComponent<SpriteRenderer>();  
        _flashSprite.enabled = false;
        _defaultScale = transform.localScale;
        _startParent = transform.parent;
    }

    public void StartEffect(float duration)
    {
        StartCoroutine(Flash(duration, _defaultScale.magnitude, _startParent));
    }

    public void StartEffect(float duration, Transform parent, float scale = 1)
    {
        StartCoroutine(Flash(duration, scale, parent));
    }

    private IEnumerator Flash(float duration, float scale, Transform parent)
    {
        _flashSprite.enabled = true;
        transform.SetParent(parent);
        transform.localScale = Vector3.one * scale;

        yield return new WaitForSeconds(duration);

        _flashSprite.enabled = false;
        transform.SetParent(_startParent);
        transform.localScale = _defaultScale;
    }
}