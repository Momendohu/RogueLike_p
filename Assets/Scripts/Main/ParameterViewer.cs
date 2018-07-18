using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ParameterViewer : MonoBehaviour {
    private Manager manager; //マネージャー
    private Text text_turn; //ターン表示
    private Text text_playerPos; //プレイヤーの位置情報
    private Text text_enemiesAct; //敵の行動状況
    private Text text_actQueue; //行動キュー
    private Text text_stairPos; //階段の位置情報
    private Text text_onakaPoint; //階段の位置情報
    private GameObject frame; //フレーム

    //================================================================
    //private
    private bool isActive; //表示アクティブの切り替え

    //================================================================
    //初期化
    private void Initialize () {
        isActive = false;
    }

    //================================================================
    //コンポーネント参照用
    private void ComponentRef () {
        manager = GameObject.Find("Manager").GetComponent<Manager>();
        text_turn = transform.Find("Frame/Turn").GetComponent<Text>();
        text_playerPos = transform.Find("Frame/PlayerPos").GetComponent<Text>();
        text_enemiesAct = transform.Find("Frame/EnemiesAct").GetComponent<Text>();
        text_actQueue = transform.Find("Frame/ActQueue").GetComponent<Text>();
        text_stairPos = transform.Find("Frame/StairPos").GetComponent<Text>();
        text_onakaPoint = transform.Find("Frame/OnakaPoint").GetComponent<Text>();
        frame = transform.Find("Frame").gameObject;
    }

    //================================================================
    //メイン
    private void Awake () {
        Initialize();
    }

    private void Start () {
        ComponentRef(); //パラメータマネージャーが存在するか確認するプロセスを踏むためStart内
    }

    private void Update () {
        ChangeVisible();

        text_turn.text = "Turn:" + manager.Turn;

        text_playerPos.text = "PlayerPos:"
            + (int)manager.GetPlayerPos().x + ":"
            + (int)manager.GetPlayerPos().y + ":"
            + manager.GetFieldDataConcernDivide((int)manager.GetPlayerPos().x,(int)manager.GetPlayerPos().y).x + ":"
            + manager.GetFieldDataConcernDivide((int)manager.GetPlayerPos().x,(int)manager.GetPlayerPos().y).y;

        text_stairPos.text = "StairPos:" + (int)manager.GetStairFirstPos().x + ":" + (int)manager.GetStairFirstPos().y;

        text_actQueue.text = "ActQueue:" + manager.ActQueue.Count;

        text_enemiesAct.text = "EnemiesAct:" + manager.EnemiesActCount + "/" + manager.GetEnemyNum();

        text_onakaPoint.text = "OnakaPoint:" + manager.GetOnakaPoint();
    }

    //================================================================
    //表示アクティブの切り替え処理
    private void ChangeVisible () {
        if(Input.GetKeyDown(KeyCode.T)) {
            if(isActive == false) {
                isActive = true;
            } else {
                isActive = false;
            }
        }

        if(isActive) {
            frame.SetActive(true);
        } else {
            frame.SetActive(false);
        }
    }
}
