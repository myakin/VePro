using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexObject : MonoBehaviour {

    public List<GameObject> neighbors = new List<GameObject>();

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

    public float GetSideLength() {
        return transform.localScale.x;
    }

    public float GetCurrentAngle() {
        return angles[activeAngleIndex];
    }

    public Vector2 GetCurrentGrid() {
        Vector2 currentGrid = transform.parent.GetComponent<GridManager>().id;
        return currentGrid;

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
        return new Vector2[] { currentGrid, wing1, wing2 };

    }

}
