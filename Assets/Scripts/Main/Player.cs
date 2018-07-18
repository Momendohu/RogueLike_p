using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.UI;

public class Player : MonoBehaviour {
    //==============================================================
    //コンポーネント
    private Manager manager;
    private FieldManager fieldManager;
    private ParameterManager parameterManager;
    private Animator animator;
    private FieldMap fieldMap;
    private TextBox textBox;
    private ItemManager itemManager;
    private InputManager inputManager;
    private Focus focus;
    private ActionSaver actionSaver;
    private NeuralNetworkManager neuralNetworkManager;
    private SoundManager soundManagerSE;

    //==============================================================
    //コンポーネント参照用
    private void ComponentRef () {
        manager = GameObject.Find("Manager").GetComponent<Manager>();
        fieldManager = GameObject.Find("Manager/FieldManager").GetComponent<FieldManager>();
        parameterManager = GameObject.Find("ParameterManager").GetComponent<ParameterManager>();
        animator = transform.Find("Sprite").GetComponent<Animator>();
        fieldMap = GameObject.Find("Canvas/FieldMap").GetComponent<FieldMap>();
        textBox = GameObject.Find("Canvas/TextBox").GetComponent<TextBox>();
        itemManager = GameObject.Find("Manager/FieldManager/ItemManager").GetComponent<ItemManager>();
        inputManager = GameObject.Find("InputManager").GetComponent<InputManager>();
        focus = GameObject.Find("Canvas/Focus").GetComponent<Focus>();
        actionSaver = GameObject.Find("ActionSaver").GetComponent<ActionSaver>();
        neuralNetworkManager = GameObject.Find("NeuralNetworkManager").GetComponent<NeuralNetworkManager>();
        soundManagerSE = GameObject.Find("SoundManagerSE").GetComponent<SoundManager>();
    }

    //==============================================================
    //public
    public int PosX; //位置
    public int PosY; //位置
    public bool IsAttack; //攻撃したか
    public int Direction; //方向(0(右)、反時計回り)
    public bool RemoveFlag; //消滅フラグ

    //==============================================================
    //private 
    private bool dismovable; //進めない状態を保存(trueならターンを加算しない)
    private bool onceCheckStair; //階段判定を一階だけ行うためのフラグ(階段から離れると戻る)
    private bool operatable; //操作可能か
    private int operateInterval; //操作可能判定を出すインターバル
    private bool onceRemove; //一回だけ消滅させるための判定
    private bool onceCheckPickUpItem; //一回だけアイテムを拾う処理をする
    private bool IsDiagonal; //ななめかどうか

    //==============================================================
    //初期化関数
    // _posX -> 位置
    // _posY -> 位置
    private void Initialize (int _posX,int _posY) {
        dismovable = false;
        onceCheckStair = false;

        PosX = _posX;
        PosY = _posY;

        IsAttack = false;
        Direction = 270;
    }

    //==============================================================
    //メイン
    private void Start () {
        ComponentRef();
        Initialize((int)manager.GetPlayerFirstPos().x,(int)manager.GetPlayerFirstPos().y);
        ApplyPositionStep(PosX,PosY);
        StartCoroutine(Move());
    }

    private void Update () {
        //消滅フラグがたっていないなら
        if(RemoveFlag == false) {
            //階段とプレイヤーの位置が一緒なら
            if(manager.GetStairFirstPos().x == PosX && manager.GetStairFirstPos().y == PosY) {
                if(onceCheckStair == false) {
                    manager.IsOperateOptionMenu = true;
                    onceCheckStair = true;
                }
            } else {
                onceCheckStair = false;
            }
        } else {
            manager.IsDeadPlayer = true;
        }

        //万が一壁の中にいるなら
        if(fieldManager.fieldData[PosX,PosY] == 1) {
            textBox.Apply("*いしのなかにいる*",false);
        }

        //フォーカスするかどうか
        if(fieldManager.IsRoom(PosX,PosY)) {
            focus.IsFocus = false;
        } else {
            focus.IsFocus = true;
        }

        if(parameterManager.playerParameters.CheckDead()) {
            RemoveFlag = true;
        }
    }

    //==============================================================
    //動く
    private IEnumerator Move () {
        while(true) {
            if(RemoveFlag == false) {
                //ターン進行待機状態なら
                if(manager.WasPlayerAct == false) {

                    //アニメーションの再生速度を通常に
                    animator.speed = 1;

                    //操作に応じて処理を行う
                    //[移動]方向、方向アニメーション、位置を変更する
                    //[攻撃]攻撃処理を行う
                    //[足踏み]足踏み処理を行う
                    //部屋でない場合、上下左右移動、攻撃のみにする
                    int operate = Operate();
                    switch(operate) {
                        //動作なし
                        case 0:
                        break;

                        //上
                        case 1:
                        IsDiagonal = false;
                        Direction = 90;
                        ApplyDirection("To_N");
                        ApplyPosition(0,1);
                        break;

                        //下
                        case -1:
                        IsDiagonal = false;
                        Direction = 270;
                        ApplyDirection("To_S");
                        ApplyPosition(0,-1);
                        break;

                        //左
                        case 10:
                        IsDiagonal = false;
                        Direction = 180;
                        ApplyDirection("To_W");
                        ApplyPosition(-1,0);
                        break;

                        //右
                        case -10:
                        IsDiagonal = false;
                        Direction = 0;
                        ApplyDirection("To_E");
                        ApplyPosition(1,0);
                        break;

                        //左上
                        case 11:
                        IsDiagonal = true;
                        Direction = 135;
                        ApplyDirection("To_NW");
                        ApplyPosition(-1,1);
                        break;

                        //右上
                        case -9:
                        IsDiagonal = true;
                        Direction = 45;
                        ApplyDirection("To_NE");
                        ApplyPosition(1,1);
                        break;

                        //左下
                        case 9:
                        IsDiagonal = true;
                        Direction = 225;
                        ApplyDirection("To_SW");
                        ApplyPosition(-1,-1);
                        break;

                        //右下
                        case -11:
                        IsDiagonal = true;
                        Direction = 315;
                        ApplyDirection("To_SE");
                        ApplyPosition(1,-1);
                        break;

                        //==============================================================
                        //攻撃
                        case 1000:
                        case 1001:
                        case 999:
                        case 1010:
                        case 990:
                        case 1011:
                        case 991:
                        case 1009:
                        case 989:
                        if(IsDiagonal == false) {
                            IsAttack = true;
                            manager.IsActiceActPlayer = true;
                            manager.ActQueue.Add(Attack(Direction));
                        } else {
                            if(fieldManager.IsRoom(PosX,PosY)) {
                                IsAttack = true;
                                manager.IsActiceActPlayer = true;
                                manager.ActQueue.Add(Attack(Direction));
                            } else {
                                dismovable = true;
                            }
                        }
                        break;

                        //==============================================================
                        //足踏み
                        case 10000:
                        //アニメーションの再生速度を3倍に
                        animator.speed = 3;
                        break;

                        //例外
                        default:
                        break;
                    }

                    //壁などならターン処理をなどを実行しない
                    if(dismovable) {
                        dismovable = false;
                        operate = 0;
                    }

                    //位置を適用
                    if(operate != 0) {
                        manager.WasPlayerAct = true;

                        //ApplyPositionStep(posX,posY);
                        yield return ApplyPositionAnim(PosX,PosY);
                    }
                }

                PickUpItem();
            } else {
                if(onceRemove == false) {
                    onceRemove = true;
                    yield return Remove();
                }
            }

            yield return new WaitForEndOfFrame();
        }
    }

    //=================================================================
    //アイテムを拾う
    private void PickUpItem () {
        int index = itemManager.IsExistItemOnThisPos(PosX,PosY);

        //アイテムを同じ位置にいるなら
        if(index != -1) {
            //アイテムの最大所持数以下なら
            if(parameterManager.playerParameters.HoldItems.Count < ValueDefinition.MAX_HOLD_ITEM_NUM) {
                //テキストボックス表示
                textBox.Apply(itemManager.Items[index].GetComponent<Item>().Name + "を拾った",false);

                //アイテムを保持
                parameterManager.playerParameters.HoldItems.Add(itemManager.Items[index].GetComponent<Item>().ID);

                //アイテムオブジェクトをフィールド上から消去
                itemManager.Remove(index);
            } else {
                if(onceCheckPickUpItem == false) {
                    //テキストボックス表示
                    textBox.Apply("アイテムはもう拾えません",false);

                    //テキストボックス表示を一回だけ出すようにフラグをいれる
                    onceCheckPickUpItem = true;
                }
            }
        } else {
            //フラグを元に戻す
            onceCheckPickUpItem = false;
        }
    }

    //=================================================================
    //消滅時の処理
    private IEnumerator Remove () {
        GameObject spriteObj = transform.Find("Sprite").gameObject;
        float time = 0;

        while(true) {
            time += Time.deltaTime * 10;
            if(time >= 10) {
                break;
            }

            if((int)time % 2 == 0) {
                spriteObj.SetActive(false);
            } else {
                spriteObj.SetActive(true);
            }

            yield return new WaitForEndOfFrame();
        }

        Destroy(spriteObj);
        yield break;
    }

    //==============================================================
    //攻撃
    //direction -> 方向
    private IEnumerator Attack (int direction) {
        Vector2 prePos = transform.position; //開始時の位置
        float time = 0;

        AttackToEnemy(direction);
        yield return new WaitForSeconds(0.1f);

        while(true) {
            time += Time.deltaTime * 180 * 5;
            transform.position =
                prePos +
                new Vector2(Mathf.Cos(direction * Mathf.Deg2Rad) * ValueDefinition.STEP_DISTANCE,Mathf.Sin(direction * Mathf.Deg2Rad) * ValueDefinition.STEP_DISTANCE) * Mathf.Sin(Mathf.Deg2Rad * time);

            if(time >= 180) {
                time = 0;
                transform.position = prePos;
                break;
            }
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(0.3f);
        textBox.TextProgress = true; //テキストを進行させる

        IsAttack = false; //攻撃を行った(ApplyPositionAnimのループ待機を終了させる)
        yield break;
    }

    //==============================================================
    //敵がプレイヤーに攻撃する
    //enemyIndex -> 敵の番号

    public void AttackToEnemy (int direction) {
        int checkIndex = IsEnemy(PosX + 1,PosY);

        switch(direction) {
            case 0:
            ApplyDamageToEnemy(IsEnemy(PosX + 1,PosY),textBox.ColorText(parameterManager.playerParameters.Name) + "の攻撃!",parameterManager.playerParameters.AttackPower);
            break;

            case 45:
            ApplyDamageToEnemy(IsEnemy(PosX + 1,PosY + 1),textBox.ColorText(parameterManager.playerParameters.Name) + "の攻撃!",parameterManager.playerParameters.AttackPower);
            break;

            case 90:
            ApplyDamageToEnemy(IsEnemy(PosX,PosY + 1),textBox.ColorText(parameterManager.playerParameters.Name) + "の攻撃!",parameterManager.playerParameters.AttackPower);
            break;

            case 135:
            ApplyDamageToEnemy(IsEnemy(PosX - 1,PosY + 1),textBox.ColorText(parameterManager.playerParameters.Name) + "の攻撃!",parameterManager.playerParameters.AttackPower);
            break;

            case 180:
            ApplyDamageToEnemy(IsEnemy(PosX - 1,PosY),textBox.ColorText(parameterManager.playerParameters.Name) + "の攻撃!",parameterManager.playerParameters.AttackPower);
            break;

            case 225:
            ApplyDamageToEnemy(IsEnemy(PosX - 1,PosY - 1),textBox.ColorText(parameterManager.playerParameters.Name) + "の攻撃!",parameterManager.playerParameters.AttackPower);
            break;

            case 270:
            ApplyDamageToEnemy(IsEnemy(PosX,PosY - 1),textBox.ColorText(parameterManager.playerParameters.Name) + "の攻撃!",parameterManager.playerParameters.AttackPower);
            break;

            case 315:
            ApplyDamageToEnemy(IsEnemy(PosX + 1,PosY - 1),textBox.ColorText(parameterManager.playerParameters.Name) + "の攻撃!",parameterManager.playerParameters.AttackPower);
            break;
        }
    }

    //=================================================================
    //敵にダメージを与える処理
    //_checkIndex -> 敵番号
    //_startText -> この処理が始まるときの最初のテキスト
    //_damage -> 与えるダメージ
    //_isItem -> アイテムかどうか
    private void ApplyDamageToEnemy (int _checkIndex,string _startText,int _damage) {
        string playerName = parameterManager.playerParameters.Name;
        string tmpApplyText = _startText; //テキストボックスに表示させるテキスト(使いまわすために文字送りコードを含める)

        if(_checkIndex != -1) {
            string enemyName = parameterManager.enemiesParameters[_checkIndex].Name;

            //敵の体力を減らす
            parameterManager.enemiesParameters[_checkIndex].Damage(_damage);
            tmpApplyText += "\n" + textBox.ColorText(enemyName) + "に" + textBox.ColorText("" + _damage) + "のダメージ";

            //ダメージ表示
            manager.CreateApplyingHPText(manager.Enemies[_checkIndex],_damage,0);

            //ダメージアニメーション
            StartCoroutine(manager.ShakeCheractor(manager.Enemies[_checkIndex].transform,0.4f));

            //敵の体力が0以下になったら
            if(parameterManager.enemiesParameters[_checkIndex].CheckDead()) {
                int exp = parameterManager.enemiesParameters[_checkIndex].ExperiencePoint;

                //テキストの追加
                tmpApplyText += "\n" + textBox.ColorText(enemyName) + "を倒した";

                // 経験値を追加
                parameterManager.playerParameters.AddExperiencePoint(exp);
                tmpApplyText += "\n" + "経験値" + textBox.ColorText("" + exp) + "を得た";

                //経験値がたまったらレベルアップ
                if(parameterManager.playerParameters.CheckLevelUpPlayer()) {
                    parameterManager.playerParameters.LevelUp();
                    tmpApplyText += "\n" + "レベルが" + textBox.ColorText("" + parameterManager.playerParameters.Level) + "にあがった!";

                    textBox.SetSE(0);
                    textBox.Apply(tmpApplyText,true);
                } else {
                    //テキストを変換してから適用
                    for(int i = 0;i < textBox.ChangeSendTextToText(tmpApplyText).Count;i++) {
                        textBox.Apply(textBox.ChangeSendTextToText(tmpApplyText)[i],false);
                    }
                }

                //敵除去処理
                manager.Enemies[_checkIndex].GetComponent<Enemy>().RemoveFlag = true;
                manager.Enemies.RemoveAt(_checkIndex);
                fieldManager.enemiesPos.RemoveAt(_checkIndex);
                parameterManager.enemiesParameters.RemoveAt(_checkIndex);

                //敵の名前(インデクス)を振りなおす
                for(int i = 0;i < manager.Enemies.Count;i++) {
                    if(int.Parse(manager.Enemies[i].name) > i) {
                        manager.Enemies[i].name = "" + i;
                        manager.Enemies[i].GetComponent<Enemy>().Index = i;
                    }
                }
            } else {
                textBox.Apply(tmpApplyText,false);
            }

            //サウンドエフェクト
            soundManagerSE.TriggerSE(1);
        } else {
            //サウンドエフェクト
            soundManagerSE.TriggerSE(2);
            textBox.Apply(tmpApplyText,false);
        }
    }

    //=================================================================
    //特定の座標に敵がいるかどうか
    //x,y -> 画像
    private int IsEnemy (int x,int y) {
        for(int i = 0;i < manager.GetEnemyNum();i++) {
            if(x == manager.GetEnemyPos(i).x && y == manager.GetEnemyPos(i).y) {
                return i;
            }
        }

        return -1;
    }

    //==============================================================
    //向きを適用する
    // direction -> 向く方向
    private void ApplyDirection (string direction) {
        AnimationInitialize();
        animator.SetBool(direction,true);
    }

    //==============================================================
    //位置情報を適用する
    // moveX -> 動く方向
    // moveY -> 動く方向
    private void ApplyPosition (int moveX,int moveY) {
        PosX += moveX;
        PosY += moveY;

        //壁なら位置情報を適用しない
        if(manager.GetFieldData(PosX,PosY) == 1) {
            dismovable = true;
            PosX -= moveX;
            PosY -= moveY;
        }

        //ななめ移動状態の時、「現在位置」と「進行予定位置」が部屋じゃない場所なら位置情報を適用しない
        if(dismovable == false) {
            if(IsDiagonal) {
                if(fieldManager.IsRoom(PosX,PosY) == false || fieldManager.IsRoom(PosX - moveX,PosY - moveY) == false) {
                    dismovable = true;
                    PosX -= moveX;
                    PosY -= moveY;
                }
            }
        }

        //方向選択状態なら位置情報を適用しない
        if(dismovable == false) {
            if(manager.IsSelectDirection) {
                dismovable = true;
                PosX -= moveX;
                PosY -= moveY;
            }
        }

        //進行先に敵がいたら位置情報を適用しない
        if(dismovable == false) {
            for(int i = 0;i < manager.GetEnemyNum();i++) {
                if(manager.GetEnemyPos(i).x == PosX && manager.GetEnemyPos(i).y == PosY) {
                    dismovable = true;
                    PosX -= moveX;
                    PosY -= moveY;
                }
            }
        }

        //動いたなら
        if(dismovable == false) {
            actionSaver.AddAction(0);
        }
    }

    //==============================================================
    //アニメーション遷移フラグの初期化
    private void AnimationInitialize () {
        animator.SetBool("To_S",false);
        animator.SetBool("To_SW",false);
        animator.SetBool("To_W",false);
        animator.SetBool("To_SE",false);
        animator.SetBool("To_E",false);
        animator.SetBool("To_NW",false);
        animator.SetBool("To_N",false);
        animator.SetBool("To_NE",false);
    }


    //==============================================================
    //位置を適用(飛び飛び)
    // _posX -> 位置
    // _posY -> 位置
    private void ApplyPositionStep (int _posX,int _posY) {
        PosX = _posX;
        PosY = _posY;
        transform.position = new Vector2(_posX * ValueDefinition.STEP_DISTANCE,_posY * ValueDefinition.STEP_DISTANCE);
    }

    //==============================================================
    //位置を適用(アニメーション処理)
    // _posX -> 位置
    // _posY -> 位置
    private IEnumerator ApplyPositionAnim (int _posX,int _posY) {
        Vector2 prePos = transform.position; //開始時の位置
        Vector2 goalPos = new Vector2(_posX * ValueDefinition.STEP_DISTANCE,_posY * ValueDefinition.STEP_DISTANCE); //到着地点
        float time = 0; //時間
        while(true) {
            transform.position = Vector2.Lerp(prePos,goalPos,time); //線形につなぐ
            time += Time.deltaTime * 6;
            if(time > 1) {
                time = 1;
                transform.position = goalPos;
                break;
            }

            yield return new WaitForEndOfFrame();
        }

        while(IsAttack) {
            yield return new WaitForEndOfFrame();
        }

        manager.EndedPlayerAct = true;
        manager.Turn++; //ターンを追加

        //ターン進行による空腹値や体力の変化
        switch(parameterManager.playerParameters.CheckHungryAndRecoverForTurn(3)) {
            case 1:
            textBox.Apply("お腹が空いてきた・・・",false);
            break;

            case 2:
            textBox.Apply("お腹がすごく空いてきた・・・",false);
            break;

            case 3:
            if(parameterManager.playerParameters.CheckDead()) {
                textBox.Apply("お腹が空いて目が回ってきた・・・" + "\n" +
                    "空腹で" + textBox.ColorText("" + 2) + "のダメージ" +
                     "\n" + parameterManager.playerParameters.Name + "は力尽きた・・・",true);
            } else {
                textBox.Apply("お腹が空いて目が回ってきた・・・" + "\n" +
                    "空腹で" + textBox.ColorText("" + 2) + "のダメージ",false);
            }
            break;

            default:
            break;

        }

        yield break;
    }

    //==============================================================
    //動く方向を決める
    private int Operate () {
        int num = 0;

        //オプション表示なし、ステージタイトル表示なし、アイテムメニュー表示なしの時操作可能フラグを立たせる
        if(manager.IsOperateOptionMenu == false && manager.IsDisplayingStageTitle == false && manager.IsDisplayingItemDisplayer == false) {
            operateInterval++;
            if(operateInterval >= ValueDefinition.INPUT_INTERVAL_PLAYER_OPARATABLE) {
                operatable = true;
            }
        } else {
            operateInterval = 0;
            operatable = false;
        }

        //操作可能なら
        if(operatable) {
            //足踏み処理
            if(inputManager.InputGetter("lr2")) {
                actionSaver.AddAction(3);
                return 10000;
            }

            if(inputManager.InputGetter("b1")) {
                actionSaver.AddAction(2);
                num += 1000;
            }

            if(inputManager.InputGetter("up")) {
                num += 1;
            }

            if(inputManager.InputGetter("down")) {
                num -= 1;
            }

            if(inputManager.InputGetter("left")) {
                num += 10;
            }

            if(inputManager.InputGetter("right")) {
                num -= 10;
            }
        }

        return num;
    }

    //==============================================================
    //前方に敵がいるかどうか調べる
    //_distance -> 距離

    public int IsExistEnemyToForward (int _distance) {
        switch(Direction) {
            case 0:
            for(int i = 1;i < _distance;i++) {
                if(IsEnemy(PosX + i,PosY) != -1) {
                    return IsEnemy(PosX + i,PosY);
                }

                if(IsAnotherObject(PosX + i,PosY)) {
                    return -1;
                }
            }
            return -1;

            case 45:
            for(int i = 1;i < _distance / 2;i++) {
                if(fieldManager.IsRoom(PosX + i,PosY + i) == false) {
                    return -1;
                }

                if(IsEnemy(PosX + i,PosY + i) != -1) {
                    return IsEnemy(PosX + i,PosY + i);
                }

                if(IsAnotherObject(PosX + i,PosY + i)) {
                    return -1;
                }
            }
            return -1;

            case 90:
            for(int i = 1;i < _distance;i++) {
                if(IsEnemy(PosX,PosY + i) != -1) {
                    return IsEnemy(PosX,PosY + i);
                }

                if(IsAnotherObject(PosX,PosY + i)) {
                    return -1;
                }
            }
            return -1;

            case 135:
            for(int i = 1;i < _distance / 2;i++) {
                if(fieldManager.IsRoom(PosX - i,PosY + i) == false) {
                    return -1;
                }

                if(IsEnemy(PosX - i,PosY + i) != -1) {
                    return IsEnemy(PosX - i,PosY + i);
                }

                if(IsAnotherObject(PosX - i,PosY + i)) {
                    return -1;
                }
            }
            return -1;

            case 180:
            for(int i = 1;i < _distance;i++) {
                if(IsEnemy(PosX - i,PosY) != -1) {
                    return IsEnemy(PosX - i,PosY);
                }

                if(IsAnotherObject(PosX - i,PosY)) {
                    return -1;
                }
            }
            return -1;

            case 225:
            for(int i = 1;i < _distance / 2;i++) {
                if(fieldManager.IsRoom(PosX - i,PosY - i) == false) {
                    return -1;
                }

                if(IsEnemy(PosX - i,PosY - i) != -1) {
                    return IsEnemy(PosX - i,PosY - i);
                }

                if(IsAnotherObject(PosX - i,PosY - i)) {
                    return -1;
                }
            }
            return -1;

            case 270:
            for(int i = 1;i < _distance;i++) {
                if(IsEnemy(PosX,PosY - i) != -1) {
                    return IsEnemy(PosX,PosY - i);
                }

                if(IsAnotherObject(PosX,PosY - i)) {
                    return -1;
                }
            }
            return -1;

            case 315:
            for(int i = 1;i < _distance / 2;i++) {
                if(fieldManager.IsRoom(PosX + i,PosY - i) == false) {
                    return -1;
                }

                if(IsEnemy(PosX + i,PosY - i) != -1) {
                    return IsEnemy(PosX + i,PosY - i);
                }

                if(IsAnotherObject(PosX + i,PosY - i)) {
                    return -1;
                }
            }
            return -1;

            default:
            return -1;
        }
    }

    //=================================================================
    //特定の座標に他のオブジェクトがあるかどうか
    //x -> x座標
    //y -> y座標

    private bool IsAnotherObject (int x,int y) {
        bool isAnotherObject = false;

        //壁かどうかを検索
        if(fieldManager.fieldData[x,y] == 1) {
            isAnotherObject = true;
        }

        //階段かどうかを検索
        if(fieldManager.stairPos.x == x && fieldManager.stairPos.y == y) {
            isAnotherObject = true;
        }

        //他のアイテムオブジェクトの座標を検索
        for(int i = 0;i < itemManager.Items.Count;i++) {
            if(itemManager.Items[i].GetComponent<Item>().Pos.x == x && itemManager.Items[i].GetComponent<Item>().Pos.y == y) {
                isAnotherObject = true;
                break;
            }
        }

        if(isAnotherObject) {
            return true;
        } else {
            return false;
        }
    }

    //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■
    //エフェクト生成
    //_id -> エフェクトの種類
    public void CreateEffect (int _id) {
        GameObject obj = null;
        int distance = 5 + 1;

        switch(_id) {
            case 0:
            //回復表示(薬草)
            manager.CreateApplyingHPText(this.gameObject,50,1);
            obj = Instantiate(Resources.Load("Prefabs/ItemEffect/RecoverEffect1")) as GameObject;
            obj.transform.SetParent(this.transform,false);

            //サウンドエフェクト
            soundManagerSE.TriggerSE(4);

            //プレイングバイアス
            neuralNetworkManager.ApplyPlayingBias(ValueDefinition.PLAYING_BIAS_ITEM_RECOVER_HITPOINT);
            break;

            //回復表示(食べ物)
            case 1:
            manager.CreateApplyingHPText(this.gameObject,20,2);
            obj = Instantiate(Resources.Load("Prefabs/ItemEffect/RecoverEffect2")) as GameObject;
            obj.transform.SetParent(this.transform,false);

            //サウンドエフェクト
            soundManagerSE.TriggerSE(4);

            //プレイングバイアス
            neuralNetworkManager.ApplyPlayingBias(ValueDefinition.PLAYING_BIAS_ITEM_RECOVER_ONAKAPOINT);
            break;

            //石を投げる
            case 2:
            obj = Instantiate(Resources.Load("Prefabs/ItemEffect/Stone")) as GameObject;

            //サウンドエフェクト
            soundManagerSE.TriggerSE(3);

            //敵がプレイヤーの正面にいるなら
            if(IsExistEnemyToForward(distance) != -1) {
                Enemy tmp = manager.Enemies[IsExistEnemyToForward(distance)].GetComponent<Enemy>();
                StartCoroutine(obj.GetComponent<StoneEffect>().Throw(PosX,PosY,tmp.PosX,tmp.PosY,true));
                ApplyDamageToEnemy(int.Parse(tmp.name),ValueDefinition.ITEMS_USED_TEXT[_id],30);

                //プレイングバイアス
                neuralNetworkManager.ApplyPlayingBias(ValueDefinition.PLAYING_BIAS_ITEM_STONE_AIM_ENEMY);
            } else {
                textBox.Apply(ValueDefinition.ITEMS_USED_TEXT[_id],false);

                int toX = 0;
                int toY = 0;
                switch(Direction) {
                    case 0:
                    for(int i = 1;i < distance;i++) {
                        if(IsAnotherObject(PosX + i,PosY)) {
                            toX = i - 1;
                            break;
                        }

                        if(i == distance - 1) {
                            toX = i;
                        }
                    }
                    break;

                    case 45:
                    for(int i = 1;i < distance / 2;i++) {
                        if(IsAnotherObject(PosX + i,PosY + i)) {
                            toX = i - 1;
                            toY = i - 1;
                            break;
                        }

                        if(fieldManager.IsRoom(PosX + i,PosY + i) == false) {
                            if(i == 0) {
                                toX = 0;
                                toY = 0;
                            } else {
                                toX = i - 1;
                                toY = i - 1;
                            }
                            break;
                        }

                        if(i == Mathf.CeilToInt(distance / 2) - 1) {
                            toX = i;
                            toY = i;
                        }
                    }
                    break;

                    case 90:
                    for(int i = 1;i < distance;i++) {
                        if(IsAnotherObject(PosX,PosY + i)) {
                            toY = i - 1;
                            break;
                        }

                        if(i == distance - 1) {
                            toY = i;
                        }
                    }
                    break;

                    case 135:
                    for(int i = 1;i < distance / 2;i++) {
                        if(IsAnotherObject(PosX - i,PosY + i)) {
                            toX = -(i - 1);
                            toY = i - 1;
                            break;
                        }

                        if(fieldManager.IsRoom(PosX - i,PosY + i) == false) {
                            if(i == 0) {
                                toX = 0;
                                toY = 0;
                            } else {
                                toX = -(i - 1);
                                toY = i - 1;
                            }
                            break;
                        }

                        if(i == Mathf.CeilToInt(distance / 2) - 1) {
                            toX = -i;
                            toY = i;
                        }
                    }
                    break;

                    case 180:
                    for(int i = 1;i < distance;i++) {
                        if(IsAnotherObject(PosX - i,PosY)) {
                            toX = -(i - 1);
                            break;
                        }

                        if(i == distance - 1) {
                            toX = -i;
                        }
                    }
                    break;

                    case 225:
                    for(int i = 1;i < distance / 2;i++) {
                        if(IsAnotherObject(PosX - i,PosY - i)) {
                            toX = -(i - 1);
                            toY = -(i - 1);
                            break;
                        }

                        if(fieldManager.IsRoom(PosX - i,PosY - i) == false) {
                            if(i == 0) {
                                toX = 0;
                                toY = 0;
                            } else {
                                toX = -(i - 1);
                                toY = -(i - 1);
                            }
                            break;
                        }

                        if(i == Mathf.CeilToInt(distance / 2) - 1) {
                            toX = -i;
                            toY = -i;
                        }
                    }
                    break;

                    case 270:
                    for(int i = 1;i < distance;i++) {
                        if(IsAnotherObject(PosX,PosY - i)) {
                            toY = -(i - 1);
                            break;
                        }

                        if(i == distance - 1) {
                            toY = -i;
                        }
                    }
                    break;

                    case 315:
                    for(int i = 1;i < distance / 2;i++) {
                        if(IsAnotherObject(PosX + i,PosY - i)) {
                            toX = i - 1;
                            toY = -(i - 1);
                            break;
                        }

                        if(fieldManager.IsRoom(PosX + i,PosY - i) == false) {
                            if(i == 0) {
                                toX = 0;
                                toY = 0;
                            } else {
                                toX = i - 1;
                                toY = -(i - 1);
                            }
                            break;
                        }

                        if(i == Mathf.CeilToInt(distance / 2) - 1) {
                            toX = i;
                            toY = -i;
                        }
                    }
                    break;

                    default:
                    break;
                }

                StartCoroutine(obj.GetComponent<StoneEffect>().Throw(PosX,PosY,PosX + toX,PosY + toY,false));
            }
            break;

            //杖
            case 3:
            obj = Instantiate(Resources.Load("Prefabs/ItemEffect/Staff")) as GameObject;
            obj.transform.position = this.transform.position;

            //サウンドエフェクト
            soundManagerSE.TriggerSE(5);

            //敵がプレイヤーの正面にいるなら敵の位置を変更する
            if(IsExistEnemyToForward(distance) != -1) {
                //敵の位置を変更する
                Enemy tmp = manager.Enemies[IsExistEnemyToForward(distance)].GetComponent<Enemy>();
                StartCoroutine(fieldManager.ChangeEnemiesPos(tmp));

                //敵の位置にエフェクトを発生させる
                GameObject obj2 = Instantiate(Resources.Load("Prefabs/ItemEffect/JumpAnotherPos1")) as GameObject;
                obj2.transform.position = tmp.transform.position;

                //プレイングバイアス
                neuralNetworkManager.ApplyPlayingBias(ValueDefinition.PLAYING_BIAS_ITEM_STAFF_AIM_ENEMY);
            }

            break;

            default:
            Debug.Log("エフェクト生成エラー");
            break;
        }
    }
}
