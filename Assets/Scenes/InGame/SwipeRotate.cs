using UnityEngine;

public class SwipeRotate : MonoBehaviour
{
    private Vector2 startTouchPosition;
    private Vector2 currentTouchPosition;
    private Vector2 endTouchPosition;
    private bool stopTouch = false;

    public float rotateSpeedModifier = 0.2f;  // 回転速度の調整

    void Update()
    {
        SwipeToRotate();
    }

    void SwipeToRotate()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    startTouchPosition = touch.position;
                    stopTouch = false;
                    break;

                case TouchPhase.Moved:
                    if (!stopTouch)
                    {
                        currentTouchPosition = touch.position;

                        // スワイプの方向に応じた回転を実施
                        Vector2 swipeDirection = currentTouchPosition - startTouchPosition;
                        float rotateX = swipeDirection.y * rotateSpeedModifier;
                        float rotateY = -swipeDirection.x * rotateSpeedModifier;

                        transform.Rotate(rotateX, rotateY, 0, Space.World);

                        startTouchPosition = currentTouchPosition;  // 次のフレームのために更新
                    }
                    break;

                case TouchPhase.Ended:
                    stopTouch = true;
                    break;
            }
        }
    }
}
