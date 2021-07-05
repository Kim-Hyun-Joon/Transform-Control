using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
    private Camera cam;
    private ChangeCursor cursor;
    private Transform ground;

    public bool useKeyboardInput = true;        //키보드 입력을 받을지
    public bool usePanning = true;              //이동할지 말지
    public bool useScrollwheelZooming = true;   //스크롤휠을 쓸지
    public bool useKeyboardZooming = true;      //키보드로 줌인아웃 할지
    public bool useMouseRotation = true;        //마우스로 회전을 할지
    public bool cameraMode;                     //true : orthographic false : perspective
    public bool limitMap = true;                //카메라 이동에 제한을 둘지

    private enum InputMouse {
        Translate,
        Rotate,
        None
    }
    private InputMouse mode;

    public float keyboardMovementSpeed = 15f;
    public float panningSpeed = 10f;
    public LayerMask groundMask = -1;           //바닥
    public float zoomPos = 0;
    public float keyboardZoomingSensitivity = 2f;
    public float scrollWheelZoomingSensitivity = 25f;
    public float minHeight = 5f;
    public float maxHeight = 20f;
    public float zoomSpeed = 5f;
    public float rotationSpeed = 150f;
    public float limitX, limitZ;

    private string horizontalAxis = "Horizontal";
    private string verticalAxis = "Vertical";

    private KeyCode panningKey = KeyCode.Mouse2;

    private string zoomingAxis = "Mouse ScrollWheel";

    private KeyCode mouseRotationKey = KeyCode.Mouse1;

    private Vector2 KeyboardInput {

        get { return useKeyboardInput ? new Vector2(Input.GetAxis(horizontalAxis), Input.GetAxis(verticalAxis)) : Vector2.zero; }
    }
    private Vector2 MouseInput {

        get { return Input.mousePosition; }
    }
    private float ScrollWheel {

        get { return Input.GetAxis(zoomingAxis); }
    }
    private Vector2 MouseAxis {

        get { return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")); }
    }

    // Start is called before the first frame update
    void Start() {

        ground = GameObject.FindWithTag("Ground").transform;
        limitX = ground.localScale.x;
        limitZ = ground.localScale.z;
        mode = InputMouse.None;
        cam = transform.GetComponent<Camera>();
        cursor = transform.GetComponent<ChangeCursor>();
    }

    // Update is called once per frame
    void LateUpdate() {

        ModeUpdate();
        CameraUpdate();
        CursorUpdate();

    }
    private void ModeUpdate() {

        if (Input.GetKeyDown(KeyCode.Mouse1) && mode == InputMouse.None) {
            mode = InputMouse.Rotate;
        }
        if (Input.GetKeyDown(KeyCode.Mouse2) && mode == InputMouse.None) {
            mode = InputMouse.Translate;
        }
        if(Input.GetKeyUp(KeyCode.Mouse1) || Input.GetKeyUp(KeyCode.Mouse2)) {
            mode = InputMouse.None;
        }
    }

    private void CursorUpdate() {

        if(mode == InputMouse.Translate) {
            cursor.TranslateCursor();
        }
        if(mode == InputMouse.Rotate) {
            cursor.RotateCursor();
        }
        if(mode == InputMouse.None) {
            cursor.DefaultCursor();
        }
    }

    private void CameraUpdate() {

        Move();
        Zoom();
        Rotation();
        LimitPosition();
    }

    private void Move() {

        if (mode == InputMouse.Rotate)
            return;

        if (useKeyboardInput) {
            Vector3 desiredMove = new Vector3(KeyboardInput.x, 0, KeyboardInput.y);

            desiredMove *= keyboardMovementSpeed * Time.deltaTime;
            desiredMove = Quaternion.Euler(new Vector3(0f, transform.eulerAngles.y, 0f)) * desiredMove;
            desiredMove = transform.InverseTransformDirection(desiredMove);

            transform.Translate(desiredMove, Space.Self);
        }
        if (usePanning && Input.GetKey(panningKey) && MouseAxis != Vector2.zero) {
            Vector3 desiredMove = new Vector3(-MouseAxis.x, 0, -MouseAxis.y);

            desiredMove *= panningSpeed * Time.deltaTime;
            desiredMove = Quaternion.Euler(new Vector3(0f, transform.eulerAngles.y, 0f)) * desiredMove;
            desiredMove = transform.InverseTransformDirection(desiredMove);

            transform.Translate(desiredMove, Space.Self);
        }
    }
    private void Zoom() {

        if (useScrollwheelZooming) {
            zoomPos -= ScrollWheel * Time.deltaTime * scrollWheelZoomingSensitivity;
        }
        zoomPos = Mathf.Clamp01(zoomPos);
        if (cam.orthographic) {
            cam.orthographicSize =  Mathf.Lerp(minHeight, maxHeight, zoomPos);
        } else {
            float targetHeight = Mathf.Lerp(minHeight, maxHeight, zoomPos);

            transform.position = Vector3.Lerp(transform.position,
                new Vector3(transform.position.x, targetHeight, transform.position.z), Time.deltaTime * zoomSpeed);
        }
    }
    private void Rotation() {

        if (mode == InputMouse.Translate)
            return;

        if (useMouseRotation && Input.GetKey(mouseRotationKey)) {
            float rotationX = transform.localEulerAngles.y + MouseAxis.x * Time.deltaTime * rotationSpeed;
            float rotationY = Mathf.Clamp(transform.localEulerAngles.x - MouseAxis.y * Time.deltaTime * rotationSpeed, 0f, 90f);
            transform.localEulerAngles = new Vector3(rotationY, rotationX, 0f);
        }
    }
    private void LimitPosition() {

        if (!limitMap)          //나중에 설정키를 만들것
            return;

        transform.position = new Vector3(Mathf.Clamp(transform.position.x, -limitX * minHeight, limitX * minHeight),
            transform.position.y,
            Mathf.Clamp(transform.position.z, -limitZ * minHeight, limitZ * minHeight));
    }
}
