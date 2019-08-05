using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour {
    
    private Camera mainCam;
    private Vector2 touchDownPos;
    private Vector2 touchMovePos;
    private Transform objectTouched;
    public bool isRotationOn;

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
                        GameController.gc.SetOutlinerPosition(objectTouched.GetComponent<HexObject>());
                    }


                }
            } else if (playerTouch.phase == TouchPhase.Ended) {
                Debug.Log("Performed a touch up");

            } else if (playerTouch.phase == TouchPhase.Moved) {
                touchMovePos = Input.mousePosition;
                if ((touchMovePos - touchDownPos).sqrMagnitude>4f) {
                    if (!isRotationOn) {

                    }
                    // Debug.Log("Performing a drag on " + objectTouched.gameObject.name);
                }

            }
        }

    }



}
