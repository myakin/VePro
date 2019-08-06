using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectorManager : MonoBehaviour {

    public Transform catchedObject;



    private void OnTriggerEnter(Collider other) {
        if (other.tag == "Grid") {
            Debug.Log("Entered a grid");
        }
        if (other.tag == "Hex") {
            Debug.Log("Entered a hex");
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.tag == "Grid") {
            Debug.Log("Exited a grid");
        }
        if (other.tag == "Hex") {
            Debug.Log("Exited a hex");
        }
    }

    private void OnTriggerStay(Collider other) {
        if (other.tag == "Grid") {
            Debug.Log("Staying on a grid");
        }
        if (other.tag == "Hex") {
            Debug.Log("Staying on a hex");
        }
    }


}
