using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.UI;
//using SystemCollections.Generic;

public class TextBoxCursor : MonoBehaviour {
    //================================================================
    float time;
    Vector3 prePos;

    //================================================================
    //メイン
    private void Awake () {
        time = 0;
        prePos = GetComponent<RectTransform>().localPosition;
    }

    private void Update () {
        time += Time.deltaTime * 360;

        GetComponent<RectTransform>().localPosition = prePos + Vector3.down * 5 * Mathf.Sin(Mathf.Deg2Rad * time);
    }
}
