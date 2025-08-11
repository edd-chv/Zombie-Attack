using UnityEngine;

namespace WiresMiniGame
{
    public class UnityPosition : MonoBehaviour
    {
        private void Awake()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).localPosition = new(transform.GetChild(i).localPosition.x, transform.GetChild(i).localPosition.y, transform.position.z);
            }
        }
    }
}