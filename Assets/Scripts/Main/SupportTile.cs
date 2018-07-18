using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.UI;
//using SystemCollections.Generic;

public class SupportTile : MonoBehaviour {
    //================================================================
    //public
    public bool IsMovable; //進める方向か

    //================================================================
    private void Initialize () {
        //IsMovable = false;
    }

    //================================================================
    //コンポーネント
    private SpriteRenderer spriteRenderer;

    //================================================================
    //コンポーネント参照用
    private void ComponentRef () {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    //================================================================
    //メイン
    private void Awake () {
        ComponentRef();
        Initialize();
    }

    private void Update () {
        if(IsMovable) {
            spriteRenderer.color = new Color(255f / 255,56f / 255,56f / 255,0.8f);
        } else {
            spriteRenderer.color = new Color(1,1,1,0.5f);
        }
    }
}
