using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Loading : MonoBehaviour {
    //==============================================================
    //コンポーネント
    private ParameterManager parameterManager;
    private Text text;
    private NeuralNetworkManager neuralNetworkManager; //ニューラルネットワークマネージャー

    //==============================================================
    //コンポーネント参照用
    private void ComponentRef () {
        parameterManager = GameObject.Find("ParameterManager").GetComponent<ParameterManager>();
        text = GameObject.Find("Canvas/Text").GetComponent<Text>();
        neuralNetworkManager = GameObject.Find("NeuralNetworkManager").GetComponent<NeuralNetworkManager>();
    }

    //================================================================
    //private
    private int transPattern; //シーン遷移パターン
    private bool onceStartCoroutine; //一回だけコルーチンを作動させる
    private bool onceStartNeuralNetwork; //一回だけニューラルネットワークのコルーチンを作動させる

    //==============================================================
    //public
    public bool IsLoadCompleted; //ローディングが完了したか

    //================================================================
    //メイン
    private void Awake () {
        CheckPreloadImputManager();
        CheckPreloadParameterManager();
        CheckPreloadtNeuralNetworkManager();

        ComponentRef();
    }

    private void Start () {
        transPattern = parameterManager.TransPattern;
    }

    private void Update () {
        switch(transPattern) {
            //起動時
            case 0:
            if(onceStartNeuralNetwork == false) {
                StartCoroutine(WaitCalculateNeuralNetwork("First"));
                onceStartNeuralNetwork = true;
            }
            break;

            case 1:
            if(onceStartNeuralNetwork == false) {
                StartCoroutine(WaitCalculateNeuralNetwork("StartStage"));
                onceStartNeuralNetwork = true;
            }
            break;

            case 2:
            if(onceStartNeuralNetwork == false) {
                StartCoroutine(WaitCalculateNeuralNetwork("NextStage"));
                onceStartNeuralNetwork = true;
            }
            break;

            case 3:
            if(onceStartNeuralNetwork == false) {
                StartCoroutine(WaitCalculateNeuralNetwork("Result"));
                onceStartNeuralNetwork = true;
            }
            break;

            case 4:
            IsLoadCompleted = true;
            break;

            default:
            text.text = "ERROR";
            break;
        }

        if(IsLoadCompleted) {
            if(onceStartCoroutine == false) {
                StartCoroutine(CompleteLoadingRoutine(transPattern));
                onceStartCoroutine = true;
            }
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
    //ニューラルネットワークマネージャーが存在するか確認する
    private void CheckPreloadtNeuralNetworkManager () {
        if(GameObject.FindWithTag("NeuralNetwork") == null) {
            GameObject obj = Instantiate(Resources.Load("Prefabs/NeuralNetworkManager")) as GameObject;
            obj.name = "NeuralNetworkManager";
        }
    }

    //==============================================================
    //ニューラルネットワークマネージャーの処理を待機する
    private IEnumerator WaitCalculateNeuralNetwork (string _type) {
        yield return new WaitForSeconds(1);

        yield return neuralNetworkManager.Calcalate(_type);

        IsLoadCompleted = true;
        yield break;
    }

    //==============================================================
    //ローディング終了時の動作
    private IEnumerator CompleteLoadingRoutine (int _transPattern) {
        yield return new WaitForSeconds(0.1f);
        text.text = "COMPLETE!";

        float time = 0;
        while(true) {
            text.GetComponent<RectTransform>().localScale = Vector3.one * (1 + Mathf.Sin(Mathf.Deg2Rad * time) / 5);
            time += 12f;
            if(time >= 180) {
                text.GetComponent<RectTransform>().localScale = Vector3.one;
                break;
            }

            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(0.2f);

        switch(transPattern) {
            //起動時
            case 0:
            SceneManager.LoadScene("Title");
            break;

            case 1:
            SceneManager.LoadScene("GameMain");
            break;

            case 2:
            SceneManager.LoadScene("GameMain");
            break;

            case 3:
            SceneManager.LoadScene("Result");
            break;

            case 4:
            SceneManager.LoadScene("Title");
            break;

            default:
            text.text = "ERROR";
            break;
        }

        yield break;
    }
}
