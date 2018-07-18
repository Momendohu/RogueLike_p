using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.UI;
//using SystemCollections.Generic;

public class ActionSaver : MonoBehaviour {
    //=================================================================
    //コンポーネント
    private NeuralNetworkManager neuralNetworkManager; //ニューラルネットワークマネージャー
    private ParameterManager parameterManager; //パラメータマネージャー
    private Manager manager; //マネージャー
    private TextController textController; //テキストコントローラー

    //=================================================================
    //コンポーネント参照用
    private void ComponentRef () {
        neuralNetworkManager = GameObject.Find("NeuralNetworkManager").GetComponent<NeuralNetworkManager>();
        parameterManager = GameObject.Find("ParameterManager").GetComponent<ParameterManager>();
        manager = GameObject.Find("Manager").GetComponent<Manager>();
        textController = transform.Find("TextController").GetComponent<TextController>();
    }

    //================================================================
    //private
    private List<int> move = new List<int>();
    private List<int> useItem = new List<int>();
    private List<int> attack = new List<int>();
    private List<int> step = new List<int>();

    private double moveProb;
    private double useItemProb;
    private double attackProb;
    private double stepProb;

    //================================================================
    //初期化
    private void Initialize () {
        for(int i = move.Count - 1;i >= 0;i--) {
            move.RemoveAt(i);
        }

        for(int i = useItem.Count - 1;i >= 0;i--) {
            useItem.RemoveAt(i);
        }

        for(int i = attack.Count - 1;i >= 0;i--) {
            attack.RemoveAt(i);
        }

        for(int i = step.Count - 1;i >= 0;i--) {
            step.RemoveAt(i);
        }
    }

    //================================================================
    //メイン
    private void Start () {
        ComponentRef();
    }

    //================================================================
    public void AddAction (int _type) {
        switch(_type) {
            case 0:
            move.Add(_type);
            break;

            case 1:
            useItem.Add(_type);
            break;

            case 2:
            attack.Add(_type);
            break;

            case 3:
            step.Add(_type);
            break;

            default:
            Debug.Log("ERROR");
            break;
        }
    }

    //================================================================
    public void ApplyActionToNeuralNetwork () {
        if(parameterManager.Mode == "A") {
            //Debug.Log(manager.Turn);
            //Debug.Log(move.Count + ":::" + useItem.Count + ":::" + attack.Count + ":::" + step.Count);

            moveProb = move.Count / (double)(manager.Turn - 1);
            useItemProb = useItem.Count / (double)(manager.Turn - 1);
            attackProb = attack.Count / (double)(manager.Turn - 1);
            stepProb = step.Count / (double)(manager.Turn - 1);

            Debug.Log(moveProb + "::" + useItemProb + "::" + attackProb + "::" + stepProb);

            neuralNetworkManager.SetInput(0,moveProb);
            neuralNetworkManager.SetInput(1,useItemProb);
            neuralNetworkManager.SetInput(2,attackProb);
            neuralNetworkManager.SetInput(3,stepProb);

            //テキストに情報を保存
            textController.WriteText(parameterManager.NeuralNetworkID + 1 + "," + DateTime.Now.ToString("yyyy : MM : dd : HH : mm : ss") + "," + parameterManager.PlayingSkill + "," + (manager.Turn - 1) + "," + move.Count + "," + useItem.Count + "," + attack.Count + "," + step.Count + "," + moveProb + "," + useItemProb + "," + attackProb + "," + stepProb);
            parameterManager.PlayingSkill= 0;
            Initialize();
        }
    }
}
