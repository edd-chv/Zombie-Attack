using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
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
    public class ColorWire
    {
        public Color color = new(255, 0, 205, 1.0f);
        public bool IsFree = true;

        public ColorWire() { }
        public ColorWire(Color c)
        {
            color = c;
            IsFree = true;
        }
        public static void BusyReset(ref ColorWire[] cw)
        {
            for (int i = 0; i < cw.Length; i++)
            {
                cw[i].IsFree = true;
                //Debug.Log($"BusyReset: Color: {cw[i].color}, IsFree: {cw[i].IsFree}");
            }
        }
        public static ColorWire GetRandomColor(ref ColorWire[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                int index = Random.Range(0, array.Length);
                if (array[index].IsFree)
                {
                    array[index].IsFree = false;
                    return array[index];
                }
            }
            return new();
        }
    }

    public class WireManager : MonoBehaviour
    {
        public ColorWireVisualize _cWV;
        [SerializeField] private UnityEvent _allWiresConnected = new UnityEvent();
        private class Wire
        {
            private Transform _lineTr;
            private Transform _wanderingWireTr;
            private Transform _anchorTr;

            private LineConnection _line;
            private WanderingWireDrag _wanderingWire;
            private AnchorWire _anchor;

            internal float _stepZ
            {
                set
                {
                    SetPositionZ(_lineTr, value);
                    SetPositionZ(_wanderingWireTr, value);
                    SetPositionZ(_anchorTr, value);
                }
            }
            internal Transform _parent
            {
                set
                {
                    SetParent(_lineTr, value);
                    SetParent(_wanderingWireTr, value);
                    SetParent(_anchorTr, value);
                }
            }
            internal float _multipleScale
            {
                set
                {
                    SetScaleMultiple(_wanderingWireTr, value);
                    SetScaleMultiple(_anchorTr, value);
                    _lineTr.localScale = Vector3.one;
                }
            }
            internal Color _updateColor
            {
                set 
                {
                    _line.SingleColor(value);
                    _anchor.Color = value;
                    _wanderingWire.UpdateColor(value);
                }
            }
            public Wire(LineConnection line, AnchorWire anchor, WanderingWireDrag wanderingWire)
            {
                _line = line;
                _anchor = anchor;
                _wanderingWire = wanderingWire;

                _lineTr = line.transform;
                _anchorTr = anchor.transform;
                _wanderingWireTr = wanderingWire.transform;
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

        private Wire[] _wire;
        private WanderingWireDrag[] _wanderingWireDrag;
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
            _colorWires = new ColorWire[_color.Length];
            for (int i = 0; i < _colorWires.Length; i++)
            {
                _colorWires[i] = new(_color[i]);
            }

            _cWV.Init(ref _colorWires);

            float linePosStep = 0;

            _wire = new Wire[_wanderingTransform.Count];
            _wanderingWireDrag = new WanderingWireDrag[_wanderingTransform.Count];

            _widthLine = 1.3f / 100 * _scaleWire;
            _AnchorDeltaPointConnection = -1.8f / 100 * _scaleWire;

            for (int i = 0; i < _wanderingTransform.Count; i++)
            {
                GameObject pl = Instantiate(_prefabLine);
                pl.name = $"line {i}";

                GameObject paw = Instantiate(_prefabAnchorWire);
                paw.name = $"anchor {i}";

                GameObject pww = Instantiate(_prefabWanderingWire);
                pww.name = $"wandering {i}";

                pl.transform.position = SetPosition(_lineTransform[i].position);
                paw.transform.position = SetPosition(GetFreePositionRandom(_anchorTransform));
                pww.transform.position = SetPosition(_wanderingTransform[i].position);

                AnchorWire aw = paw.GetComponent<AnchorWire>();
                WanderingWireDrag wwd = pww.GetComponent<WanderingWireDrag>();
                WanderingWireFixator wwf = pww.GetComponent<WanderingWireFixator>();
                LineConnection lc = pl.GetComponent<LineConnection>();

                aw.Init(_AnchorDeltaPointConnection);
                aw.WasConnect.AddListener(() => 
                {
                    _countConnected++;

                    if (_countConnected == _anchorTransform.Count)
                    {
                        _allWiresConnected.Invoke();
                        _countConnected = 0;
                    }
                });

                wwd.Init(_canvasRectTr, _overlapRadius, _sparklesConnected, _flashScale);
                _wanderingWireDrag[i] = wwd;

                wwf.Init(pl.GetComponent<RectTransform>());

                lc.Init(pww.GetComponent<RectTransform>(), _widthLine);

                _wire[i] = new(lc, aw, wwd)
                {
                    _parent = _canvasRectTr,
                    _multipleScale = _scaleWire,
                    _updateColor = ColorWire.GetRandomColor(ref _colorWires).color
                };

                linePosStep += _positionStepZ;
            }

            Vector3 SetPosition(Vector3 pos)
            {
                return pos + new Vector3(0, 0, linePosStep);
            }
        }

        public void ResetWires()
        {
            ColorWire.BusyReset(ref _colorWires);

            for (int i = 0; i < _wanderingWireDrag.Length; i++)
            {
                _wanderingWireDrag[i].ResetPosition();

                ColorWire c = ColorWire.GetRandomColor(ref _colorWires);
                _wire[i]._updateColor = c.color;
            }
        }

        private Vector3 GetFreePositionRandom(List<Transform> t)
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