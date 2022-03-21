using System.Collections;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager instance { get; set; }
    private Touch  touch , touchZero, touchOne;
    public Camera maincamera = null;
    public GameObject CamParent = null, map = null;
    private Vector2 fingerDown = Vector2.zero, fingerUp = Vector2.zero;
    private Vector3 lastPanPosition = Vector3.zero;
    internal float MinSize = 25, MaxSize = 80, minpanningpos = 3.5f, minSwipeDistance = 0.1f, defautlsize = 30 /*55f*/, PanSpeed = 4f, horiz_rotatespeed = 40f,
                             vert_rotatespeed = 5f, ZoomSpeedTouch = 5f, ZoomSpeedMouse = 10f, difference = 0f;
    public bool detectSwipeOnlyAfterRelease = false;

    public float SWIPE_THRESHOLD = 20f;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }
    internal void Inititalize()
    {
        if (maincamera == null)
            maincamera = Camera.main;
        maincamera.fieldOfView = defautlsize;
    }
    internal void ResetData()
    {
        maincamera.transform.localPosition = new Vector3(0f, 11f, 3.3f);
        map.transform.localPosition = new Vector3(0f, 0f, -1.35f);
        map.transform.localEulerAngles = new Vector3(0f, -180f, 0f);
        CamParent.transform.localEulerAngles = new Vector3(0f,-180f,0f);
        CamParent.transform.position = Vector3.zero;
        maincamera.fieldOfView = defautlsize;
    }
    private void Update()
    {
        if (!GameManager.isGameStart || TowerDefence.TowerManager.isItemDrag || InputManager.isdragscroller)
            return;
        if (Input.touchSupported && Application.platform != RuntimePlatform.WebGLPlayer)
            HandleTouch();
        else
            HandleMouse();
    }
    private void HandleTouch()
    {
        switch (Input.touchCount)
        {
            case 1:
                touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                    lastPanPosition = touch.position;
                else if (touch.phase == TouchPhase.Moved)
                    PanCamera(touch.position);
                break;
            case 2:

                touchZero = Input.GetTouch(0);
                touchOne = Input.GetTouch(1);

                Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                float currentMagnitude = (touchZero.position - touchOne.position).magnitude;
                difference = currentMagnitude - prevMagnitude;
                ZoomCamera(difference, 0.05f);
                //if (touchOne.phase == TouchPhase.Moved && touchZero.phase == TouchPhase.Moved)
                //{
                //    if (!detectSwipeOnlyAfterRelease)
                //    {
                //        fingerDown = touchOne.position;
                //        checkSwipe();
                //    }
                //}
                break;
            default:
                break;
        }
    }
    private void HandleMouse()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        ZoomCamera(scroll, ZoomSpeedMouse);
        if (Input.GetMouseButtonDown(0))
            lastPanPosition = Input.mousePosition;
        else if (Input.GetMouseButton(0))
            PanCamera(Input.mousePosition); 
    }
    private void ZoomCamera(float offset, float speed)
    {
        if (offset == 0)
            return;
        maincamera.fieldOfView = Mathf.Clamp(maincamera.fieldOfView - (offset * speed), MinSize, MaxSize);
    }
    private void PanCamera(Vector3 newPanPosition)
    {
        Vector3 offset = maincamera.ScreenToViewportPoint(lastPanPosition - newPanPosition);
        Vector3 move = new Vector3(-offset.x * PanSpeed,0f,- offset.y * PanSpeed);
       
        CamParent.transform.Translate(move, Space.Self);
        Vector3 pos = CamParent.transform.position;
        pos.x = Mathf.Clamp(CamParent.transform.position.x, -minpanningpos, minpanningpos);
        pos.z = Mathf.Clamp(CamParent.transform.position.z, -minpanningpos, minpanningpos);
        CamParent.transform.position = new Vector3(pos.x, 0f, pos.z);
        lastPanPosition = newPanPosition;
    }
    internal IEnumerator Shake(float duration, float magnitude)
    {
        Vector3 orignalPosition = CamParent.transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float z = Random.Range(-1f, 1f) * magnitude;
           CamParent.transform.position = new Vector3(x, CamParent.transform.position.y, z);
            elapsed += Time.deltaTime;
            yield return 0;
        }
        CamParent.transform.position = orignalPosition;
    }
    private static int WrapAngle(float angle)
    {
        angle %= 360;
        if (angle > 180)
            return (int)angle - 360;
        return (int)angle;
    }
    private void checkSwipe()
    {
        //Check if Vertical swipe
        if (verticalMove() > SWIPE_THRESHOLD && verticalMove() > horizontalValMove())
        {
            //Debug.Log("Vertical");
            if (fingerDown.y - fingerUp.y > 0)//up swipe
            {
                OnSwipeUp();
            }
            else if (fingerDown.y - fingerUp.y < 0)//Down swipe
            {
                OnSwipeDown();
            }
            fingerUp = fingerDown;
        }
        //Check if Horizontal swipe
        else if (horizontalValMove() > SWIPE_THRESHOLD && horizontalValMove() > verticalMove())
        {
            //Debug.Log("Horizontal");
            if (fingerDown.x - fingerUp.x > 0)//Right swipe
            {
                OnSwipeRight();
            }
            else if (fingerDown.x - fingerUp.x < 0)//Left swipe
            {
                OnSwipeLeft();
            }
            fingerUp = fingerDown;
        }
        //No Movement at-all
        else
        {
            ZoomCamera(difference, 0.05f);
            Debug.Log("No Swipe!");
        }
    }

    private float verticalMove()
    {
        return Mathf.Abs(fingerDown.y - fingerUp.y);
    }

    private float horizontalValMove()
    {
        return Mathf.Abs(fingerDown.x - fingerUp.x);
    }

    private void OnSwipeUp()
    {
        Debug.Log("Swipe UP ");
        VerticalRotate();
    }

    private void OnSwipeDown()
    {
        VerticalRotate();
        Debug.Log("Swipe Down ");
    }

    void OnSwipeLeft()
    {
        Debug.Log("Swipe Left ");
        HorizontalRotate();
    }

    void OnSwipeRight()
    {
        Debug.Log("Swipe Right ");
        HorizontalRotate();
    }

    private void VerticalRotate()
    {
        float rotateSpeed1 = touchOne.deltaPosition.y * 5 * Time.smoothDeltaTime;
        CamParent.transform.localEulerAngles += new Vector3(1f * rotateSpeed1, 0f, 0f);      //-----------------Up Or Down Rotation

        Vector3 tempRotation = new Vector3(CamParent.transform.localEulerAngles.x, CamParent.transform.localEulerAngles.y, 0f);
        tempRotation.x = Mathf.Clamp(WrapAngle(tempRotation.x), 0f, 45f);
        if (tempRotation.x >= 0 && tempRotation.x <= 45)
            CamParent.transform.localEulerAngles = tempRotation;
        else
            CamParent.transform.localEulerAngles = new Vector3(0, CamParent.transform.rotation.y, 0f);
    }
    private void HorizontalRotate()
    {
        float rotateSpeed = touchOne.deltaPosition.x * 5 * Time.deltaTime;
        CamParent.transform.localEulerAngles += new Vector3(0f, 1f * rotateSpeed, 0f);
    }
}
