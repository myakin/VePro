using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationManager : MonoBehaviour {

    public Vector2[] grids = new Vector2[3];
    public GameObject[] attachedObjects = new GameObject[3];
    public bool isRotatingClockwise;

    public void SetObjects(GameObject[] objs) {
        attachedObjects[0] = objs[0];
        attachedObjects[1] = objs[1];
        attachedObjects[2] = objs[2];
    }

    public void SetGrids(Vector2[] gridsInfo) {
        grids[0] = gridsInfo[0];
        grids[1] = gridsInfo[1];
        grids[2] = gridsInfo[2];

    }

    public void SetObject(GameObject obj, int indexNo) {
        attachedObjects[indexNo] = obj;
    }
    public void SetGrid(Vector2 aGrid, int indexNo) {
        grids[indexNo] = aGrid;
    }

    public void RotateGridsInfo() {
        Vector2[] newGridsInfo = new Vector2[3];
        if (isRotatingClockwise) {
            newGridsInfo[0] = grids[2];
            newGridsInfo[1] = grids[0];
            newGridsInfo[2] = grids[1];
        } else {
            newGridsInfo[0] = grids[1];
            newGridsInfo[1] = grids[2];
            newGridsInfo[2] = grids[0];
        }
        SetGrids(newGridsInfo);
    }


}
