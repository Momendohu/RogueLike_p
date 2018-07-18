using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using SystemCollections.Generic;

public class Fade : MonoBehaviour {
    //================================================================
    //private
    private float time;

    //================================================================
    //public
    public float TimeLength;

    //================================================================
    //コンポーネント
    private Image image;

    //================================================================
    //コンポーネント参照
    private void ComponentRef () {
        image = GetComponent<Image>();
    }

    //================================================================
    //メイン
    private void Awake () {
        time = 0;
        ComponentRef();
    }

    private void Update () {
        time += Time.deltaTime / TimeLength;
        image.color = new Color(0,0,0,time / TimeLength);
    }
}
