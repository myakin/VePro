using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

    public static GameController gc;

    public Vector2 gridSize = new Vector2(8, 9);
    public Transform backgroundObject;
    public Transform outliner;
    private Transform rotator;

    private float sqrRoot3 = Mathf.Sqrt(3);

    private Dictionary<string, GridHexPair> gridsAndContents = new Dictionary<string, GridHexPair>();

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
                } else if(randomlyDefinedColorChoice == 3) {
                    hex = Instantiate(Resources.Load("HexPurple") as GameObject, gridObject.transform);
                }

                GridHexPair newGridHexPair = new GridHexPair(gridObject, hex);
                gridsAndContents.Add(x.ToString() + "-" + y.ToString(), newGridHexPair);

                // now that we have our grid, we have to re-scale our object to fit perfectly to the grid
                // since all of our hex objects are designed by 1 unit side length, it is ok to simply scale the object by sideLength, which is our calculated side length
                hex.transform.localScale = new Vector3(sideLength, sideLength, sideLength);

            }
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
    public void SetRotatorPositionForHexObject(HexObject aHexObjectManager) {
        rotator.transform.position = aHexObjectManager.GetCornerPos();
        rotator.transform.localScale = new Vector3(aHexObjectManager.sideLength, aHexObjectManager.sideLength, aHexObjectManager.sideLength);
        rotator.gameObject.SetActive(true);
    }
    */
    public void SetOutlinerPosition(HexObject aHexObjectManager) {
        outliner.position = aHexObjectManager.transform.position;
        outliner.rotation = Quaternion.Euler(0, 0, aHexObjectManager.ExecuteRotatorAngle());
        outliner.localScale = new Vector3(aHexObjectManager.GetSideLength(), aHexObjectManager.GetSideLength(), aHexObjectManager.GetSideLength());
        outliner.gameObject.SetActive(true);
        PrepareForRotation(aHexObjectManager);
    }
    private void PrepareForRotation(HexObject aHexObjectManager) {
        // rotation has to be made by parenting hex objects under a GameObject whose scale is Vector3.one
        // otherwise, deformations on meshes will be seen
        // so, for this, we will first SetRotationPivot(), then aHexObjectManager.GetSelectedGridIndexes(), 
        // then serch for those indexes in gridsAndContents, and parent the values from gridsAndContents under rotator
        // and set rotator's RotationManager values
        SetRotationPivot();

        Vector2[] selectedGrids = aHexObjectManager.GetSelectedGridIndexes();
        for (int i = 0; i < selectedGrids.Length; i++) {
            GameObject targetObj = gridsAndContents[selectedGrids[i].x.ToString() + "-" + selectedGrids[i].y].hex;
            targetObj.transform.SetParent(rotator);
            rotator.GetComponent<RotationManager>().SetGrid(selectedGrids[i], i);
            rotator.GetComponent<RotationManager>().SetObject(targetObj, i);
        }

    }
 

    private void SetRotationPivot() {
        if (rotator==null) {
            rotator = new GameObject("Rotator").transform;
            rotator.gameObject.AddComponent<RotationManager>();
        }
        rotator.position = outliner.transform.GetChild(0).position;
        rotator.rotation = outliner.transform.GetChild(0).rotation;
    }

    public void DumpOutlinedObjects() {
        if (rotator!=null) {
            for (int i = 0; i < rotator.GetComponent<RotationManager>().grids.Length; i++) {
                string key = rotator.GetComponent<RotationManager>().grids[i].x.ToString() + "-" + rotator.GetComponent<RotationManager>().grids[i].y.ToString();
                gridsAndContents[key].hex = rotator.GetComponent<RotationManager>().attachedObjects[i];
                gridsAndContents[key].hex.transform.SetParent(gridsAndContents[key].grid.transform);
            }
        }
    }


}
