using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.UI;
//using SystemCollections.Generic;

public class Item : MonoBehaviour {
    //================================================================
    //public
    public string Name; //名前
    public int ID; //ID
    public Vector2Int Pos = new Vector2Int(); //座標

    //================================================================
    //メイン
    private void Update () {
        //アイテムが石なら
        if(ID == 2) {
            transform.localScale = new Vector3(0.6f,0.6f,0.6f);
        }

        //アイテムが杖なら
        if(ID == 3) {
            transform.localScale = new Vector3(0.6f,0.6f,0.6f);
        }
    }
}
