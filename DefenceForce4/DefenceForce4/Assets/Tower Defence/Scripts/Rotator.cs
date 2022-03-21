using UnityEngine;

public class Rotator : MonoBehaviour
{
    public float speed = 30, sky_moveSpeed = 0.02f;
    public Material skyMaterial = null;

    [SerializeField]
    private Transform fan_left_top = null, fan_left_bottom = null, fan_right_top_bottom= null, fan_right_bottom = null,fan_top_left = null, fan_top_right = null;
    private void Update()
    {
        if (!GameManager.isGameStart)
            return;
        Vector3 offset = new Vector3(0f, Time.time * sky_moveSpeed, 0f);
        skyMaterial.mainTextureOffset = offset;

        fan_left_top.Rotate(-Vector3.down * Time.fixedDeltaTime * speed, Space.Self);
        fan_left_bottom.Rotate(Vector3.down * Time.fixedDeltaTime * speed, Space.Self);
        fan_right_top_bottom.Rotate(-Vector3.up * Time.fixedDeltaTime * speed, Space.Self);
        fan_right_bottom.Rotate(Vector3.up * Time.fixedDeltaTime * speed, Space.Self);
        fan_top_left.Rotate(-Vector3.down * Time.fixedDeltaTime * 50, Space.Self);
        fan_top_right.Rotate(Vector3.down * Time.fixedDeltaTime * 50, Space.Self);
    }
}
