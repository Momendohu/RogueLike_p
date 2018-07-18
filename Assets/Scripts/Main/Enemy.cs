using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
//using UnityEngine.UI;

public class Enemy : MonoBehaviour {
    //==============================================================
    //コンポーネント参照
    private Manager manager;
    private FieldManager fieldManager;
    private Animator animator;
    private ParameterManager parameterManager;
    private TextBox textBox;
    private NeuralNetworkManager neuralNetworkManager;
    private SoundManager soundManagerSE;

    //==============================================================
    //参照用
    private void ComponentRef () {
        manager = GameObject.Find("Manager").GetComponent<Manager>();
        fieldManager = GameObject.Find("Manager").transform.Find("FieldManager").GetComponent<FieldManager>();
        animator = transform.Find("Sprite").GetComponent<Animator>();
        parameterManager = GameObject.Find("ParameterManager").GetComponent<ParameterManager>();
        textBox = GameObject.Find("Canvas/TextBox").GetComponent<TextBox>();
        neuralNetworkManager = GameObject.Find("NeuralNetworkManager").GetComponent<NeuralNetworkManager>();
        soundManagerSE = GameObject.Find("SoundManagerSE").GetComponent<SoundManager>();
    }

    //==============================================================
    //プロパティ
    public int PosX; //位置
    public int PosY; //位置
    public int Index; //番号

    public bool IsAttack; //攻撃したか
    public int Direction; //方向(0(右)、反時計回り)
    public bool RemoveFlag; //消滅フラグ

    //==============================================================
    //private
    private bool dismovable; //進めない状態を保存
    private bool wasActed; //行動したかどうか
    private int moveLoopNum; //ループ回数を保存(無限ループ回避&エラー処理用)
    private bool onceRemoveRoutine; //一回だけ消滅判定をつける
    private bool IsDiagonal; //ななめかどうか

    //==============================================================
    //初期化関数
    // _posX -> 位置
    // _posY -> 位置
    private void Initialize (int _posX,int _posY) {
        dismovable = false;
        wasActed = false;

        PosX = _posX;
        PosY = _posY;

        IsAttack = false;
        Direction = 270;
    }

    //================================================================
    //メイン
    private void Awake () {
        ComponentRef();
    }

    private void Start () {
        //NOTICE:敵のインデクス情報に応じて位置を決定するため、初期値決定のタイミングをAwakeからStart時に
        Initialize((int)manager.GetEnemiesFirstPos(Index).x,(int)manager.GetEnemiesFirstPos(Index).y);
        ApplyPositionStep(PosX,PosY);
        StartCoroutine(Move());
    }

    private void Update () {
        if(RemoveFlag && onceRemoveRoutine == false) {
            onceRemoveRoutine = true;
            StartCoroutine(Remove());
        }
    }

    //==============================================================
    //動く
    private IEnumerator Move () {
        while(true) {
            if(RemoveFlag == false) {
                //敵全員が行動したなら、次の行動の待機を始める
                if(manager.EnemiesActCount == 0) {
                    wasActed = false;
                }

                bool actTrigger = false;
                if(manager.IsActiceActPlayer) {
                    actTrigger = (wasActed == false && manager.EndedPlayerAct);
                } else {
                    actTrigger = (wasActed == false && manager.WasPlayerAct);
                }

                if(actTrigger) {
                    moveLoopNum = 0;
                    //行動が決定するまでループ
                    while(true) {
                        //int act = RandomWalk();
                        int act = ChasePlayer();
                        switch(act) {
                            //動作なし
                            case 0:
                            break;

                            //上
                            case 1:
                            Direction = 90;
                            ApplyPosition(0,1);
                            ApplyDirection("To_N");
                            break;

                            //下
                            case -1:
                            Direction = 270;
                            ApplyPosition(0,-1);
                            ApplyDirection("To_S");
                            break;

                            //左
                            case -10:
                            Direction = 180;
                            ApplyPosition(-1,0);
                            ApplyDirection("To_W");
                            break;

                            //右
                            case 10:
                            Direction = 0;
                            ApplyPosition(1,0);
                            ApplyDirection("To_E");
                            break;

                            //左上
                            case -9:
                            Direction = 135;
                            ApplyPosition(-1,1);
                            ApplyDirection("To_NW");
                            break;

                            //右上
                            case 11:
                            Direction = 45;
                            ApplyPosition(1,1);
                            ApplyDirection("To_NE");
                            break;

                            //左下
                            case -11:
                            Direction = 225;
                            ApplyPosition(-1,-1);
                            ApplyDirection("To_SW");
                            break;

                            //右下
                            case 9:
                            Direction = 315;
                            ApplyPosition(1,-1);
                            ApplyDirection("To_SE");
                            break;

                            //行動なし
                            case 777:
                            break;

                            //================================================================
                            //攻撃

                            //上
                            case 100:
                            Direction = 90;
                            IsAttack = true;
                            ApplyDirection("To_N");
                            manager.ActQueue.Add(Attack(Direction));
                            break;

                            //下
                            case -100:
                            Direction = 270;
                            IsAttack = true;
                            ApplyDirection("To_S");
                            manager.ActQueue.Add(Attack(Direction));
                            break;

                            //左
                            case -1000:
                            Direction = 180;
                            IsAttack = true;
                            ApplyDirection("To_W");
                            manager.ActQueue.Add(Attack(Direction));
                            break;

                            //右
                            case 1000:
                            Direction = 0;
                            IsAttack = true;
                            ApplyDirection("To_E");
                            manager.ActQueue.Add(Attack(Direction));
                            break;

                            //左上
                            case -900:
                            Direction = 135;
                            IsAttack = true;
                            ApplyDirection("To_NW");
                            manager.ActQueue.Add(Attack(Direction));
                            break;

                            //右上
                            case 1100:
                            Direction = 45;
                            IsAttack = true;
                            ApplyDirection("To_NE");
                            manager.ActQueue.Add(Attack(Direction));
                            break;

                            //左下
                            case -1100:
                            Direction = 225;
                            IsAttack = true;
                            ApplyDirection("To_SW");
                            manager.ActQueue.Add(Attack(Direction));
                            break;

                            //右下
                            case 900:
                            Direction = 315;
                            IsAttack = true;
                            ApplyDirection("To_SE");
                            manager.ActQueue.Add(Attack(Direction));
                            break;

                            //例外
                            default:
                            break;
                        }

                        //壁などならターン処理をなどを実行しない
                        if(dismovable) {
                            dismovable = false;
                            act = 0;
                        }

                        //位置を適用
                        if(act != 0) {
                            //ApplyPositionStep(posX,posY);
                            yield return ApplyPositionAnim(PosX,PosY);
                            break; //行動を決定したので、ループを抜ける
                        }

                        moveLoopNum++;
                        if(moveLoopNum >= 50) {
                            Error(4);
                            yield return new WaitForEndOfFrame();
                        }
                    }
                } else {
                    if(RemoveFlag) {
                        parameterManager.enemiesParameters.RemoveAt(int.Parse(this.name));
                        Destroy(this.gameObject);
                    }
                }
            }

            yield return new WaitForEndOfFrame();
        }
    }

    //==============================================================
    //位置を適用(飛び飛び)
    // _posX -> 位置
    // _posY -> 位置
    public void ApplyPositionStep (int _posX,int _posY) {
        PosX = _posX;
        PosY = _posY;
        transform.position = new Vector2(_posX * ValueDefinition.STEP_DISTANCE,_posY * ValueDefinition.STEP_DISTANCE);
    }

    //==============================================================
    //位置情報と向きを適用する
    // moveX -> 動く方向
    // moveY -> 動く方向
    // direction -> 向く方向
    private void ApplyPosition (int moveX,int moveY) {
        PosX += moveX;
        PosY += moveY;
        if(manager.GetFieldData(PosX,PosY) == 1) {
            dismovable = true;
            PosX -= moveX;
            PosY -= moveY;
        }
    }

    private void ApplyDirection (string direction) {
        AnimationInitialize();
        animator.SetBool(direction,true);
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
    //位置を適用(アニメーション処理)
    // _posX -> 位置
    // _posY -> 位置
    private IEnumerator ApplyPositionAnim (int _posX,int _posY) {
        wasActed = true;

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

        manager.EnemiesActCount++;

        yield break;
    }

    //==============================================================
    //攻撃
    //direction -> 方向
    private IEnumerator Attack (int direction) {
        Vector2 prePos = transform.position; //開始時の位置
        float time = 0;
        bool onceShakePlayer = false; //一回だけプレイヤーを揺らすため

        //プレイヤーがすでに倒されたなら処理をパス
        if(manager.IsDeadPlayer) {
            yield break;
        }

        AttackToPlayer();
        yield return new WaitForSeconds(0.1f);

        while(true) {
            time += Time.deltaTime * 180 * 5;
            transform.position =
                prePos +
                new Vector2(Mathf.Cos(direction * Mathf.Deg2Rad) * ValueDefinition.STEP_DISTANCE,Mathf.Sin(direction * Mathf.Deg2Rad) * ValueDefinition.STEP_DISTANCE) * Mathf.Sin(Mathf.Deg2Rad * time);

            //一回だけプレイヤーを揺らす
            if(time >= 120 && onceShakePlayer == false) {
                onceShakePlayer = true;
                StartCoroutine(manager.ShakeCheractor(manager.Player.transform,0.4f));
            }

            if(time >= 180) {
                time = 0;
                transform.position = prePos;
                break;
            }
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(1);
        textBox.TextProgress = true; //テキストを進行させる

        IsAttack = false; //攻撃を行った(ApplyPositionAnimのループ待機を終了させる)
        yield break;
    }

    //==============================================================
    //敵がプレイヤーに攻撃する

    public void AttackToPlayer () {
        int damage = parameterManager.enemiesParameters[Index].AttackPower; //ダメージ値
        string playerName = parameterManager.playerParameters.Name; //名前
        string enemyName = parameterManager.enemiesParameters[Index].Name;

        string tmpApplyText = textBox.ColorText(enemyName) + "の攻撃!"; //テキストボックスに表示させるテキスト

        //サウンドエフェクト
        soundManagerSE.TriggerSE(1);

        //ダメージの適用
        parameterManager.playerParameters.Damage(damage);
        tmpApplyText += "\n" + textBox.ColorText(playerName) + "に" + textBox.ColorText("" + damage) + "のダメージ!";
        manager.CreateApplyingHPText(manager.Player,damage,0);

        //プレイングバイアス
        neuralNetworkManager.ApplyPlayingBias(ValueDefinition.PLAYING_BIAS_DAMAGE);

        //プレイヤーの死亡判定
        if(parameterManager.playerParameters.CheckDead()) {
            tmpApplyText += "\n" + playerName + "は力尽きた・・・";
            textBox.Apply(tmpApplyText,true);
        } else {
            textBox.Apply(tmpApplyText,false);
        }
    }


    //==============================================================
    //ランダムウォーク(壁か敵なら立ち止まる)
    private int RandomWalk () {
        int randomNum;
        if(fieldManager.IsRoom(PosX,PosY)) {
            randomNum = Random.Range(1,10);
        } else {
            randomNum = Random.Range(1,6);
        }

        switch(randomNum) {
            case 1:
            if(IsAnotherObject(PosX,PosY + 1)) return 777;
            return 1;

            case 2:
            if(IsAnotherObject(PosX,PosY - 1)) return 777;
            return -1;

            case 3:
            if(IsAnotherObject(PosX + 1,PosY)) return 777;
            return 10;

            case 4:
            if(IsAnotherObject(PosX - 1,PosY)) return 777;
            return -10;

            case 5:
            return 777;

            case 6:
            if(IsAnotherObject(PosX + 1,PosY - 1)) return 777;
            if(fieldManager.IsRoom(PosX + 1,PosY - 1) == false) return 777;
            return 9;

            case 7:
            if(IsAnotherObject(PosX - 1,PosY + 1)) return 777;
            if(fieldManager.IsRoom(PosX - 1,PosY + 1) == false) return 777;
            return -9;

            case 8:
            if(IsAnotherObject(PosX + 1,PosY + 1)) return 777;
            if(fieldManager.IsRoom(PosX + 1,PosY + 1) == false) return 777;
            return 11;

            case 9:
            if(IsAnotherObject(PosX - 1,PosY - 1)) return 777;
            if(fieldManager.IsRoom(PosX - 1,PosY - 1) == false) return 777;
            return -11;

            default:
            Error(3);
            return -100;
        }
    }

    //==============================================================
    private int ChasePlayer () {
        bool isNearPlayer = IsNear(ValueDefinition.ENEMY_SEARCH_PLAYER_RANGE,PosX,PosY,manager.GetPlayerPos().x,manager.GetPlayerPos().y);
        bool isSamePlayerRoom = fieldManager.fieldDataConcernDivide[PosX,PosY] == fieldManager.fieldDataConcernDivide[manager.Player.GetComponent<Player>().PosX,manager.Player.GetComponent<Player>().PosY];
        bool isNeighborPlayer = Mathf.Abs(PosX - manager.GetPlayerPos().x) <= 1 && Mathf.Abs(PosY - manager.GetPlayerPos().y) <= 1;
        //条件に合うなら探索、違うならランダムウォーク
        if((isNearPlayer && isSamePlayerRoom) || isNeighborPlayer) {
            int directionCode = 0; //方向を定義(1は正の方向、2は負の方向)

            //範囲内を探索
            for(int i = -ValueDefinition.ENEMY_SEARCH_PLAYER_RANGE / 2;i < ValueDefinition.ENEMY_SEARCH_PLAYER_RANGE / 2 + 1;i++) {
                for(int j = -ValueDefinition.ENEMY_SEARCH_PLAYER_RANGE / 2;j < ValueDefinition.ENEMY_SEARCH_PLAYER_RANGE / 2 + 1;j++) {
                    DecideAction(i,j,ref directionCode);
                }
            }

            //Debug.Log("objectName->" + gameObject.name + ":directioncode->" + directionCode);

            switch(directionCode) {
                case 0:
                Error(2);
                return 0;

                //プレイヤーが敵の右
                case 1:
                return 10;

                //プレイヤーが敵の左
                case 2:
                return -10;

                //プレイヤーが敵の上
                case 10:
                return 1;

                //プレイヤーが敵の下
                case 20:
                return -1;

                //プレイヤーが敵の右上
                case 11:
                return 11;

                //プレイヤーが敵の左上
                case 12:
                return -9;

                //プレイヤーが敵の右下
                case 21:
                return 9;

                //プレイヤーが敵の左下
                case 22:
                return -11;

                //動作無し
                case 777:
                return 777;

                //===================================================================
                //攻撃(上下左右)

                //どの方向でもない
                case 1000:
                return 777;

                //プレイヤーが敵の右
                case 1001:
                return 1000;

                //プレイヤーが敵の左
                case 1002:
                return -1000;

                //プレイヤーが敵の上
                case 1010:
                return 100;

                //プレイヤーが敵の下
                case 1020:
                return -100;

                //プレイヤーが敵の右上
                case 1011:
                return 1100;

                //プレイヤーが敵の左上
                case 1012:
                return -900;

                //プレイヤーが敵の右下
                case 1021:
                return 900;

                //プレイヤーが敵の左下
                case 1022:
                return -1100;

                default:
                Error(1);
                return 0;
            }
        } else {
            return RandomWalk();
        }
    }

    //=================================================================
    //特定の座標にプレイヤーがいるかどうか

    private bool IsPlayer (int x,int y) {
        if(x == manager.GetPlayerPos().x && y == manager.GetPlayerPos().y) {
            return true;
        } else {
            return false;
        }
    }

    //=================================================================
    //行動を決定する
    private void DecideAction (int x,int y,ref int directionCode) {
        //敵の位置がわかったら
        if(IsPlayer(PosX + x,PosY + y)) {
            //プレイヤーが隣接するマスにいるならプレイヤーの方を向いて攻撃
            //探索範囲ならプレイヤーを追いかける
            if(Mathf.Abs(x) <= 1 && Mathf.Abs(y) <= 1) {
                //プレイヤーが道にいる
                bool playerInRoot = !fieldManager.IsRoom(manager.GetPlayerPos().x,manager.GetPlayerPos().y);
                //敵が道にいる
                bool enemyInRoot = !fieldManager.IsRoom(PosX,PosY);

                if(playerInRoot || enemyInRoot) {
                    //マンハッタン距離が1以下なら攻撃する
                    if(Mathf.Abs(x) + Mathf.Abs(y) <= 1) {
                        directionCode += 1000;
                    } else {
                        directionCode = 777;
                        return;
                    }
                } else {
                    directionCode += 1000;
                }
            }


            int dir = 0;
            int toR = 1;
            int toL = 2;
            int toU = 4;
            int toD = 8;

            int relativePosX = manager.GetPlayerPos().x - PosX;
            int relativePosY = manager.GetPlayerPos().y - PosY;

            if(relativePosX > 0) {
                directionCode += 1;
                dir += toR;
            } else if(relativePosX < 0) {
                directionCode += 2;
                dir += toL;
            }

            if(relativePosY > 0) {
                directionCode += 10;
                dir += toU;
            } else if(relativePosY < 0) {
                directionCode += 20;
                dir += toD;
            }

            switch(dir) {
                case 0: //とどまる
                break;

                case 1: //右
                if(IsAnotherObject(PosX + 1,PosY)) {
                    directionCode -= 1;
                }
                break;

                case 2: //左
                if(IsAnotherObject(PosX - 1,PosY)) {
                    directionCode -= 2;
                }
                break;

                case 4: //上
                if(IsAnotherObject(PosX,PosY + 1)) {
                    directionCode -= 10;
                }
                break;

                case 5: //右上
                if(IsAnotherObject(PosX + 1,PosY + 1)) {
                    directionCode -= 11;

                    if(IsAnotherObject(PosX + 1,PosY) == false) {
                        directionCode += 1;
                    } else if(IsAnotherObject(PosX,PosY + 1) == false) {
                        directionCode += 10;
                    }
                }
                break;

                case 6: //左上
                if(IsAnotherObject(PosX - 1,PosY + 1)) {
                    directionCode -= 12;

                    if(IsAnotherObject(PosX - 1,PosY) == false) {
                        directionCode += 2;
                    } else if(IsAnotherObject(PosX,PosY + 1) == false) {
                        directionCode += 10;
                    }
                }
                break;

                case 8: //下
                if(IsAnotherObject(PosX,PosY - 1)) {
                    directionCode -= 20;
                }
                break;

                case 9: //右下
                if(IsAnotherObject(PosX + 1,PosY - 1)) {
                    directionCode -= 21;

                    if(IsAnotherObject(PosX + 1,PosY) == false) {
                        directionCode += 1;
                    } else if(IsAnotherObject(PosX,PosY - 1) == false) {
                        directionCode += 20;
                    }
                }
                break;

                case 10: //左下
                if(IsAnotherObject(PosX - 1,PosY - 1)) {
                    directionCode -= 22;

                    if(IsAnotherObject(PosX - 1,PosY) == false) {
                        directionCode += 2;
                    } else if(IsAnotherObject(PosX,PosY - 1) == false) {
                        directionCode += 20;
                    }
                }
                break;

                default:
                break;
            }

            if(directionCode == 0) {
                directionCode = 777;
            }
        }
    }

    //=================================================================
    //範囲内に点と点があるか
    //range -> 範囲
    //x1,y1,x2,y2 -> 座標
    private bool IsNear (int range,int x1,int y1,int x2,int y2) {
        for(int i = -ValueDefinition.ENEMY_SEARCH_PLAYER_RANGE / 2;i < ValueDefinition.ENEMY_SEARCH_PLAYER_RANGE / 2 + 1;i++) {
            for(int j = -ValueDefinition.ENEMY_SEARCH_PLAYER_RANGE / 2;j < ValueDefinition.ENEMY_SEARCH_PLAYER_RANGE / 2 + 1;j++) {
                if((x1 + i) == x2 && (y1 + j) == y2) {
                    return true;
                }
            }
        }

        return false;
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

        //他の敵オブジェクトの座標を検索
        for(int i = 0;i < manager.GetEnemyNum();i++) {

            //自分以外
            if(Index != i) {
                if(manager.GetEnemyPos(i).x == x && manager.GetEnemyPos(i).y == y) {
                    isAnotherObject = true;
                    //Debug.Log(Index);
                    break;
                }
            }
        }

        if(isAnotherObject) {
            return true;
        } else {
            return false;
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

        //プレイングバイアス
        neuralNetworkManager.ApplyPlayingBias(ValueDefinition.PLAYING_BIAS_DEFEAT_ENEMY);

        parameterManager.DefeatEnemyNum++; //敵を倒した数を加算
        parameterManager.DefeatEnemyNumInThisFloor++; //敵を倒した数を加算
        manager.EnemiesActCount++; //行動カウント数を加算
        Destroy(this.gameObject); //このオブジェクトを消す
        yield break;
    }

    //=================================================================
    //errorCode -> エラー番号
    //1 -> 敵の方向設定ミス(ChasePlayer() 定義されていない方向)
    //2 -> 敵の方向設定ミス(ChasePlayer() プレイヤーと敵の位置が同じ)
    //3 -> 敵の方向設定ミス(RandomWalk () 定義されていない方向)
    //4 -> 敵の方向設定ミス(Move() 過剰ループ)

    private void Error (int errorCode) {
        Debug.LogError("エラーが発生しました" + " オブジェクト名:" + gameObject.name + " コード:" + errorCode);
        //EditorApplication.isPaused = true;
        //EditorApplication.isPlaying = false;
    }
}
