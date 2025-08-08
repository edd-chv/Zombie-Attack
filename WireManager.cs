using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace WiresMiniGame
{
    public static class ListExtensions
    {
        public static void Shuffle<T>(this List<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                T temp = list[i];
                int randomIndex = Random.Range(i, list.Count);
                list[i] = list[randomIndex];
                list[randomIndex] = temp;
            }
        }
    }

    public class WireManager : MonoBehaviour
    {
        [SerializeField] private UnityEvent _allWiresConnected = new UnityEvent();
        private class Wire
        {
            private GameObject _line;
            private GameObject _wanderingWire;
            private GameObject _anchor;
            private float StepZ
            {
                set
                {
                    SetPositionZ(_line.transform, value);
                    SetPositionZ(_wanderingWire.transform, value);
                    SetPositionZ(_anchor.transform, value);
                }
            }
            private Transform Parent
            {
                set
                {
                    SetParent(_line.transform, value);
                    SetParent(_wanderingWire.transform, value);
                    SetParent(_anchor.transform, value);
                }
            }
            private float MultipleScale
            {
                set
                {
                    SetScaleMultiple(_wanderingWire.transform, value);
                    SetScaleMultiple(_anchor.transform, value);
                }
            }
            public Wire(GameObject line, GameObject anchor, GameObject wanderingWire, Transform parent, float multipleScale, float stepZ)
            {
                _line = line;
                _anchor = anchor;
                _wanderingWire = wanderingWire;

                Parent = parent;
                MultipleScale = multipleScale;
                StepZ = stepZ;

                _line.transform.localScale = Vector3.one;
            }
            private void SetParent(Transform t, Transform parent)
            {
                t.SetParent(parent);
            }
            private void SetScaleMultiple(Transform t, float multiple)
            {
                t.localScale = Vector3.one * multiple;
            }
            private void SetPositionZ(Transform t, float stepZ)
            {
                t.position = new Vector3(t.position.x, t.position.y, t.position.z + stepZ);
            }
        }
        private class ColorWire
        {
            public Color color = new(255, 0, 205, 1.0f);
            public bool IsUsed = false;
        }
        private class SameColor
        {
            internal SameColor()
            {

            }

            public void SetColor(Color c)
            {

            }
        }

        [SerializeField] private ParticleSystem _sparklesConnected;
        [SerializeField] private float _flashScale;
        [SerializeField] private float _positionStepZ;
        [SerializeField] private GameObject _prefabLine;
        [SerializeField] private GameObject _prefabWanderingWire;
        [SerializeField] private GameObject _prefabAnchorWire;
        [SerializeField] private float _overlapRadius = 0.1f;
        [SerializeField] private RectTransform _canvasRectTr;
        [SerializeField] private Canvas _canvas;
        [SerializeField] private float _scaleWire;

        [SerializeField] private List<Transform> _wanderingTransform;
        [SerializeField] private List<Transform> _anchorTransform;
        [SerializeField] private List<Transform> _lineTransform;
        [SerializeField] private Color[] _color;

        private float _AnchorDeltaPointConnection;
        private ColorWire[] _colorWires;
        private int _countConnected = 0;
        private float _widthLine;

        public static bool CompareColor(Color color1, Color color2)
        {
            float _toleranceColorStatic = 0.01f;
            return
        Mathf.Abs(color1.r - color2.r) < _toleranceColorStatic &&
        Mathf.Abs(color1.g - color2.g) < _toleranceColorStatic &&
        Mathf.Abs(color1.b - color2.b) < _toleranceColorStatic &&
        Mathf.Abs(color1.a - color2.a) < _toleranceColorStatic;
        }

        private void OnValidate()
        {
            if (_color.Length < _wanderingTransform.Count)
            {
                Debug.LogWarning("Count not enough");
            }
            if (_wanderingTransform.Count == 0 || _anchorTransform.Count == 0 || _lineTransform.Count == 0)
            {
                Debug.LogError("List: 0");
            }
        }

        private void Awake()
        {
            Wire[] wire = new Wire[_wanderingTransform.Count];
            ColorWire cw = new();
            List<AnchorWire> _anchors = new();

            _widthLine = 1.3f / 100 * _scaleWire;
            _AnchorDeltaPointConnection = -1.8f / 100 * _scaleWire;

            _colorWires = new ColorWire[_color.Length];
            for (int i = 0; i < _colorWires.Length; i++)
            {
                _colorWires[i] = new()
                {
                    color = _color[i],
                    IsUsed = false
                };
            }

            float linePosStep = 0;
            for (int i = 0; i < _wanderingTransform.Count; i++)
            {
                GameObject pl = Instantiate(_prefabLine);
                GameObject paw = Instantiate(_prefabAnchorWire);
                GameObject pww = Instantiate(_prefabWanderingWire);

                pl.transform.position = _lineTransform[i].position;
                paw.transform.position = GetFreePosition(_anchorTransform);
                pww.transform.position = _wanderingTransform[i].position;

                wire[i] = new(pl, paw, pww, parent: _canvasRectTr, multipleScale:_scaleWire, stepZ: _positionStepZ);

                for (int j = 0; j < _colorWires.Length; j++)
                {
                    cw = _colorWires[Random.Range(0, _colorWires.Length)];
                    if (!cw.IsUsed)
                    {
                        cw.IsUsed = true;
                        j = _colorWires.Length;
                    }
                }

                AnchorWire aw = paw.GetComponent<AnchorWire>();
                aw.Init(_AnchorDeltaPointConnection, cw.color);
                _anchors.Add(aw);

                WanderingWireDrag wwd = pww.GetComponent<WanderingWireDrag>();
                wwd.Init(_canvasRectTr, _overlapRadius, cw.color, _sparklesConnected, _flashScale);

                WanderingWireFixator wwf = pww.GetComponent<WanderingWireFixator>();
                wwf.Init(pl.GetComponent<RectTransform>());

                LineConnection lc = pl.GetComponent<LineConnection>();
                lc.Init(pww.GetComponent<RectTransform>(), cw.color, _widthLine);

                linePosStep += _positionStepZ;
            }

            for (int i = 0; i < _anchors.Count; i++)
            {
                _anchors[i].WasConnect.AddListener(() =>
                {
                    _countConnected++;

                    if (_countConnected == _anchors.Count)
                    {
                        _allWiresConnected.Invoke();
                    }
                });
            }
        }

        private Vector3 GetFreePosition(List<Transform> t)
        {
            while (true)
            {
                int randIndex = Random.Range(0, t.Count);

                if (t[randIndex].gameObject.activeSelf)
                {
                    t[randIndex].gameObject.SetActive(false);
                    return t[randIndex].position;
                }

                if (t.Count == 0)
                {
                    Debug.Log("Transform list is empty");
                    return Vector3.zero;
                }
            }
        }
    }
}