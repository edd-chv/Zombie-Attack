using UnityEngine;

namespace WiresMiniGame
{
    public class WanderingWireFixator : MonoBehaviour
    {
        private RectTransform LineRectTr;

        internal void Init(RectTransform lineRectTr)
        {
            LineRectTr = lineRectTr;
        }

        void Update()
        {
            Rotation();
        }
        private void Rotation()
        {
            if (LineRectTr != null)
            {
                Vector2 directionToTarget = (transform.localPosition - LineRectTr.transform.localPosition);
                Vector2 perpendicular = new(-directionToTarget.y, directionToTarget.x);
                Vector3 targetRotation = Quaternion.FromToRotation(Vector2.up, perpendicular.normalized).eulerAngles;
                transform.rotation = Quaternion.Euler(targetRotation);
            }
        }
    }
}