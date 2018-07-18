using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Focus : MonoBehaviour {
    //================================================================
    //public
    public bool IsFocus;

    //================================================================
    //初期化
    private void Initialize () {
        IsFocus = false;
    }

    //================================================================
    //メイン
    private void Awake () {
        Initialize();
    }

    private void Update () {
        if(IsFocus) {
            transform.Find("Image").gameObject.SetActive(true);
        } else {
            transform.Find("Image").gameObject.SetActive(false);
        }
    }
}
