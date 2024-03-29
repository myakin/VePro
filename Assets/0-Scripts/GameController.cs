﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

    public static GameController gc;

    public Vector2 gridSize = new Vector2(8, 9);
    public Transform backgroundObject;
    public Transform outliner;
    public HexObject touchFocusedOnThisObject;
    private Transform rotator;
    private float sqrRoot3 = Mathf.Sqrt(3);
    //private bool isSelectionCoroutineRunning;

    public Dictionary<Vector2, GridHexPair> gridsAndContents = new Dictionary<Vector2, GridHexPair>();

    public List<GameObject> generationPool = new List<GameObject>();

    public int score = 0;

    private void Awake() {
        if (GameController.gc==null) {
            GameController.gc = this;
        } else {
            if (GameController.gc!=this) {
                Destroy(GameController.gc.gameObject);
                GameController.gc = this;
            }
        }
        DontDestroyOnLoad(GameController.gc.gameObject);
    }

    private void Start() {
        MakeGrid();
    }

    private void MakeGrid() {
        float sideLength = GetSideLength();
        // total row length is 2a + 3ax/2 (see explanation in GetSideLength()
        // we want to move half way back in x axis to find the starting point of the background object
        // one way to do that is using this formula: backgroundObject.position - (backgroundObject.right * (GetRowLength()/2))
        // second way is: backgroundObject.position - (backgroundObject.right * ((2 *sideLength) + (3 * sideLength * x/2)/2)), since we alrey know side length
        // if we can find a better way to calculate sideLength without using background object bounds, we will prefer the second formula; for now, we will use the first one
        Vector3 gridStartPoint = backgroundObject.position - (backgroundObject.right * (GetRowLength()/2));
        // and there will ve a small offsetX for the row which is sideLength
        gridStartPoint += backgroundObject.right * sideLength;

        //GameObject s = new GameObject("Origin"); // this is only for debugging purposses to see if the startPoint is calculated properly
        //s.transform.position = gridStartPoint; // this is only for debugging purposses to see if the startPoint is calculated properly

        // now we are ready to place the grid objects. Grid objects are empty GameObjects with GridManager script attached
        // we canb also attach a hexObject to each grid object in order to prevent a second for loop
        // so, we will first calculate the position of a grid object and generate it, and then attach a hex object as a child to it; color will be determined in this process
        // lastly, we will have to scale hex object after generation to fit the grid perfectly
        for (int y = 0; y < gridSize.y; y++) {
            for (int x = 0; x < gridSize.x; x++) {

                // grid insertion x coordinate is calculated according to formula: a + 3x/2, where a is the sideLength
                // grid insertion y ccordinate is calculated with formula (2y+1) * (sqrt(3)*a/2) for even x indexes, and 
                // with (2y+2) * (sqrt(3)*a/2) for odd indexes; we will use mode 2 to add an offset to (2y+1) portion of the formula to produce this effect
                float offsetY = x % 2 == 0 ? 0 : 1;
                Vector2 gridInsertionPoint = new Vector2(sideLength * (1.5f * x), (2 * y + 1 + offsetY) * (sqrRoot3 * sideLength * 0.5f));

                // if our starting point is calculated correctly, then:
                Vector3 gridWorldPoint = gridStartPoint + (backgroundObject.right * gridInsertionPoint.x) + (backgroundObject.up * gridInsertionPoint.y);

                GameObject gridObject = Instantiate(Resources.Load("GridCell") as GameObject, gridWorldPoint, Quaternion.identity, backgroundObject);
                gridObject.GetComponent<GridManager>().id = new Vector2(x, y);

                GameObject hex = null;
                int randomlyDefinedColorChoice = Random.Range(0, 4);
                if (randomlyDefinedColorChoice == 0) {
                    hex = Instantiate(Resources.Load("HexBlue") as GameObject, gridObject.transform);
                } else if (randomlyDefinedColorChoice == 1) {
                    hex = Instantiate(Resources.Load("HexGreen") as GameObject, gridObject.transform);
                } else if (randomlyDefinedColorChoice == 2) {
                    hex = Instantiate(Resources.Load("HexRed") as GameObject, gridObject.transform);
                } else if (randomlyDefinedColorChoice == 3) {
                    hex = Instantiate(Resources.Load("HexPurple") as GameObject, gridObject.transform);
                }

                // this is a helper Dictionary that we only need on start up: 
                // we'll use this to check if neightbor indexes are generated for each hex object
                // then we eill assign the hex object's neighbors if check passes
                GridHexPair newGridHexPair = new GridHexPair(gridObject, hex);
                gridsAndContents.Add(new Vector2(x,y), newGridHexPair);

                // set neighbor indexes
                hex.GetComponent<HexObject>().parentingGrid = gridObject.transform;
                hex.GetComponent<HexObject>().SetNeighborIndexes();

                // now that we have our grid, we have to re-scale our object to fit perfectly to the grid
                // since all of our hex objects are designed by 1 unit side length, it is ok to simply scale the object by sideLength, which is our calculated side length
                hex.transform.localScale = new Vector3(sideLength, sideLength, sideLength);

            }
        }

        // set neighbors
        foreach (KeyValuePair<Vector2, GridHexPair> items in gridsAndContents) {
            UpdateObjectNeighbors(items.Value.hex.GetComponent<HexObject>());
        }
    }



    private float GetSideLength() {
        // let a be the side length of a hexagon => if hexagonal tiles are designed as side by side formation,
        // total length of the row will be a + 3ax/2 + a, where x is the last grid element's index number in horizontal (x) direction
        // then, 2a + 3ax/2 = L, where L is total length
        // a(2+3x/2) = L => a = L/(2+3x/2) ==> if there are 3 elements (grids) in x axis, (2+3x/2) is handled as (2+3(gridSize.x-1)/2)
        // we already know L is bounds.max.x + ABS(bouns.min.x)

        float platformLength = GetRowLength();
        //float unitLength = (platformLength / gridSize.x) * (2f/5f);
        //float unitLength = platformLength / (gridSize.x / 2f);
        float sideLength = platformLength / (2 + (3 * (gridSize.x - 1) / 2));
        return sideLength;

    }

    private float GetRowLength() {
        return backgroundObject.GetComponent<MeshRenderer>().bounds.max.x + Mathf.Abs(backgroundObject.GetComponent<MeshRenderer>().bounds.min.x);
    }

    /*
    public void SetOutlinerPositionWithDelay(HexObject aHexObjectManager, float aDelayTime) {
        touchFocusedOnThisObject = aHexObjectManager;
        Invoke("SetOutlinerPosition", aDelayTime);
    }


    public void SetOutlinerPosition() {
        if (!InputManager.im.isDragging) {
            outliner.position = touchFocusedOnThisObject.transform.position;
            outliner.rotation = Quaternion.Euler(0, 0, touchFocusedOnThisObject.ExecuteRotatorAngle());
            outliner.localScale = new Vector3(touchFocusedOnThisObject.GetSideLength(), touchFocusedOnThisObject.GetSideLength(), touchFocusedOnThisObject.GetSideLength());
            outliner.gameObject.SetActive(true);
            Debug.Log("Object selection triggered in GameController");
        }
    }
    */
    /*
    public void SelectObject(HexObject aHexObjectManager) {
        if (!isSelectionCoroutineRunning) {
            StartCoroutine(SetOutlinerPosition(aHexObjectManager));
        }
    }
    private IEnumerator SetOutlinerPosition(HexObject aHexObjectManager) {
        isSelectionCoroutineRunning = true;
        yield return new WaitForSeconds(0.3f);
        if (!InputManager.im.isDragging) {
            touchFocusedOnThisObject = aHexObjectManager;
            outliner.position = touchFocusedOnThisObject.transform.position;
            outliner.rotation = Quaternion.Euler(0, 0, touchFocusedOnThisObject.ExecuteRotatorAngle());
            outliner.localScale = new Vector3(touchFocusedOnThisObject.GetSideLength(), touchFocusedOnThisObject.GetSideLength(), touchFocusedOnThisObject.GetSideLength());
            outliner.gameObject.SetActive(true);
            Debug.Log("Object selection triggered in GameController");
        }
        isSelectionCoroutineRunning = false;
    }
    */
    public void SetOutlinerPosition(HexObject aHexObjectManager) {
        touchFocusedOnThisObject = aHexObjectManager;
        outliner.position = touchFocusedOnThisObject.transform.position;
        outliner.rotation = Quaternion.Euler(0, 0, touchFocusedOnThisObject.ExecuteRotatorAngle());
        outliner.localScale = new Vector3(touchFocusedOnThisObject.GetSideLength(), touchFocusedOnThisObject.GetSideLength(), touchFocusedOnThisObject.GetSideLength());
        outliner.gameObject.SetActive(true);
        //Debug.Log("Object selection triggered in GameController");
    }


    private void PrepareRotator(HexObject aHexObjectManager) {
        // rotation has to be made by parenting hex objects under a GameObject whose scale is Vector3.one
        // otherwise, deformations on meshes will be seen due to the localScale of the outLiner
        // so, for this, we will first set rotation pivot, then aHexObjectManager.GetSelectedGridIndexes(), 
        // then serch for those indexes in gridsAndContents, and parent the values from gridsAndContents under rotator
        // and set rotator's RotationManager values
        if (rotator == null) {
            rotator = new GameObject("Rotator").transform;
            rotator.gameObject.AddComponent<RotationManager>();
        }
        rotator.position = outliner.transform.GetChild(0).position;
        rotator.rotation = outliner.transform.GetChild(0).rotation;

        Transform[] selectedNeighbors = aHexObjectManager.GetSelectedNeighbors();
        selectedNeighbors[0].SetParent(rotator); // this is always original
        selectedNeighbors[1].SetParent(rotator); // this is always wing1
        selectedNeighbors[2].SetParent(rotator); // this is always wing2 ==> the order of the whole is counter-clockwise 

        rotator.GetComponent<RotationManager>().SetObjects(selectedNeighbors[0], selectedNeighbors[1], selectedNeighbors[2]);

    }

    public void RotateObjects() {
        PrepareRotator(touchFocusedOnThisObject);
        // we have to determine, first, if user is rotating clockwise or counter-clockwise
        Vector2 rotatorScreenCoords = Camera.main.WorldToScreenPoint(rotator.position);
        float angle = Vector2.SignedAngle(InputManager.im.touchDownPos - rotatorScreenCoords, InputManager.im.touchMovePos - rotatorScreenCoords);
        bool isRotatingClockwise = angle < 0 ? false : true;

        // begin rotation
        rotator.GetComponent<RotationManager>().RotateOnce(outliner, isRotatingClockwise);
    }


    public void DumpOutlinedObjects() {
        if (rotator!=null) {
            for (int i = 0; i < rotator.childCount; i++) {
                Transform newParent = rotator.GetChild(i).GetComponent<HexObject>().parentingGrid;
                rotator.GetChild(i).SetParent(newParent);
            }
        }
    }


    public void UpdateObjectNeighbors(HexObject aHexObject) {
        aHexObject.neighbor30 = gridsAndContents.ContainsKey(aHexObject.neighborIndex30) ? gridsAndContents[aHexObject.neighborIndex30].hex.transform : null;
        aHexObject.neighbor90 = gridsAndContents.ContainsKey(aHexObject.neighborIndex90) ? gridsAndContents[aHexObject.neighborIndex90].hex.transform : null;
        aHexObject.neighbor120 = gridsAndContents.ContainsKey(aHexObject.neighborIndex120) ? gridsAndContents[aHexObject.neighborIndex120].hex.transform : null;
        aHexObject.neighbor210 = gridsAndContents.ContainsKey(aHexObject.neighborIndex210) ? gridsAndContents[aHexObject.neighborIndex210].hex.transform : null;
        aHexObject.neighbor270 = gridsAndContents.ContainsKey(aHexObject.neighborIndex270) ? gridsAndContents[aHexObject.neighborIndex270].hex.transform : null;
        aHexObject.neighbor330 = gridsAndContents.ContainsKey(aHexObject.neighborIndex330) ? gridsAndContents[aHexObject.neighborIndex330].hex.transform : null;
    }

    public void ProcessPostBlowEvents(Transform[] aGridGroup) {
        // 1. detect empty grids (incoming aGridGroup has these objects)
        // 2. check if the grid above them are occupied; if not, continue checking until the top row
        // 3. if occupied, lerp them to this world space
        // 4. generate new objects at the top of these columns
        // terminology: when gridCoordinatesOfEmptyGrid is used, index numbers of the grid as integers of x and y axis (which is stored in GridManager) is referred.
        // when worldSpaceOfGrid is used, it is the unity coordinates of the object

        int[] columnsChecked = new int[aGridGroup.Length]; // we need this not to lerp the same column twice
        for (int i = 0; i < aGridGroup.Length; i++) {
            Vector2 gridCoordinatesOfEmptyGrid = aGridGroup[i].GetComponent<GridManager>().id;
            if (System.Array.IndexOf(columnsChecked, gridCoordinatesOfEmptyGrid.x) ==-1) {
                Transform aTargetHex;
                if (CheckIfUpperNeightborGridIsOccupied(aGridGroup[i])) {
                    aTargetHex = gridsAndContents[new Vector2(gridCoordinatesOfEmptyGrid.x, gridCoordinatesOfEmptyGrid.y + 1)].grid.transform.GetChild(0);
                } else { // in this case we will look at the grid on 2 upper row and lerp the content down
                    aTargetHex = gridsAndContents[new Vector2(gridCoordinatesOfEmptyGrid.x, gridCoordinatesOfEmptyGrid.y + 2)].grid.transform.GetChild(0);
                }
                columnsChecked[i] = (int)gridCoordinatesOfEmptyGrid.x;
                Transform targetGrid = aGridGroup[i];
                if (!CheckIfLowerNeightborGridIsOccupied(aGridGroup[i])) {
                    targetGrid = gridsAndContents[new Vector2(gridCoordinatesOfEmptyGrid.x, gridCoordinatesOfEmptyGrid.y - 1)].grid.transform;
                }
                StartCoroutine(LerpContentsDownOnColumn(aTargetHex, aGridGroup[i]));

            }
        }
        GenerateNewObjectsForColumn(aGridGroup);

    }
    private bool CheckIfUpperNeightborGridIsOccupied(Transform aGrid) {
        Vector2 gridCoordinatesOfEmptyGrid = aGrid.GetComponent<GridManager>().id;
        if (gridCoordinatesOfEmptyGrid.y + 1 < gridSize.y) {
            if (gridsAndContents[new Vector2(gridCoordinatesOfEmptyGrid.x, gridCoordinatesOfEmptyGrid.y + 1)].grid.transform.childCount > 0) {
                return true;
            }
        }
        return false;
    }
    private bool CheckIfLowerNeightborGridIsOccupied(Transform aGrid) {
        Vector2 gridCoordinatesOfEmptyGrid = aGrid.GetComponent<GridManager>().id;
        if (gridCoordinatesOfEmptyGrid.y - 1 < gridSize.y) {
            if (gridsAndContents[new Vector2(gridCoordinatesOfEmptyGrid.x, gridCoordinatesOfEmptyGrid.y - 1)].grid.transform.childCount > 0) {
                return true;
            }
        }
        return false;
    }
    private IEnumerator LerpContentsDownOnColumn(Transform aTargetHex, Transform aTargetGrid) {
        if (aTargetHex.parent.GetComponent<GridManager>().id.y < gridSize.y) { // execute within the grid system boundaries
            Vector2 gridCoordinatesOfUpperHex = aTargetHex.parent.GetComponent<GridManager>().id;
            Transform nextHex = gridCoordinatesOfUpperHex.y + 1 < gridSize.y ? gridsAndContents[new Vector2(gridCoordinatesOfUpperHex.x, gridCoordinatesOfUpperHex.y + 1)].grid.transform.GetChild(0) : null;
            Vector2 gridCoordinatesOfUpperEmptyGrid = aTargetGrid.GetComponent<GridManager>().id;
            Transform nextGrid = gridCoordinatesOfUpperEmptyGrid.y + 1 < gridSize.y ? gridsAndContents[new Vector2(gridCoordinatesOfUpperEmptyGrid.x, gridCoordinatesOfUpperEmptyGrid.y + 1)].grid.transform : null;

            Vector3 initialPos = aTargetHex.transform.position;
            Vector3 finalPos = aTargetGrid.transform.position;

            float timer = 0;
            float animationDuration = 1f;
            while (timer < animationDuration) {
                aTargetHex.transform.position = Vector3.Lerp(initialPos, finalPos, timer / animationDuration);
                timer += Time.deltaTime;
                yield return new WaitForSeconds(0.05f);
            }
            aTargetHex.transform.position = finalPos;
            aTargetHex.transform.SetParent(aTargetGrid);

            StopCoroutine(LerpContentsDownOnColumn(aTargetHex, aTargetGrid));

            // lerp down the next (upper) hex
            if (nextHex!=null && nextGrid!=null) {
                StartCoroutine(LerpContentsDownOnColumn(nextHex, nextGrid));
            }


        } else {
            StopCoroutine(LerpContentsDownOnColumn(aTargetHex, aTargetGrid));

        }

    }


    public void GenerateNewObjectsForColumn(Transform[] aGridGroup) {
        // we have to generate new hexes at the max grid at the related column
        // so, the essential info is the coordinates of the grid on that column
        for (int i = 0; i < aGridGroup.Length; i++) {
            int column = (int)aGridGroup[i].GetComponent<GridManager>().id.x;
            Vector2 generationGridIndex = new Vector2(column, gridSize.y - 1);
            GameObject newHex = generationPool[0].gameObject;
            generationPool.RemoveAt(0);

            Transform generationGrid = gridsAndContents[generationGridIndex].grid.transform;
            newHex.transform.position = generationGrid.transform.position;
            newHex.SetActive(true);

            Vector2 targetGridIndex = new Vector2(generationGridIndex.x, generationGridIndex.y-1);

            StartCoroutine(LerpGenerationsDownOnColumn(newHex.transform, targetGridIndex));
        }
    }
    private IEnumerator LerpGenerationsDownOnColumn(Transform aTargetHex, Vector2 aTargetGridIndex) {
        if (aTargetGridIndex.y >= 0) {

            if (gridsAndContents[aTargetGridIndex].grid.transform.childCount == 0) {
                Vector3 initialPos = aTargetHex.transform.position;
                Vector3 finalPos = gridsAndContents[aTargetGridIndex].grid.transform.position;

                float timer = 0;
                float animationDuration = 1f;
                while (timer < animationDuration) {
                    aTargetHex.transform.position = Vector3.Lerp(initialPos, finalPos, timer / animationDuration);
                    timer += Time.deltaTime;
                    yield return new WaitForSeconds(0.05f);
                }
                aTargetHex.transform.position = finalPos;

                StopCoroutine(LerpGenerationsDownOnColumn(aTargetHex, aTargetGridIndex));

                Vector3 nextGridIndex = new Vector3(aTargetGridIndex.x, aTargetGridIndex.y - 1);
                StartCoroutine(LerpGenerationsDownOnColumn(aTargetHex, nextGridIndex));

            } else {
                Vector3 upperGridIndex = new Vector2(aTargetGridIndex.x, aTargetGridIndex.y + 1);
                aTargetHex.SetParent(gridsAndContents[upperGridIndex].grid.transform);
                StopCoroutine(LerpGenerationsDownOnColumn(aTargetHex, aTargetGridIndex));
            }


        } else {
            StopCoroutine(LerpGenerationsDownOnColumn(aTargetHex, aTargetGridIndex));

        }
    }

    public void IncreaseScore() {
        score += 5;
        PlayerUIManager.puim.SetScore(score.ToString());
    }
}
