using UnityEngine;

public class RotateSphere : MonoBehaviour
{
    public bool rotateSphere = false;
    public float rotationSpeed = 20f;

    void Update()
    {
        if (rotateSphere)
        {
            transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime, Space.Self);
        }
    }

    public void SetRotateSphere(bool rotate)
    {
        rotateSphere = rotate;
        Debug.Log("Rotatation flag changed: " + rotate);
    }

    public void SetRotationSpeed(string speedString)
    {
        if (float.TryParse(speedString, out float speed))
        {
            rotationSpeed = speed;
            Debug.Log("Rotatation speed changed: " + speed);
        }
        else
        {
            Debug.LogWarning("Invalid number input: " + speedString);
        }
    }
}
