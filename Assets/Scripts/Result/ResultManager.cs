using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class ResultManager : MonoBehaviour {
    //==============================================================
    //コンポーネント参照
    private ParameterManager parameterManager;
    private Image backgroundImage;
    private GameObject result1;
    private GameObject result2;
    private GameObject result3;
    private GameObject result4;
    private InputManager inputManager; //インプットマネージャー
    private SoundManager soundManagerBGM; //BGM
    private TextController textController;

    //==============================================================
    //コンポーネント参照用
    private void ComponentRef () {
        parameterManager = GameObject.Find("ParameterManager").GetComponent<ParameterManager>();
        backgroundImage = transform.Find("Background/Image2").GetComponent<Image>();
        result1 = transform.Find("Result1").gameObject;
        result2 = transform.Find("Result2").gameObject;
        result3 = transform.Find("Result3").gameObject;
        result4 = transform.Find("Result4").gameObject;
        inputManager = GameObject.Find("InputManager").GetComponent<InputManager>();
        soundManagerBGM = GameObject.Find("SoundManagerBGM").GetComponent<SoundManager>();
        textController = transform.Find("Result").GetComponent<TextController>();
    }

    //================================================================
    //メイン
    private void Awake () {
        CheckPreloadImputManager();
        CheckPreloadParameterManager();
        ComponentRef();
    }

    private void Start () {
        result1.GetComponent<Text>().text = parameterManager.playerParameters.Name + "は" + "\n" + parameterManager.StageName + "の" + parameterManager.Floor + "階まで到達した";
        result2.GetComponent<Text>().text = "レベル " + parameterManager.playerParameters.Level;
        result3.GetComponent<Text>().text = "使用したアイテム数 " + parameterManager.playerParameters.UseItemNum + "個";
        result4.GetComponent<Text>().text = "倒した敵 " + parameterManager.DefeatEnemyNum + "体";

        soundManagerBGM.Trigger(1,false);
        StartCoroutine(BackgroundImageAnimation());

        StartCoroutine(ResultTextAnimation(result1,1.3f));
        StartCoroutine(ResultTextAnimation(result2,1.4f));
        StartCoroutine(ResultTextAnimation(result3,1.5f));
        StartCoroutine(ResultTextAnimation(result4,1.6f));

        SaveResult();
    }

    private void Update () {
        if(inputManager.InputGetter("b1")) {

            //パラメータの初期化
            parameterManager.Initialize();

            soundManagerBGM.StopMusic(1);

            //タイトルへ遷移
            parameterManager.TransPattern = 4;
            SceneManager.LoadScene("Loading");
        }
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

    //==============================================================
    //バックグラウンドの画像のアニメーション処理
    private IEnumerator BackgroundImageAnimation () {
        float time = 0;

        backgroundImage.color = new Color(1,1,1,1);

        while(true) {
            backgroundImage.color = new Color(1,1,1,2 - time);

            time += Time.deltaTime;
            if(time >= 1.5f) {
                break;
            }

            yield return new WaitForEndOfFrame();
        }
        yield break;
    }

    //==============================================================
    //リザルト表示テキストのアニメーション
    // _obj -> 動かすオブジェクト
    // _startDisplayTime -> 表示を始めるタイミング
    private IEnumerator ResultTextAnimation (GameObject _obj,float _startDisplayTime) {
        float time = 0;
        Vector3 prePos = _obj.GetComponent<RectTransform>().localPosition;
        Color preColor = _obj.GetComponent<Text>().color;
        _obj.GetComponent<Text>().color = new Color(preColor.r,preColor.g,preColor.b,0);

        //待機
        yield return new WaitForSeconds(_startDisplayTime);

        while(true) {
            _obj.transform.GetComponent<RectTransform>().localPosition = Vector3.Lerp(prePos + new Vector3(-100,0,0),prePos,time);
            _obj.GetComponent<Text>().color = new Color(preColor.r,preColor.g,preColor.b,time * 2);

            time += Time.deltaTime;
            if(time >= 1) {
                _obj.GetComponent<Text>().color = new Color(preColor.r,preColor.g,preColor.b,1);
                break;
            }

            yield return new WaitForEndOfFrame();
        }
    }

    //================================================================
    public void SaveResult () {
        //テキストに情報を保存
        textController.WriteText(
            DateTime.Now.ToString("yyyy : MM : dd : HH : mm : ss") + "," +
            parameterManager.Mode + "," +
            parameterManager.playerParameters.Name + "," +
            parameterManager.StageName + "," +
            parameterManager.Floor + "," +
            parameterManager.playerParameters.Level + "," +
            parameterManager.playerParameters.UseItemNum + "," +
            parameterManager.DefeatEnemyNum
            );
    }
}
