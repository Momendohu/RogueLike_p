using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecoverEffect : MonoBehaviour {
    //================================================================
    //コンポーネント
    private SpriteRenderer spriteRenderer;

    //================================================================
    //コンポーネント参照用
    private void ComponentRef () {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    //================================================================
    //private
    private float time;

    //================================================================
    //初期化
    private void Initialize () {
        time = 0;
    }

    //================================================================
    //メイン
    private void Start () {
        ComponentRef();
        Initialize();
    }

    private void Update () {
        spriteRenderer.color = new Color(1,1,1,3 - time);

        time += Time.deltaTime * 6;
        if(time >= 3) {
            Destroy(this.gameObject);
        }
    }
}
