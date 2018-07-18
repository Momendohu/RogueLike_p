using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.UI;
//using SystemCollections.Generic;

public class ItemUseDialog : MonoBehaviour {
    //================================================================
    //コンポーネント
    private Manager manager; //マネージャー
    private ParameterManager parameterManager; //パラメータマネージャー
    private GameObject cursor; //カーソル
    private GameObject frame; //フレーム
    private ItemDisplayer itemDisplayer; //アイテム表示
    private TextBox textBox; //テキストボックス
    private InputManager inputManager; //インプットマネージャー
    private ActionSaver actionSaver; //アクションセーバー
    private SoundManager soundManagerSE; //サウンドエフェクト用のサウンドマネージャー

    //================================================================
    //コンポーネント参照用
    private void ComponentRef () {
        manager = GameObject.Find("Manager").GetComponent<Manager>();
        parameterManager = GameObject.Find("ParameterManager").GetComponent<ParameterManager>();
        cursor = transform.Find("Frame/Cursor").gameObject;
        frame = transform.Find("Frame").gameObject;
        itemDisplayer = transform.parent.parent.GetComponent<ItemDisplayer>();
        textBox = GameObject.Find("Canvas/TextBox").GetComponent<TextBox>();
        inputManager = GameObject.Find("InputManager").GetComponent<InputManager>();
        actionSaver = GameObject.Find("ActionSaver").GetComponent<ActionSaver>();
        soundManagerSE = GameObject.Find("SoundManagerSE").GetComponent<SoundManager>();
    }

    //================================================================
    //private 
    private int cursorPoint;
    private int selectInterval; //入力の重なりの回避用
    private int inputInterval; //単純操作のインターバル

    private void Initialize () {
        cursorPoint = 0;
        selectInterval = 0;
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
        if(manager.IsDisplayingCheckUseItemDialog) {
            frame.SetActive(true);
            Cursor();
            selectInterval++;
        } else {
            selectInterval = 0;
            frame.SetActive(false);
        }
    }

    //=================================================================
    //カーソルの処理
    private void Cursor () {
        //上にカーソルを移動
        if(inputManager.InputGetter("up") && inputInterval <= 0) {
            soundManagerSE.SetVolume(0.3f,6);
            soundManagerSE.TriggerSE(6);

            cursorPoint--;
            if(cursorPoint <= -1) {
                cursorPoint = 1;
            }

            inputInterval++;
        }

        //下にカーソルを移動
        if(inputManager.InputGetter("down") && inputInterval <= 0) {
            soundManagerSE.SetVolume(0.3f,6);
            soundManagerSE.TriggerSE(6);

            cursorPoint++;
            if(cursorPoint >= 2) {
                cursorPoint = 0;
            }

            inputInterval++;
        }

        if(inputInterval >= 1) {
            inputInterval++;
            if(inputInterval >= ValueDefinition.INPUT_INTERVSL_OPERATE_ITEM_USE_DIALOG) {
                inputInterval = 0;
            }
        }

        cursor.GetComponent<RectTransform>().localPosition = new Vector3(-176,-3 - cursorPoint * 40.5f,0);

        //進めるキー入力を行ったら
        if(inputManager.InputGetter("b1") && selectInterval >= ValueDefinition.INPUT_INTERVSL_WAIT_ITEM_USE_DIALOG) {
            //はい
            if(cursorPoint == 0) {
                manager.ActQueue.Add(UseItemRoutine());
            } else {
                soundManagerSE.TriggerSE(7);
                Initialize();
                manager.IsDisplayingCheckUseItemDialog = false;
            }

            selectInterval = 0;
        }

        //戻るキー入力を行ったら
        if(inputManager.InputGetter("b3")) {
            Initialize();
            manager.IsDisplayingCheckUseItemDialog = false;
        }
    }

    //=================================================================
    public IEnumerator UseItemRoutine () {
        //使用アイテムのIDを一時保存
        int usedItemId = parameterManager.playerParameters.HoldItems[itemDisplayer.CursorPoint_UseItemIndex];

        //アイテムの使用
        parameterManager.playerParameters.UseItem(itemDisplayer.CursorPoint_UseItemIndex);
        //Debug.Log(usedItemId);

        //エフェクト
        manager.Player.GetComponent<Player>().CreateEffect(usedItemId);

        //アクションセーバーに適用
        actionSaver.AddAction(1);

        if(usedItemId != 2) {
            //テキストボックス表示
            textBox.Apply(ValueDefinition.ITEMS_USED_TEXT[usedItemId],false);
        }

        //初期化
        itemDisplayer.Initialize();
        Initialize();
        manager.IsDisplayingItemDisplayer = false;

        //ターン処理
        manager.WasPlayerAct = true;
        manager.IsActiceActPlayer = true;
        manager.Turn++; //ターンを追加

        //アイテム使用と敵の行動の間に時間を設ける
        yield return new WaitForSeconds(0.5f);

        manager.EndedPlayerAct = true;
        yield break;
    }
}
