using UnityEngine;

public class Camera2D : MonoBehaviour
{
    public Transform Area;

    Vector3 lastPosition;
    Vector3 lastScale;

    public void Set()
    {
        if (Area == null) return;

        float areaWidth = Area.localScale.x;
        float areaHeight = Area.localScale.y;

        float screenAspect = (float)Screen.width / Screen.height;
        float areaAspect = areaWidth / areaHeight;

        float orthoSize = areaHeight / 2f;

        if (screenAspect < areaAspect)
        {
            orthoSize = (areaWidth / screenAspect) / 2f;
        }

        Camera.main.orthographicSize = orthoSize;

        // Center camera on the Area
        Vector3 newPosition = Area.position;
        newPosition.z = Camera.main.transform.position.z;
        Camera.main.transform.position = newPosition;
    }

    void LateUpdate()
    {
        if (Area == null) return;

        if (lastPosition != Area.position || lastScale != Area.localScale)
        {
            lastPosition = Area.position;
            lastScale = Area.localScale;
            Set();
        }
    }
}
