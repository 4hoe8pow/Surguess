using UnityEngine;

public class MouseDragRotate : MonoBehaviour
{
    private Vector3 lastMousePosition;
    private bool isDragging = false;

    public float rotateSpeedModifier = 0.2f;
    public float inertiaMultiplier = 0.5f;  // 慣性の強さ
    private float currentInertiaX = 0f;  // 現在のX軸慣性
    private float currentInertiaY = 0f;  // 現在のY軸慣性

    void Update()
    {
        if (Input.GetMouseButton(0))  // 左クリックを押している間
        {
            if (!isDragging)
            {
                lastMousePosition = Input.mousePosition;
                isDragging = true;
            }
            else
            {
                Vector3 currentMousePosition = Input.mousePosition;

                // マウスの動きに応じてオブジェクトを回転
                float deltaX = currentMousePosition.x - lastMousePosition.x;
                float deltaY = currentMousePosition.y - lastMousePosition.y;

                float rotateX = deltaY * rotateSpeedModifier;
                float rotateY = -deltaX * rotateSpeedModifier;

                transform.Rotate(rotateX, rotateY, 0, Space.World);

                // 現在の慣性を更新
                currentInertiaX = deltaY * inertiaMultiplier;
                currentInertiaY = -deltaX * inertiaMultiplier;

                lastMousePosition = currentMousePosition;  // 最後のマウス位置を更新
            }
        }
        else if (isDragging)  // マウスボタンが離れた場合
        {
            isDragging = false;
            ApplyInertia();  // 慣性を適用
        }
    }

    void ApplyInertia()
    {
        // 慣性の適用
        if (Mathf.Abs(currentInertiaX) > 0.01f || Mathf.Abs(currentInertiaY) > 0.01f)
        {
            transform.Rotate(currentInertiaX, currentInertiaY, 0, Space.World);

            // 慣性を減少させる
            currentInertiaX *= 0.95f;  // 減衰率を調整
            currentInertiaY *= 0.95f;  // 減衰率を調整

            // 再帰的にApplyInertiaを呼び出して慣性を持続
            if (Mathf.Abs(currentInertiaX) > 0.01f || Mathf.Abs(currentInertiaY) > 0.01f)
            {
                Invoke("ApplyInertia", Time.deltaTime);
            }
        }
    }
}
