using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.UI;

public class StaffEffect : MonoBehaviour {
    //================================================================
    //コンポーネント
    private Transform staff;
    private Transform effect;

    //================================================================
    //コンポーネント参照用
    private void ComponentRef () {
        staff = transform.Find("1");
        effect = transform.Find("2");
    }

    //================================================================
    //メイン
    private void Awake () {
        ComponentRef();
    }

    private void Start () {
        StartCoroutine(Animation());
    }

    //=================================================================
    //アニメーション
    private IEnumerator Animation () {
        float time = 0;

        while(true) {
            staff.GetComponent<SpriteRenderer>().color = new Color(1,1,1,2 - time);
            effect.GetComponent<SpriteRenderer>().color = new Color(1,1,1,2 - time);
            staff.eulerAngles += new Vector3(0,0,35 * Mathf.Abs(1.5f - time));

            transform.position += new Vector3(0,0.003f,0);

            time += Time.deltaTime * 2;
            if(time >= 2) {
                break;
            }

            yield return new WaitForEndOfFrame();
        }

        Destroy(this.gameObject);
        yield break;
    }
}
