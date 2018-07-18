using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.UI;
//using SystemCollections.Generic;

public class Credit : MonoBehaviour {
    //================================================================
    //private
    private bool isDisplayedAll; //素材がすべて表示されている状態なら
    private bool onceMoveAll; //一回だけクレジット全体を動かす
    private bool onceMoveReturnObject; //一回だけ戻る表示を動かす

    //================================================================
    //初期化
    private void Initialize () {
        isDisplayedAll = false;
        onceMoveAll = false;
        onceMoveReturnObject = false;
    }

    //================================================================
    //コンポーネント
    private TitleManager titleManager; //タイトルマネージャー
    private GameObject returnObject; //戻るのオブジェクト
    private GameObject background; //背景
    private InputManager inputManager; //インプットマネージャー

    //================================================================
    //コンポーネント参照用
    private void ComponentRef () {
        titleManager = transform.parent.GetComponent<TitleManager>();
        returnObject = transform.Find("Return").gameObject;
        background = transform.Find("Background").gameObject;
        inputManager = GameObject.Find("InputManager").GetComponent<InputManager>();
    }

    //================================================================
    //メイン
    private void Start () {
        ComponentRef();
    }

    private void Update () {
        //使用素材表示フラグが立ったら
        if(titleManager.IsDisplayCredit) {
            //使用素材が全て表示されているなら
            if(isDisplayedAll) {
                if(inputManager.InputGetter("b1")) {
                    if(onceMoveReturnObject == false) {
                        StartCoroutine(MoveAll("down"));
                        onceMoveReturnObject = true;
                    }
                }
            } else { //使用材料が表示されていないなら
                if(onceMoveAll == false) {
                    StartCoroutine(MoveAll("up"));
                    onceMoveAll = true;
                }
            }
        }
    }

    //================================================================
    //画面全体を動かす
    //direction -> 方向
    private IEnumerator MoveAll (string direction) {
        float time = 0;

        Vector3 prePos = Vector3.zero;
        Vector3 goalPos = Vector3.zero;
        if(direction == "up") {
            prePos = new Vector3(0,-1200,0);
            goalPos = Vector3.zero;
        } else if(direction == "down") {
            prePos = Vector3.zero;
            goalPos = new Vector3(0,-1200,0);
        }

        while(true) {
            GetComponent<RectTransform>().localPosition = Vector3.Lerp(prePos,goalPos,time);

            time += Time.deltaTime * 2;
            if(time >= 1) {
                GetComponent<RectTransform>().localPosition = goalPos;
                break;
            }

            yield return new WaitForEndOfFrame();
        }

        //(上昇時)使用素材全表示フラグを立たせる
        if(direction == "up") {
            isDisplayedAll = true;
        }

        //(下降時)初期化
        if(direction == "down") {
            Initialize();
            titleManager.IsDisplayCredit = false;
        }

        yield break;
    }
}
