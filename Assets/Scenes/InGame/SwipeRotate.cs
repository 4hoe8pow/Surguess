using UnityEngine;

public class SwipeRotate : MonoBehaviour
{
    private Vector2 startTouchPosition;
    private Vector2 currentTouchPosition;
    private Vector2 lastTouchPosition;
    private bool isSwiping = false;

    public float rotateSpeedModifier = 0.2f;
    public float inertiaMultiplier = 0.5f;  // 慣性の強さ
    private float rotationX = 0f;  // X軸の回転
    private float rotationY = 0f;  // Y軸の回転
    private float currentInertiaX = 0f;  // 現在のX軸慣性
    private float currentInertiaY = 0f;  // 現在のY軸慣性

    void Update()
    {
        SwipeToRotate();
        ApplyInertia();
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
                    lastTouchPosition = touch.position;  // 最後のタッチ位置を初期化
                    isSwiping = true;
                    break;

                case TouchPhase.Moved:
                    if (isSwiping)
                    {
                        currentTouchPosition = touch.position;

                        // スワイプの方向に応じた回転を実施
                        Vector2 swipeDirection = currentTouchPosition - startTouchPosition;
                        rotationX = swipeDirection.y * rotateSpeedModifier;
                        rotationY = -swipeDirection.x * rotateSpeedModifier;

                        transform.Rotate(rotationX, rotationY, 0, Space.World);

                        // 現在の慣性を更新
                        currentInertiaX = (currentTouchPosition.y - lastTouchPosition.y) * inertiaMultiplier;
                        currentInertiaY = (lastTouchPosition.x - currentTouchPosition.x) * inertiaMultiplier;

                        startTouchPosition = currentTouchPosition;  // 次のフレームのために更新
                        lastTouchPosition = currentTouchPosition;  // 最後のタッチ位置を更新
                    }
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    isSwiping = false;
                    break;
            }
        }
    }

    void ApplyInertia()
    {
        if (!isSwiping)
        {
            if (Mathf.Abs(currentInertiaX) > 0.01f || Mathf.Abs(currentInertiaY) > 0.01f)
            {
                transform.Rotate(currentInertiaX, currentInertiaY, 0, Space.World);

                // 慣性を減少させる
                currentInertiaX *= 0.95f;  // 減衰率を調整
                currentInertiaY *= 0.95f;  // 減衰率を調整
            }
        }
    }
}
