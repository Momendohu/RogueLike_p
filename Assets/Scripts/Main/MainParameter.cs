using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using SystemCollections.Generic;

public class MainParameter : MonoBehaviour {
    //===============================================================
    private ParameterManager parameterManager; //パラメータマネージャー

    private Text floor;
    private Text level;
    private Text hitPoint;
    private Text onakaPoint;

    private void ComponentRef () {
        parameterManager = GameObject.Find("ParameterManager").GetComponent<ParameterManager>();

        floor = transform.Find("Frame/Floor").GetComponent<Text>();
        level = transform.Find("Frame/Level").GetComponent<Text>();
        hitPoint = transform.Find("Frame/HitPoint").GetComponent<Text>();
        onakaPoint = transform.Find("Frame2/OnakaPoint").GetComponent<Text>();
    }

    //================================================================
    //メイン
    private void Awake () {
        ComponentRef();
    }

    private void Update () {
        onakaPoint.text = "満腹度" + "<color=#ff4040>" + parameterManager.playerParameters.OnakaPoint + "</color>" + "%";
        floor.text = "<color=#ff4040>" + parameterManager.Floor + "</color>" + "F";
        level.text = "LV" + "<color=#ff4040>" + parameterManager.playerParameters.Level + "</color>";
        hitPoint.text = "HP" + "<color=#ff4040>" + parameterManager.playerParameters.HitPoint + "</color>" + "/" + "<color=#ff4040>" + parameterManager.playerParameters.MaxHitPoint + "</color>";
    }
}
