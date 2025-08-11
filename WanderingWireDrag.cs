using UnityEngine;
using UnityEngine.EventSystems;

namespace WiresMiniGame
{
    public class WanderingWireDrag : MonoBehaviour, IDragHandler, IEndDragHandler
    {
        [SerializeField] private FlashEffect _flashEffect;
        [SerializeField] private SpriteRenderer _insulationSprite;
        private float _flashScale;
        private ParticleSystem _sparkles;
        private bool _isDrag;
        private bool _isConnected;
        private float _radius = 0.1f;
        private Vector3 _startPosition;
        private Collider2D _thisCollider;
        private Collider2D[] _hits;
        private RectTransform _canvasRectTr;
        private RectTransform _rect;

        private void OnEnable()
        {
            EnableChild();
        }

        internal void Init(RectTransform canvasRectTr, float radius, ParticleSystem sparkles, float flashScale)
        {
            _thisCollider = GetComponent<Collider2D>();
            _rect = GetComponent<RectTransform>();

            _canvasRectTr = canvasRectTr;
            _flashScale = flashScale;
            _sparkles = sparkles;
            _radius = radius;
        }

        void Start()
        {
            _startPosition = _rect.anchoredPosition3D;
        }

        void Update()
        {
            if (_isDrag && !_isConnected)
            {
                _hits = Physics2D.OverlapCircleAll(GetMouseWorldPosition(), _radius);
                foreach (Collider2D hit in _hits)
                {
                    if (hit != null && hit != _thisCollider)
                    {
                        if (hit.TryGetComponent<AnchorWire>(out var anchorWire))
                        {
                            if (_isConnected = WireManager.CompareColor(_insulationSprite.color, anchorWire.Color))
                            {
                                Debug.Log($"name wan: {gameObject.name}. insul: {_insulationSprite.color}. anchor: {anchorWire.Color}");
                                Debug.Log("match");
                                transform.position = anchorWire.GetPointConnection();
                                _sparkles.transform.position = anchorWire.GetPointConnection();
                                _sparkles.Play();
                                _flashEffect.StartEffect(_sparkles.totalTime, _canvasRectTr.transform, _flashScale);
                                anchorWire.WasConnect.Invoke();
                                DisableChild();
                            }
                        }
                    }
                }
            }
        }

        private void OnMouseDown()
        {
            transform.position = GetMouseWorldPosition();
        }

        private Vector3 GetMouseWorldPosition()
        {
            Vector3 mouseScreenPos = Input.mousePosition;
            mouseScreenPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
            mouseScreenPos.z = transform.position.z;
            return mouseScreenPos;
        }

        public void OnDrag(PointerEventData eventData)
        {
            _isDrag = true;
            if (!_isConnected)
            {
                transform.position = GetMouseWorldPosition();
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _isDrag = false;
            _hits = null;
            if (!_isConnected)
            {
                _rect.anchoredPosition3D = _startPosition;
            }
        }

        private void DisableChild()
        {
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(false);
            }
        }

        private void EnableChild()
        {
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(true);
            }
        }

        internal void UpdateColor(Color c)
        {
            _insulationSprite.color = c;
        }

        internal void ResetPosition()
        {
            _isConnected = false;
            EnableChild();
            _rect.anchoredPosition3D = _startPosition;
        }
    }
}