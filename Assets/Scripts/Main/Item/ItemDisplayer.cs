using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemDisplayer : MonoBehaviour {
    //================================================================
    //コンポーネント
    private Manager manager; //マネージャー
    private Text text; //アイテムを表示するテキスト
    private ParameterManager parameterManager; //パラメータマネージャー
    private GameObject cursor; //カーソル
    private GameObject frame; //フレーム
    private Text descriptionText; //アイテムの説明文
    private InputManager inputManager; //インプットマネージャー
    private SoundManager soundManagerSE; //サウンドエフェクト用のサウンドマネージャー

    //================================================================
    //コンポーネント参照用
    private void ComponentRef () {
        manager = GameObject.Find("Manager").GetComponent<Manager>();
        text = transform.Find("Frame/Text").GetComponent<Text>();
        parameterManager = GameObject.Find("ParameterManager").GetComponent<ParameterManager>();
        cursor = transform.Find("Frame/Cursor").gameObject;
        frame = transform.Find("Frame").gameObject;
        descriptionText = transform.Find("Frame/Description/Text").GetComponent<Text>();
        inputManager = GameObject.Find("InputManager").GetComponent<InputManager>();
        soundManagerSE = GameObject.Find("SoundManagerSE").GetComponent<SoundManager>();
    }

    //================================================================
    //public
    public int CursorPoint_UseItemIndex;

    //================================================================
    //private
    private int inputInterval; //単純操作のインターバル
    private int openItemDisplayerInterval; //アイテム表示を出してすぐ消えないようにインターバルをもたせる

    //================================================================
    //初期化(openItemDisplayerIntervalは切り替え処理に使用するため1)

    public void Initialize () {
        CursorPoint_UseItemIndex = 0;
        inputInterval = 0;
        openItemDisplayerInterval = 1;
        descriptionText.text = "";
        manager.IsDisplayingCheckUseItemDialog = false;
        manager.IsDisplayingItemDisplayer = false;
        cursor.GetComponent<RectTransform>().localPosition = new Vector3(-272,260,0);
        cursor.SetActive(false);
        frame.SetActive(false);
    }

    //================================================================
    //メイン

    private void Start () {
        ComponentRef();
        Initialize();
    }

    private void Update () {
        //アイテム欄の更新
        text.text = "";
        for(int i = 0;i < parameterManager.playerParameters.HoldItems.Count;i++) {
            text.text += ValueDefinition.ITEMS_NAME[parameterManager.playerParameters.HoldItems[i]] + "\n";
        }

        //アイテム使用確認ダイアログが表示されているならカーソルのアクティブを切る
        if(manager.IsDisplayingCheckUseItemDialog) {
            cursor.SetActive(false);
        }

        //アイテムメニュー操作フラグが立って、アイテム使用確認ダイアログが表示されていなく、かつ行動キューがたまっていなければ
        //フレームとカーソルをアクティブにする
        if(manager.IsDisplayingItemDisplayer && manager.IsDisplayingCheckUseItemDialog == false && manager.ActQueue.Count == 0) {
            frame.SetActive(true);
            Cursor();
        }

        //アイテムメニューを開く処理
        //オプション表示なし + ステージタイトル表示なし + 行動キューがたまっていない状態 + 死亡判定なし
        if(manager.IsOperateOptionMenu == false && manager.IsDisplayingStageTitle == false && manager.ActQueue.Count == 0 && manager.IsDeadPlayer == false) {

            //特定の入力を受け取ったら
            if(inputManager.InputGetter("select") || inputManager.InputGetter("b2")) {
                if(openItemDisplayerInterval <= 0) {
                    if(manager.IsDisplayingItemDisplayer == false) {
                        frame.SetActive(true);
                        manager.IsDisplayingItemDisplayer = true;
                    } else {
                        Initialize();
                    }

                    openItemDisplayerInterval++;
                }
            }
        }

        //アイテムメニュー表示時のインターバルが進行しているなら
        if(openItemDisplayerInterval >= 1) {
            openItemDisplayerInterval++;
            if(openItemDisplayerInterval >= ValueDefinition.INPUT_INTERVAL_OPEN_ITEM_DISPLAYER) {
                openItemDisplayerInterval = 0;
            }
        }
    }

    //=================================================================
    //カーソルの処理
    private void Cursor () {
        //戻るキー入力を行ったら
        if(inputManager.InputGetter("b3") && inputInterval <= 0) {
            Initialize();
        }

        //持ってるアイテム数が1以上なら
        if(parameterManager.playerParameters.HoldItems.Count >= 1) {
            cursor.SetActive(true);
            descriptionText.text = ValueDefinition.ITEMS_DESCRIPTION[parameterManager.playerParameters.HoldItems[CursorPoint_UseItemIndex]];

            //上にカーソルを移動
            if(inputManager.InputGetter("up") && inputInterval <= 0) {
                soundManagerSE.SetVolume(0.3f,6);
                soundManagerSE.TriggerSE(6);

                CursorPoint_UseItemIndex--;
                if(CursorPoint_UseItemIndex <= -1) {
                    CursorPoint_UseItemIndex = parameterManager.playerParameters.HoldItems.Count - 1;
                }

                inputInterval++;
            }

            //下にカーソルを移動
            if(inputManager.InputGetter("down") && inputInterval <= 0) {
                soundManagerSE.SetVolume(0.3f,6);
                soundManagerSE.TriggerSE(6);

                CursorPoint_UseItemIndex++;
                if(CursorPoint_UseItemIndex >= parameterManager.playerParameters.HoldItems.Count) {
                    CursorPoint_UseItemIndex = 0;
                }

                inputInterval++;
            }

            if(inputInterval >= 1 && manager.IsDisplayingCheckUseItemDialog == false) {
                inputInterval++;
                if(inputInterval >= ValueDefinition.INPUT_INTERVAL_OPERATE_ITEM_DISPLAYER) {
                    inputInterval = 0;
                }
            }

            //cursor.GetComponent<RectTransform>().localPosition = new Vector3(-272,260 - CursorPoint_UseItemIndex * 57,0);
            cursor.GetComponent<RectTransform>().localPosition = new Vector3(-272,260 - CursorPoint_UseItemIndex * 58.1f,0);


            if(inputManager.InputGetter("b1") && inputInterval <= 0) {
                manager.IsDisplayingCheckUseItemDialog = true;
                inputInterval++;
            }
        } else {
            descriptionText.text = "アイテムを所持していません";
            cursor.SetActive(false);
        }
    }
}
