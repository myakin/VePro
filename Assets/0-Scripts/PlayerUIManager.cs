using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerUIManager : MonoBehaviour {

    public static PlayerUIManager puim;

    public TextMeshProUGUI scoreTMP;

    private void Awake() {
        if (PlayerUIManager.puim==null) {
            PlayerUIManager.puim = this;
        } else {
            if (PlayerUIManager.puim!=this){
                Destroy(PlayerUIManager.puim.gameObject);
                PlayerUIManager.puim = this;
            }
        }
        DontDestroyOnLoad(PlayerUIManager.puim.gameObject);
    }

    private void Start() {
        SetScore("0");
    }

    public void SetScore(string aScore) {
        scoreTMP.SetText(aScore);
    }

}
