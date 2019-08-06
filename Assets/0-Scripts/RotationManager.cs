using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationManager : MonoBehaviour {

    // these are outliner's selection objects where order is counter-clockwise. so, wing1 is the next element after the selected hex
    public Transform originalObj;
    public Transform wing1Obj;
    public Transform wing2Obj;

    public bool isRotatingClockwise;
    public bool isAnimating;


    private Transform outlinerOldParent;

    public void SetObjects(Transform anOriginalObj, Transform aWing1Obj, Transform aWing2Obj) {
        originalObj = anOriginalObj;
        wing1Obj = aWing1Obj;
        wing2Obj = aWing2Obj;
    }

    public void RotateOnce(Transform anOutliner, bool isClockwise) {
        if (!isAnimating) {
            ParentObjs();
            outlinerOldParent = anOutliner;
            Transform objectToRotate = GameObject.FindGameObjectWithTag("OutlinerBody").transform;
            anOutliner.SetParent(objectToRotate);
            isRotatingClockwise = isClockwise;
            StartCoroutine(AnimateRotation(objectToRotate));
        }
    }



    private IEnumerator AnimateRotation(Transform anOutlinerBody) {
        Quaternion targetRotation = transform.rotation * Quaternion.Euler(0, 0, 120f);
        Quaternion targetOutlinerRotation = anOutlinerBody.rotation * Quaternion.Euler(0, 0, 120f);
        if (!isRotatingClockwise) {
            targetRotation = transform.rotation * Quaternion.Euler(0, 0, -120f);
            targetOutlinerRotation = anOutlinerBody.rotation * Quaternion.Euler(0, 0, -120f);
        }

        float timer = 0;
        float animtionDuration = 1f;
        while (timer<animtionDuration) {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, timer / animtionDuration);
            anOutlinerBody.rotation = Quaternion.Slerp(anOutlinerBody.rotation, targetOutlinerRotation, timer / animtionDuration);
            timer += Time.deltaTime;
            yield return null;
        }

        StopCoroutine(AnimateRotation(anOutlinerBody));

        RotateObjectGrids();
        UpdateGameControllerDictionary();
        UpdateHexNeighbors();
        originalObj.GetComponent<HexObject>().UpdateAngleActiveIndex(isRotatingClockwise);
        UnParentObjs();
        InputManager.im.isRotationOn = false;
        //GameController.gc.DumpOutlinedObjects();

        isAnimating = false;

        anOutlinerBody.parent.SetParent(outlinerOldParent);

    }

    private void RotateObjectGrids() {
        Transform originalObjOldGrid = originalObj.GetComponent<HexObject>().parentingGrid;
        Transform wing1ObjOldGrid = wing1Obj.GetComponent<HexObject>().parentingGrid;
        Transform wing2ObjOldGrid = wing2Obj.GetComponent<HexObject>().parentingGrid;

        if (isRotatingClockwise) {
            originalObj.GetComponent<HexObject>().parentingGrid = wing2ObjOldGrid;
            wing1Obj.GetComponent<HexObject>().parentingGrid = originalObjOldGrid;
            wing2Obj.GetComponent<HexObject>().parentingGrid = wing1ObjOldGrid;
        } else {
            originalObj.GetComponent<HexObject>().parentingGrid = wing1ObjOldGrid;
            wing1Obj.GetComponent<HexObject>().parentingGrid = wing2ObjOldGrid;
            wing2Obj.GetComponent<HexObject>().parentingGrid = originalObjOldGrid;
        }
    }

    private void UpdateGameControllerDictionary() { // must be called after RotateObjectGrids()
        GameController.gc.gridsAndContents[originalObj.GetComponent<HexObject>().parentingGrid.GetComponent<GridManager>().id].hex = originalObj.gameObject;
        GameController.gc.gridsAndContents[wing1Obj.GetComponent<HexObject>().parentingGrid.GetComponent<GridManager>().id].hex = wing1Obj.gameObject;
        GameController.gc.gridsAndContents[wing2Obj.GetComponent<HexObject>().parentingGrid.GetComponent<GridManager>().id].hex = wing2Obj.gameObject;
    }

    private void UpdateHexNeighbors() {
        originalObj.GetComponent<HexObject>().SetNeighborIndexes();
        wing1Obj.GetComponent<HexObject>().SetNeighborIndexes();
        wing2Obj.GetComponent<HexObject>().SetNeighborIndexes();

        GameController.gc.UpdateObjectNeighbors(originalObj.GetComponent<HexObject>());
        GameController.gc.UpdateObjectNeighbors(wing1Obj.GetComponent<HexObject>());
        GameController.gc.UpdateObjectNeighbors(wing2Obj.GetComponent<HexObject>());

    }

    private void ParentObjs() {
        originalObj.SetParent(transform);
        wing1Obj.SetParent(transform);
        wing2Obj.SetParent(transform);
    }
    private void UnParentObjs() {
        originalObj.SetParent(originalObj.GetComponent<HexObject>().parentingGrid);
        wing1Obj.SetParent(wing1Obj.GetComponent<HexObject>().parentingGrid);
        wing2Obj.SetParent(wing2Obj.GetComponent<HexObject>().parentingGrid);
    }

}
