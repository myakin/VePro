using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour {

    public Vector2 id;
    public bool isBlown;

    public void MoveColumn() {
        Vector2 upperGridIndex = new Vector2(id.x, id.y + 1);
        Transform targetHex = null;
        if (upperGridIndex.y < GameController.gc.gridSize.y) {
            if (GameController.gc.gridsAndContents[upperGridIndex].grid.transform.childCount>0) {
                if (GameController.gc.gridsAndContents[upperGridIndex].grid.transform.GetChild(0).GetComponent<HexObject>().isBlown) {
                    upperGridIndex = new Vector2(id.x, id.y + 2);
                    if (upperGridIndex.y < GameController.gc.gridSize.y) {
                        if (GameController.gc.gridsAndContents[upperGridIndex].grid.transform.childCount > 0) {
                            if (!GameController.gc.gridsAndContents[upperGridIndex].grid.transform.GetChild(0).GetComponent<HexObject>().isBlown) {
                                targetHex = GameController.gc.gridsAndContents[upperGridIndex].grid.transform.GetChild(0);
                            }
                        }
                    }
                } else {
                    targetHex = GameController.gc.gridsAndContents[upperGridIndex].grid.transform.GetChild(0);
                }
            }
        }

        if (targetHex!=null) {
            targetHex.GetComponent<HexObject>().SetFallTargetGrid(transform);
        }
    }


}
