using UnityEngine;

public class FloatingOrigin : MonoBehaviour
{
    public Transform player;
    public float threshold = 5000f;

    void LateUpdate()
    {
        Vector3 offset = player.position;

        if (offset.magnitude > threshold)
        {
            foreach (GameObject obj in FindObjectsOfType<GameObject>())
            {
                if (obj.transform.parent == null)
                {
                    obj.transform.position -= offset;
                }
            }
        }
    }
}