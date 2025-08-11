using UnityEngine;

namespace WiresMiniGame
{
    public class LineConnection : MonoBehaviour
    {
        private RectTransform _lineRectTr;
        private RectTransform _wireRectTr;
        internal LineRenderer _line;

        void Start()
        {
            _lineRectTr = GetComponent<RectTransform>();
        }

        void Update()
        {
            if (_line == null)
            {
                Debug.Log("line is null");
            }
            if (_wireRectTr == null)
            {
                Debug.Log("wireRectTr is null");
            }
            if (_lineRectTr == null)
            {
                Debug.Log("LineRectTr is null");
            }
            _line.SetPosition(_line.positionCount - 1, _wireRectTr.localPosition - _lineRectTr.localPosition);
        }

        internal void Init(RectTransform wireRectTr, float width)
        {
            _wireRectTr = wireRectTr;

            _line = GetComponent<LineRenderer>();
            _line.widthMultiplier = width;
        }

        internal void SingleColor(Color c)
        {
            _line.startColor = c;
            _line.endColor = c;
        }
    }
}