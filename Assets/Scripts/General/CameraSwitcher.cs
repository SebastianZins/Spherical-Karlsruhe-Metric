using System.Linq;
using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    public Camera[] cameras;

    public void SwitchCamera()
    {
        int activeIndex = cameras.ToList().FindIndex(c => c.enabled);
        if (activeIndex == -1)
        {
            activeIndex = 0;
        }

        for (int i = 0; i < cameras.Length; i++)
        {
            cameras[i].enabled = (i == (activeIndex + 1) % cameras.Length);
        }
    }
}
