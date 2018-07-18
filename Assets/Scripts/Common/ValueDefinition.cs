using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ValueDefinition {
    //定義値
    public static readonly int ENEMY_NUM_ = 10; //敵の数
    public static readonly int ITEM_NUM_ = 10; //アイテムの数
    public static readonly int FIELD_DIVIDE_NUM_X = 3;//フィールドの横の分割数
    public static readonly int FIELD_DIVIDE_NUM_Y = 3;//フィールドの縦の分割数
    public static readonly float FIELD_CHIP_SIZE = 0.16f; //フィールドチップの大きさ
    public static readonly int FIELD_SIZE_X = 60; //フィールドの横方向の大きさ
    public static readonly int FIELD_SIZE_Y = 50; //フィールドの縦方向の大きさ
    public static readonly int MIN_ROOM_SIZE_X = 3; //最低限の部屋のサイズ
    public static readonly int MIN_ROOM_SIZE_Y = 3; //最低限の部屋のサイズ
    public static readonly int ENEMY_SEARCH_PLAYER_RANGE = 10; //敵の探索範囲
    public static readonly float STEP_DISTANCE = 0.16f; //動く距離
    public static readonly int MAX_HOLD_ITEM_NUM = 10; //アイテムの持てる数

    //アイテム名
    public static readonly string[] ITEMS_NAME ={
        "薬草",
        "ケーキ",
        "石",
        "杖"
    };

    //アイテムの説明
    public static readonly string[] ITEMS_DESCRIPTION = {
        "塗り薬によく使われる草\nHPを50回復",
        "本場フランスのとっても甘いケーキ\n満腹度を20回復",
        "大きめの石\n投げることができる\n当たった敵に30のダメージ",
        "不思議な雰囲気を感じる杖\n敵に向けて使うとフィールドのどこかに飛ばすことができる"
    };

    //アイテム使用時のテキスト
    public static readonly string[] ITEMS_USED_TEXT = {
        "薬草を使用した\n体力を50回復",
        "ケーキを使用した\n満腹度を20回復",
        "石を投げた",
        "杖を使った"
    };

    //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■
    //ニューラルネットワーク関連
    public static readonly int INPUT_LAYER_NUM = 4;
    public static readonly int HIDDEN_LAYER_NUM = 10;
    public static readonly int OUTPUT_LAYER_NUM = 18;

    public static readonly float PLAYING_BIAS_NEXT_FLOOR = 1.4f;
    public static readonly float PLAYING_BIAS_GAMEOVER = 0.6f;
    public static readonly float PLAYING_BIAS_DAMAGE = 0.8f;
    public static readonly float PLAYING_BIAS_ITEM_RECOVER_HITPOINT = 1.05f;
    public static readonly float PLAYING_BIAS_ITEM_RECOVER_ONAKAPOINT = 1.05f;
    public static readonly float PLAYING_BIAS_ITEM_STONE_AIM_ENEMY = 1.05f;
    public static readonly float PLAYING_BIAS_ITEM_STAFF_AIM_ENEMY = 1.05f;
    public static readonly float PLAYING_BIAS_DEFEAT_ENEMY = 1.2f;

    public static readonly double LEARNING_RATE = 0.2d; //学習率

    //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■
    public static readonly Color MAPCOLOR_NORMAL = new Color(40f / 255,21f / 255,143f / 255,140f / 255); //何もないマップ
    public static readonly Color MAPCOLOR_PLAYER_POS = new Color(255f / 255,206f / 255,0f / 255,140f / 255); //プレイヤーがいる位置
    public static readonly Color MAPCOLOR_STAIR_POS = new Color(0f / 255,181f / 255,255f / 255,140f / 255); //階段の位置
    public static readonly Color MAPCOLOR_ENEMY_POS = new Color(255f / 255,0f / 255,0f / 255,140f / 255); //敵の位置
    public static readonly Color MAPCOLOR_ITEM_POS = new Color(42f / 255,210f / 255,0f / 255,140f / 255); //アイテムの位置

    public static readonly Color APPLYHPTEXTCOLOR_PLUS = new Color(255f / 255,64f / 255,64f / 255,255f / 255);
    public static readonly Color APPLYHPTEXTCOLOR_MINUS = new Color(139f / 255,233f / 255,50f / 255,255f / 255);
    public static readonly Color APPLYHPTEXTCOLOR_KUUHUKU = new Color(58f / 255,206f / 255,255f / 255,255f / 255);

    //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■
    //入力インターバル
    public static readonly int INPUT_INTERVAL_TITLE = 12;
    public static readonly int INPUT_INTERVAL_PLAYER_OPARATABLE = 12;
    public static readonly int INPUT_INTERVAL_OPEN_ITEM_DISPLAYER = 12;
    public static readonly int INPUT_INTERVAL_OPERATE_ITEM_DISPLAYER = 12;
    public static readonly int INPUT_INTERVSL_WAIT_ITEM_USE_DIALOG = 20;
    public static readonly int INPUT_INTERVSL_OPERATE_ITEM_USE_DIALOG = 12;
    public static readonly int INPUT_INTERVAL_CHECK_STAIR_DIALOG = 12;

    //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■
    public static readonly int SUPPORT_TILE_ACTIVE_TIME = 12;

    //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■
    public static readonly float[] EVENT_TABLE_ENEMY_NUM = { 0f,0.25f,0.5f,0.75f,1f };
    public static readonly float[] EVENT_TABLE_ITEM_NUM = { 0f,0.33f,0.66f,1f };
}
