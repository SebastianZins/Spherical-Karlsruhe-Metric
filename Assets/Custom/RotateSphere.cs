using UnityEngine;

public class RotateSphere : MonoBehaviour
{
    public bool rotateSphere = false;
    public float rotationSpeed = 20f;

    void Update()
    {
        if (rotateSphere)
        {
            transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);
        }
    }
}
