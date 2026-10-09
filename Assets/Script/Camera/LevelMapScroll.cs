using UnityEngine;
using UnityEngine.InputSystem;

public class LevelMapScroll : MonoBehaviour
{
    public float scrollSpeed = 10f;
    public float smoothTime = 0.1f;

    private float targetY;
    private float velocityY;

    void Start()
    {
        targetY = transform.position.y;
    }

    void Update()
    {
        Vector2 scroll = Mouse.current.scroll.ReadValue();
        if (Mathf.Abs(scroll.y) > 0.0001f)
        {
            targetY += scroll.y * scrollSpeed * Time.deltaTime;
        }
        Vector3 newPosition = transform.position;
        newPosition.y = Mathf.SmoothDamp(transform.position.y, targetY, ref velocityY, smoothTime);
        transform.position = newPosition;
    }
}