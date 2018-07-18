using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour {
    //================================================================
    //public
    public bool IsDisplayCredit; //クレジットを表示しているか

    //================================================================
    //private
    private int selectNum; //選択番号
    private Vector3[] goalPos = new Vector3[5]; //セレクトメニューの最終位置

    private bool isStart; //スタートフラグ
    private bool onceAnim; //一回だけアニメーションを起動する
    private bool isTitleSelectable; //メニューが操作可能なら

    private int inputInterval; //入力インターバル

    //================================================================
    //初期化
    private void Initialize () {
        IsDisplayCredit = false;

        selectNum = 2;
        goalPos[0] = new Vector3(-685,-320,0);
        goalPos[1] = new Vector3(-485,-320,0);
        goalPos[2] = new Vector3(-170,-200,0);
        goalPos[3] = new Vector3(170,-200,0);
        goalPos[4] = new Vector3(510,-200,0);
        isStart = false;
        isTitleSelectable = false;

        inputInterval = 0;
    }

    //================================================================
    //コンポーネント
    private List<TitleSelect> titleSelect = new List<TitleSelect>();
    private GameObject startText;
    private InputManager inputManager;
    private ParameterManager parametermanager;

    //================================================================
    //コンポーネント参照用
    private void ComponentRef () {
        titleSelect.Add(transform.Find("Select/1").GetComponent<TitleSelect>());
        titleSelect.Add(transform.Find("Select/2").GetComponent<TitleSelect>());
        titleSelect.Add(transform.Find("Select/3").GetComponent<TitleSelect>());
        titleSelect.Add(transform.Find("Select/4").GetComponent<TitleSelect>());
        titleSelect.Add(transform.Find("Select/5").GetComponent<TitleSelect>());

        startText = transform.Find("Start").gameObject;

        inputManager = GameObject.Find("InputManager").GetComponent<InputManager>();
        parametermanager = GameObject.Find("ParameterManager").GetComponent<ParameterManager>();
    }

    //==============================================================
    //パラメータマネージャーが存在するか確認する
    private void CheckPreloadParameterManager () {
        if(GameObject.FindWithTag("Parameter") == null) {
            GameObject obj = Instantiate(Resources.Load("Prefabs/ParameterManager")) as GameObject;
            obj.name = "ParameterManager";
        }
    }

    //==============================================================
    //インプットマネージャーが存在するか確認する
    private void CheckPreloadImputManager () {
        if(GameObject.FindWithTag("Input") == null) {
            GameObject obj = Instantiate(Resources.Load("Prefabs/InputManager")) as GameObject;
            obj.name = "InputManager";
        }
    }

    //================================================================
    //メイン
    private void Awake () {
        CheckPreloadParameterManager();
        CheckPreloadImputManager();
        ComponentRef();
        Initialize();
    }

    private void Start () {
    }

    private void Update () {
        //スタートフラグがたっているなら
        if(isStart) {
            //アニメーションを開始していないなら
            if(onceAnim == false) {

                //アニメーションを開始
                StartCoroutine(MoveAnim_start());
                for(int i = 0;i < titleSelect.Count;i++) {
                    StartCoroutine(MoveAnim_select(i,i * 0.2f));
                }
                onceAnim = true;
            }

            //セレクトメニューが最終位置になったなら + クレジット表示していないなら + インプットインターバルが0なら
            if(isTitleSelectable && IsDisplayCredit == false && inputInterval == 0) {
                if(inputManager.InputGetter("b1")) {
                    switch(selectNum) {
                        //使用素材
                        case 0:
                        IsDisplayCredit = true;
                        break;

                        //練習(仮)
                        case 1:
                        parametermanager.Mode = "P";
                        StartCoroutine(titleSelect[selectNum].Tobig());
                        isTitleSelectable = false;
                        break;

                        //A
                        case 2:
                        parametermanager.Mode = "A";
                        StartCoroutine(titleSelect[selectNum].Tobig());
                        isTitleSelectable = false;
                        break;

                        //B
                        case 3:
                        parametermanager.Mode = "B";
                        StartCoroutine(titleSelect[selectNum].Tobig());
                        isTitleSelectable = false;
                        break;

                        //C
                        case 4:
                        parametermanager.Mode = "C";
                        StartCoroutine(titleSelect[selectNum].Tobig());
                        isTitleSelectable = false;
                        break;

                        default:
                        break;
                    }

                    inputInterval++;
                }

                if(inputManager.InputGetter("left") || inputManager.InputGetter("down")) {
                    selectNum--;
                    if(selectNum < 0) {
                        selectNum = 4;
                    }

                    inputInterval++;
                }

                if(inputManager.InputGetter("right") || inputManager.InputGetter("up")) {
                    selectNum++;
                    if(selectNum >= 5) {
                        selectNum = 0;
                    }

                    inputInterval++;
                }

                //選択の適用
                for(int i = 0;i < 5;i++) {
                    titleSelect[i].IsSelect = false;
                }
                titleSelect[selectNum].IsSelect = true;
            }

        } else { //スタートフラグがたっていないなら
            if(inputManager.InputGetter("b1")) {
                isStart = true;
            }
        }

        //インプットインターバルに関する処理
        if(inputInterval >= 1) {
            inputInterval++;
            if(inputInterval >= ValueDefinition.INPUT_INTERVAL_TITLE) {
                inputInterval = 0;
            }
        }
    }

    //================================================================
    //スタート(テキスト)のアニメーション
    private IEnumerator MoveAnim_start () {
        float time = 0;
        float time2 = 0.5f;

        //左に移動
        while(true) {
            time += Time.deltaTime;
            time2 += Time.deltaTime * 100;
            startText.GetComponent<RectTransform>().localPosition += Vector3.right * time2;

            if(time >= 1) {
                break;
            }

            yield return new WaitForEndOfFrame();
        }

        yield break;
    }

    //================================================================
    //セレクトメニューのアニメーション
    //num -> 番号
    private IEnumerator MoveAnim_select (int num,float timeDelay) {
        float time = -timeDelay;
        Vector3 prePos = titleSelect[num].GetComponent<RectTransform>().localPosition;

        //順々に下から移動
        while(true) {
            time += Time.deltaTime * 2;
            titleSelect[num].GetComponent<RectTransform>().localPosition = Vector3.Slerp(prePos,goalPos[num],time);

            if(time >= 1) {
                titleSelect[num].GetComponent<RectTransform>().localPosition = goalPos[num];
                time = 0;
                break;
            }

            yield return new WaitForEndOfFrame();
        }

        //少し待機
        yield return new WaitForSeconds(0.2f);
        isTitleSelectable = true;

        yield break;
    }
}
