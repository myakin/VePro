using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

    public static GameController gc;

    public Vector2 gridSize = new Vector2(8, 9);
    public Transform backgroundObject;

    private float sqrRoot3 = Mathf.Sqrt(3);


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
        float cellSize = GetCellSize();
        Vector3 gridStartPoint = backgroundObject.position - (backgroundObject.right * (cellSize * (gridSize.x / 4)));
        //GameObject s = new GameObject("Origin");
        //s.transform.position = gridStartPoint;

        for (int y = 0; y < gridSize.y; y++) {
            for (int x = 0; x < gridSize.x; x++) {
                float multiplierY = x % 2==0 ? 1 : 2;

                Vector2 gridInsertionPoint = new Vector2((cellSize * 0.5f) * (1 + 1.5f * x ) , (cellSize * 0.5f) * sqrRoot3 * 0.5f * multiplierY);
                Vector3 gridWorldPoint = gridStartPoint + (backgroundObject.right * gridInsertionPoint.x) + (backgroundObject.up * gridInsertionPoint.y);

                if (gridInsertionPoint.x < cellSize * gridSize.x) {
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
                    //float currentSize = hex.GetComponent<MeshRenderer>().bounds.max.x;
                    //float idealSize = cellSize / currentSize;
                    //hex.transform.localScale = new Vector3(idealSize, idealSize, idealSize);
                }
            }
        }
    }

    private float GetCellSize() {
        /*
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
        float cellWidth = screenSize.x / gridSize.x;
        return cellWidth;
        */

        float platformLength = backgroundObject.GetComponent<MeshRenderer>().bounds.max.x + Mathf.Abs(backgroundObject.GetComponent<MeshRenderer>().bounds.min.x);
        //float unitLength = (platformLength / gridSize.x) * (2f/5f);
        float unitLength = platformLength / (gridSize.x / 2f);
        return unitLength;



    }

}
