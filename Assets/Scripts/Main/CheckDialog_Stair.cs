using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.UI;
//using SystemCollections.Generic;

public class CheckDialog_Stair : MonoBehaviour {
    //================================================================
    //参照
    private GameObject cursor;
    private GameObject frame;
    private Manager manager;
    private InputManager inputManager;
    private ParameterManager parameterManager;
    private NeuralNetworkManager neuralNetworkManager;
    private ActionSaver actionSaver;

    //================================================================
    //参照用
    private void ComponentRef () {
        cursor = transform.Find("Frame/Cursor").gameObject;
        frame = transform.Find("Frame").gameObject;
        manager = GameObject.Find("Manager").GetComponent<Manager>();
        inputManager = GameObject.Find("InputManager").GetComponent<InputManager>();
        parameterManager = GameObject.Find("ParameterManager").GetComponent<ParameterManager>();
        neuralNetworkManager = GameObject.Find("NeuralNetworkManager").GetComponent<NeuralNetworkManager>();
        actionSaver = GameObject.Find("ActionSaver").GetComponent<ActionSaver>();
    }

    //================================================================
    //private 
    private int cursorPoint;
    private int inputInterval;

    private void Initialize () {
        cursorPoint = 0;
        inputInterval = 0;
        frame.SetActive(false);
    }

    //================================================================
    //メイン
    private void Start () {
        ComponentRef();
        Initialize();
    }

    private void Update () {
        //メニュー操作フラグが立って、かつ行動キューがたまっていなければ
        if(manager.IsOperateOptionMenu && manager.ActQueue.Count == 0) {
            frame.SetActive(true);
            Cursor();
        }
    }

    //=================================================================
    //カーソルの処理
    private void Cursor () {
        if(inputInterval <= 0) {
            //上にカーソルを移動
            if(inputManager.InputGetter("up")) {
                cursorPoint--;
                if(cursorPoint <= -1) {
                    cursorPoint = 1;
                }

                inputInterval++;
            }

            //下にカーソルを移動
            if(inputManager.InputGetter("down")) {
                cursorPoint++;
                if(cursorPoint >= 2) {
                    cursorPoint = 0;
                }

                inputInterval++;
            }

            //カーソル位置に応じた処理
            switch(cursorPoint) {
                case 0:
                cursor.GetComponent<RectTransform>().localPosition = new Vector3(-110,40,0);

                if(inputManager.InputGetter("b1")) {
                    //プレイングバイアス
                    neuralNetworkManager.ApplyPlayingBias(ValueDefinition.PLAYING_BIAS_NEXT_FLOOR);

                    //ニューラルネットワークに行動を入力として適用する
                    actionSaver.ApplyActionToNeuralNetwork();
                    parameterManager.TransPattern = 2;
                    manager.ActQueue.Add(manager.FadeScene("GameMain"));
                }
                break;

                case 1:
                cursor.GetComponent<RectTransform>().localPosition = new Vector3(-110,-40,0);

                //メニューを閉じる
                if(inputManager.InputGetter("b1")) {
                    manager.IsOperateOptionMenu = false;
                    frame.SetActive(false);
                }
                break;

                default:
                break;
            }
        }

        //メニューを閉じる
        if(inputManager.InputGetter("b3")) {
            manager.IsOperateOptionMenu = false;
            frame.SetActive(false);
        }

        if(inputInterval >= 1) {
            inputInterval++;
            if(inputInterval >= ValueDefinition.INPUT_INTERVAL_CHECK_STAIR_DIALOG) {
                inputInterval = 0;
            }
        }
    }
}
