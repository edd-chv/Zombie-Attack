using UnityEngine;
using UnityEngine.Events;

namespace WiresMiniGame
{
    public class AnchorWire : MonoBehaviour
    {
        internal UnityEvent WasConnect = new UnityEvent();
        private SpriteRenderer _spriteRenderer;

        private void OnEnable()
        {
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(true);
            }
        }

        private void Awake()
        {
            WasConnect.AddListener(() =>
            {
                foreach (Transform child in transform)
                {
                    child.gameObject.SetActive(false);
                }
            });
        }

        public Color Color
        {
            get { return _spriteRenderer.color; }
            set { _spriteRenderer.color = value; }
        }

        private float _deltaPointConnectionX;

        internal void Init(float delta)
        {
            _deltaPointConnectionX = delta;
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public Vector3 GetPointConnection()
        {
            return new(transform.position.x + _deltaPointConnectionX, transform.position.y, transform.position.z);
        }
    }
}