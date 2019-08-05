using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridHexPair {
    public GameObject grid;
    public GameObject hex;

    public GridHexPair(GameObject aGridObj, GameObject aHexObj) {
        this.grid = aGridObj;
        this.hex = aHexObj;
    }
}
