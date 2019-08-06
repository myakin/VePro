using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexObject : MonoBehaviour {

    public Transform parentingGrid;

    public Transform neighbor30;
    public Transform neighbor90;
    public Transform neighbor120;
    public Transform neighbor210;
    public Transform neighbor270;
    public Transform neighbor330;

    public Vector2 neighborIndex30;
    public Vector2 neighborIndex90;
    public Vector2 neighborIndex120;
    public Vector2 neighborIndex210;
    public Vector2 neighborIndex270;
    public Vector2 neighborIndex330;




    public int[] sides;
    public float sideLength;
    private float[] angles = new float[] { 120, 60, 0, 300, 240, 180 }; // 0=>120, 1=>60, 2=>0, 3=>300, 4=>240, 5=>180
    private int activeAngleIndex = 0;

    private float sqrRoot3 = Mathf.Sqrt(3);

    public float ExecuteRotatorAngle() {
        activeAngleIndex++;
        if (activeAngleIndex == 6) {
            activeAngleIndex = 0;
        }
        return angles[activeAngleIndex];
    }


    public Vector2[] GetSelectedGridIndexes() {
        Vector2 currentGrid = GetCurrentGrid();
        // we have a search patch defined by angles[activeAngleIndex]
        Vector2 wing1 = Vector2.zero;
        Vector2 wing2 = Vector2.zero;

        switch(angles[activeAngleIndex]) {
            case 120:
                wing1 = new Vector2(currentGrid.x - 1, (int)currentGrid.x % 2 == 0 ? currentGrid.y : currentGrid.y + 1);
                wing2 = new Vector2(currentGrid.x, currentGrid.y + 1);
                break;

            case 60:
                wing1 = new Vector2(currentGrid.x, currentGrid.y + 1);
                wing2 = new Vector2(currentGrid.x + 1, (int)currentGrid.x % 2 == 0 ? currentGrid.y : currentGrid.y + 1);
                break;

            case 0:
                wing1 = new Vector2(currentGrid.x + 1, (int)currentGrid.x % 2 == 0 ? currentGrid.y : currentGrid.y + 1);
                wing2 = new Vector2(currentGrid.x + 1, (int)currentGrid.x % 2 == 0 ? currentGrid.y - 1 : currentGrid.y);
                break;

            case 300:
                wing1 = new Vector2(currentGrid.x + 1, (int)currentGrid.x % 2 == 0 ? currentGrid.y - 1 : currentGrid.y);
                wing2 = new Vector2(currentGrid.x, currentGrid.y - 1);
                break;

            case 240:
                wing1 = new Vector2(currentGrid.x, currentGrid.y - 1);
                wing2 = new Vector2(currentGrid.x - 1, (int)currentGrid.x % 2 == 0 ? currentGrid.y - 1 : currentGrid.y);
                break;

            case 180:
                wing1 = new Vector2(currentGrid.x - 1, (int)currentGrid.x % 2 == 0 ? currentGrid.y - 1 : currentGrid.y);
                wing2 = new Vector2(currentGrid.x - 1, (int)currentGrid.x % 2 == 0 ? currentGrid.y : currentGrid.y + 1);
                break;

        }
        //Debug.Log("angle: " + angles[activeAngleIndex] + " current grid:" + currentGrid + " wing1:" + wing1 + " wing2:" + wing2);
        return new Vector2[] { currentGrid, wing1, wing2 };

    }
    public Transform[] GetSelectedNeighbors() {
        Vector2 currentGrid = GetCurrentGrid();
        Transform obj1 = null;
        Transform obj2 = null;

        float outlinerAngle = (int)GameController.gc.outliner.rotation.eulerAngles.z;
        activeAngleIndex = System.Array.IndexOf(angles, outlinerAngle);
        Debug.Log(outlinerAngle + " " + activeAngleIndex);

        switch (angles[activeAngleIndex]) {
            case 120:
                obj1 = neighbor120;
                obj2 = neighbor90;
                break;

            case 60:
                obj1 = neighbor90;
                obj2 = neighbor30;
                break;

            case 0:
                obj1 = neighbor30;
                obj2 = neighbor330;
                break;

            case 300:
                obj1 = neighbor330;
                obj2 = neighbor270;
                break;

            case 240:
                obj1 = neighbor270;
                obj2 = neighbor210;
                break;

            case 180:
                obj1 = neighbor210;
                obj2 = neighbor120;
                break;

        }
        return new Transform[] { transform, obj1, obj2 };

    }

    public void UpdateAngleActiveIndex(bool isRotatingClockwise) {
        float newAngle = angles[activeAngleIndex];
        if (isRotatingClockwise) {
            newAngle -= 60;
        } else {
            newAngle += 60;
        }
        activeAngleIndex = System.Array.IndexOf(angles, newAngle);
    }

    public void SetNeighborIndexes() {
        Vector2 thisObjGrid = GetCurrentGrid();

        neighborIndex30 = new Vector2(thisObjGrid.x + 1, (int)thisObjGrid.x % 2 == 0 ? thisObjGrid.y : thisObjGrid.y + 1);
        neighborIndex90 = new Vector2(thisObjGrid.x, thisObjGrid.y + 1);
        neighborIndex120 = new Vector2(thisObjGrid.x -1, (int)thisObjGrid.x % 2 == 0 ? thisObjGrid.y : thisObjGrid.y + 1);

        neighborIndex210 = new Vector2(thisObjGrid.x - 1, (int)thisObjGrid.x % 2 == 0 ? thisObjGrid.y - 1 : thisObjGrid.y);
        neighborIndex270 = new Vector2(thisObjGrid.x, thisObjGrid.y - 1);
        neighborIndex330 = new Vector2(thisObjGrid.x + 1, (int)thisObjGrid.x % 2 == 0 ? thisObjGrid.y - 1 : thisObjGrid.y);

    }


    public float GetSideLength() {
        return transform.localScale.x;
    }

    public float GetCurrentAngle() {
        return angles[activeAngleIndex];
    }

    public Vector2 GetCurrentGrid() {
        Vector2 currentGrid = parentingGrid.GetComponent<GridManager>().id;
        return currentGrid;

    }
}
