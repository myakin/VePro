using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour {

    public static InputManager im;

    public bool isRotationOn;
    public bool isDragging;
    public Vector2 dragDirection;

    private Camera mainCam;
    public Vector2 touchDownPos;
    public Vector2 touchMovePos;
    private Transform objectTouched;
    private bool isSelectionBeingDelayed;

    private void Awake() {
        if (InputManager.im==null) {
            InputManager.im = this;
        } else {
            if (InputManager.im!=this) {
                Destroy(InputManager.im.gameObject);
                InputManager.im = this;
            }
        }
        DontDestroyOnLoad(InputManager.im.gameObject);
    }

    private void Start() {
        mainCam = Camera.main;
    }

    private void Update() {
        if (Input.touchCount>0) {
            Touch playerTouch = Input.GetTouch(0);
            if (playerTouch.phase == TouchPhase.Began) {
                touchDownPos = Input.mousePosition;
                RaycastHit hit;
                Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit)) {
                    objectTouched = hit.transform;
                    // Debug.Log(objectTouched.gameObject.name);
                    if (objectTouched.tag=="Hex") {
                        if (!isSelectionBeingDelayed) {
                            isSelectionBeingDelayed = true;
                            StartCoroutine(ExecuteSelectionWithDelay(objectTouched.GetComponent<HexObject>()));
                            Debug.Log("selected object");
                        }
                        // GameController.gc.SetOutlinerPositionWithDelay(objectTouched.GetComponent<HexObject>(),0.3f);
                        // GameController.gc.SelectObject(objectTouched.GetComponent<HexObject>());

                    }


                }
            } else if (playerTouch.phase == TouchPhase.Ended) {
                isDragging = false;

                //Debug.Log("Performed a touch up");

            } else if (playerTouch.phase == TouchPhase.Moved) {
                touchMovePos = Input.mousePosition;
                dragDirection = touchMovePos - touchDownPos;
                // Debug.Log(dragDirection.sqrMagnitude);
                if (dragDirection.sqrMagnitude>1000f) {
                    isDragging = true;
                    if (!isRotationOn) {
                        GameController.gc.RotateObjects();
                        isRotationOn = true;
                        Debug.Log("is rotating");
                    }
                    //Debug.Log("is dragging");
                }

            }
        }

    }

    private IEnumerator ExecuteSelectionWithDelay(HexObject aHexObject) {
        yield return new WaitForSeconds(0.3f);
        if (!isDragging) {
            GameController.gc.SetOutlinerPosition(aHexObject);
        }
        StopCoroutine(ExecuteSelectionWithDelay(aHexObject));
        isSelectionBeingDelayed = false;
    }

}
