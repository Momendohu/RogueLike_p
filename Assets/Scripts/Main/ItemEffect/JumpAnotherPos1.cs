using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.UI;
//using SystemCollections.Generic;

public class JumpAnotherPos1 : MonoBehaviour {
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
    }

    private void Start () {
        StartCoroutine(Animation());
    }

    //================================================================
    private IEnumerator Animation () {
        float time = 0;

        while(true) {
            transform.eulerAngles += new Vector3(0,0,40 * (2 - time));
            spriteRenderer.color = new Color(1,1,1,2 - time);

            time += Time.deltaTime * 4;
            if(time >= 2) {
                break;
            }

            yield return new WaitForEndOfFrame();
        }

        Destroy(this.gameObject);
        yield break;
    }
}
