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
            int rotationCount = 0;
            ParentObjs();
            outlinerOldParent = anOutliner;
            Transform objectToRotate = GameObject.FindGameObjectWithTag("OutlinerBody").transform;
            anOutliner.SetParent(objectToRotate);
            isRotatingClockwise = isClockwise;
            StartCoroutine(AnimateRotation(objectToRotate, rotationCount));
        }
    }


    private IEnumerator AnimateRotation(Transform anOutlinerBody, int aRotationCount) {
        Quaternion targetRotation =  Quaternion.Euler(0, 0, (int)transform.eulerAngles.z + 120f);
        Quaternion targetOutlinerRotation = Quaternion.Euler(0, 0, (int)anOutlinerBody.eulerAngles.z + 120f);
        if (!isRotatingClockwise) {
            targetRotation = Quaternion.Euler(0, 0, (int)transform.eulerAngles.z - 120f);
            targetOutlinerRotation = Quaternion.Euler(0, 0, (int)anOutlinerBody.eulerAngles.z - 120f);
        }

        float timer = 0;
        float animtionDuration = 0.5f;
        while (timer<animtionDuration) {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, timer / animtionDuration);
            anOutlinerBody.rotation = Quaternion.Slerp(anOutlinerBody.rotation, targetOutlinerRotation, timer / animtionDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        transform.rotation = targetRotation;
        anOutlinerBody.rotation = targetOutlinerRotation;

        RotateObjectGrids();
        UpdateGameControllerDictionary();
        UpdateHexNeighbors();
        originalObj.GetComponent<HexObject>().UpdateAngleActiveIndex(isRotatingClockwise);


        if (CheckForBlowingObjects() || aRotationCount==2) { // end rotation
            StopCoroutine(AnimateRotation(anOutlinerBody, aRotationCount));
            UnParentObjs();
            InputManager.im.isRotationOn = false;
            isAnimating = false;
            anOutlinerBody.parent.SetParent(outlinerOldParent);
        } else {
            aRotationCount++;
            yield return AnimateRotation(anOutlinerBody, aRotationCount);
        }

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

        //Debug.Log(CheckForBlowingObjects());
    }

    private void ParentObjs() {
        originalObj.SetParent(transform);
        wing1Obj.SetParent(transform);
        wing2Obj.SetParent(transform);
    }

    private void BlowObjects(Transform anOriginalObject, Transform[] aNeighborList) {
        anOriginalObject.SetParent(null);
        anOriginalObject.gameObject.SetActive(false);
        Transform gridForOriginal = anOriginalObject.GetComponent<HexObject>().parentingGrid;
        GameController.gc.generationPool.Add(anOriginalObject.gameObject);
        //TODO: place a particle effect on this position and activate it

        aNeighborList[0].SetParent(null);
        aNeighborList[0].gameObject.SetActive(false);
        Transform neighborGrid0 = aNeighborList[0].GetComponent<HexObject>().parentingGrid;
        GameController.gc.generationPool.Add(aNeighborList[0].gameObject);
        //TODO: place a particle effect on this position and activate it

        aNeighborList[1].SetParent(null);
        aNeighborList[1].gameObject.SetActive(false);
        Transform neighborGrid1 = aNeighborList[1].GetComponent<HexObject>().parentingGrid;
        GameController.gc.generationPool.Add(aNeighborList[1].gameObject);
        //TODO: place a particle effect on this position and activate it


        Transform[] emptyGrids = new Transform[] { gridForOriginal, neighborGrid0, neighborGrid1 };
        GameController.gc.ProcessPostBlowEvents(emptyGrids);
    }


    private bool CheckForBlowingObjects() {
        bool somethingBlew = false;

        Transform[] originalBlow = ShouldBlow(originalObj);
        if (originalBlow[0]!=null) {
            somethingBlew = true;
            BlowObjects(originalObj, originalBlow);
        }

        Transform[] wing1Blow = ShouldBlow(wing1Obj);
        if (wing1Blow[0] != null) {
            somethingBlew = true;
            BlowObjects(wing1Obj, wing1Blow);
        }

        Transform[] wing2Blow = ShouldBlow(wing2Obj);
        if (wing2Blow[0] != null) {
            somethingBlew = true;
            BlowObjects(wing2Obj, wing2Blow);
        }
        return somethingBlew;
    }
    private Transform[] ShouldBlow(Transform aReferenceObject) {
        string[] names = new string[6];
        names[0] = aReferenceObject.GetComponent<HexObject>().neighbor30.gameObject.name.Substring(0, 6);
        names[1] = aReferenceObject.GetComponent<HexObject>().neighbor90.gameObject.name.Substring(0, 6);
        names[2] = aReferenceObject.GetComponent<HexObject>().neighbor120.gameObject.name.Substring(0, 6);
        names[3] = aReferenceObject.GetComponent<HexObject>().neighbor210.gameObject.name.Substring(0, 6);
        names[4] = aReferenceObject.GetComponent<HexObject>().neighbor270.gameObject.name.Substring(0, 6);
        names[5] = aReferenceObject.GetComponent<HexObject>().neighbor330.gameObject.name.Substring(0, 6);

        string thisName = aReferenceObject.gameObject.name.Substring(0, 6);

        for (int i = 0; i < 6; i++) {
            if (thisName == names[0] && thisName == names[1]) {
                return new Transform[] { aReferenceObject.GetComponent<HexObject>().neighbor30, aReferenceObject.GetComponent<HexObject>().neighbor90 };
            } else if (thisName == names[1] && thisName == names[2]) {
                return new Transform[] { aReferenceObject.GetComponent<HexObject>().neighbor90, aReferenceObject.GetComponent<HexObject>().neighbor120 };
            } else if (thisName == names[2] && thisName == names[3]) {
                return new Transform[] { aReferenceObject.GetComponent<HexObject>().neighbor120, aReferenceObject.GetComponent<HexObject>().neighbor210 };
            } else if (thisName == names[3] && thisName == names[4]) {
                return new Transform[] { aReferenceObject.GetComponent<HexObject>().neighbor210, aReferenceObject.GetComponent<HexObject>().neighbor270 };
            } else if (thisName == names[4] && thisName == names[5]) {
                return new Transform[] { aReferenceObject.GetComponent<HexObject>().neighbor270, aReferenceObject.GetComponent<HexObject>().neighbor330 };
            } else if (thisName == names[5] && thisName == names[0]) {
                return new Transform[] { aReferenceObject.GetComponent<HexObject>().neighbor330, aReferenceObject.GetComponent<HexObject>().neighbor30 };
            }
        }
        return new Transform[] {null,null};
    }

    private void UnParentObjs() {
        originalObj.SetParent(originalObj.GetComponent<HexObject>().parentingGrid);
        wing1Obj.SetParent(wing1Obj.GetComponent<HexObject>().parentingGrid);
        wing2Obj.SetParent(wing2Obj.GetComponent<HexObject>().parentingGrid);
    }

}
