using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 管理用クラス
/// 他クラスの数値を参照する場合はここを通す
/// </summary>
public class Manager : MonoBehaviour {
    //==============================================================
    //ゲーム進行用パラメータ
    public int Turn; //ターン数
    public bool WasPlayerAct; //プレイヤーが行動したか
    public bool EndedPlayerAct; //プレイヤーが行動し終えたか(プレイヤー→敵の行動になるようにするため)
    public bool IsActiceActPlayer; //プレイヤーが攻撃したか(敵の行動制御のため)

    public bool IsFieldCreated; //フィールドの生成を完了したかどうか
    public int EnemiesActCount; //敵の行動終了数
    public bool IsOperateOptionMenu; //オプション操作しているかどうか
    public bool IsDisplayingStageTitle; //ステージタイトルを表示しているかどうか
    public bool IsDisplayingItemDisplayer; //アイテム使用選択画面を表示しているかどうか
    public bool IsDisplayingCheckUseItemDialog; //アイテム使用確認ダイアログを表示しているかどうか
    public bool IsSelectDirection; //方向選択中か

    public bool IsDeadPlayer; //プレイヤーが死亡したか

    public List<IEnumerator> ActQueue = new List<IEnumerator>(); //行動キュー

    public GameObject Player = null;
    public List<GameObject> Enemies = new List<GameObject>();

    public void ActFlagInitialize () {
        WasPlayerAct = false;
        EndedPlayerAct = false;
        IsActiceActPlayer = false;
        EnemiesActCount = 0;
    }

    private void ParameterInitialize () {
        Turn = 1;
        IsOperateOptionMenu = false;
        IsDisplayingStageTitle = true;
        IsDisplayingItemDisplayer = false;
        IsDeadPlayer = false;
        ActFlagInitialize();
    }

    //==============================================================
    //private 
    private bool isCreatePlayer; //プレイヤーを作成したかどうか
    private bool isCreateEnemies; //敵を作成したかどうか
    private bool onceRoutineDeadPlayer; //プレイヤー死亡時の一回限りの実行部分のためのフラグ
    private int onceRoutineDeadPlayerInterval; //プレイヤー死亡時の一回限りの処理を発生させるかどうかにインターバルを設けて実行の優先順位を下げる

    //==============================================================
    //コンポーネント
    private FieldManager fieldManager;
    private ParameterManager parameterManager;
    private NeuralNetworkManager neuralNetworkManager;
    private ActionSaver actionSaver; //アクションセーバー
    private SoundManager soundManagerBGM;

    //==============================================================
    //コンポーネント参照用
    private void ComponentRef () {
        fieldManager = transform.Find("FieldManager").GetComponent<FieldManager>();
        parameterManager = GameObject.Find("ParameterManager").GetComponent<ParameterManager>();
        neuralNetworkManager = GameObject.Find("NeuralNetworkManager").GetComponent<NeuralNetworkManager>();
        actionSaver = GameObject.Find("ActionSaver").GetComponent<ActionSaver>();
        soundManagerBGM = GameObject.Find("SoundManagerBGM").GetComponent<SoundManager>();
    }


    //==============================================================
    //メイン
    private void Awake () {
        ParameterInitialize();
        CheckPreloadParameterManager();
        CheckPreloadImputManager();
        ComponentRef();
    }

    private void Start () {
        soundManagerBGM.SetVolume(0.08f,0);
        soundManagerBGM.Trigger(0,true);
        StartCoroutine(ActControl());
    }

    private void Update () {
        if(ActQueue.Count == 0) {
            //敵全員の行動が終わったら
            if(EnemiesActCount >= GetEnemyNum()) {
                ActFlagInitialize(); //行動関連のフラグの初期化
            }

            //プレイヤーがしんだら
            if(IsDeadPlayer) {
                onceRoutineDeadPlayerInterval++;

                if(onceRoutineDeadPlayerInterval >= 60) {
                    if(onceRoutineDeadPlayer == false) {
                        onceRoutineDeadPlayer = true;

                        //プレイングバイアス
                        neuralNetworkManager.ApplyPlayingBias(ValueDefinition.PLAYING_BIAS_GAMEOVER);

                        soundManagerBGM.StopMusic(0);

                        actionSaver.ApplyActionToNeuralNetwork();
                        parameterManager.TransPattern = 3;
                        ActQueue.Add(FadeScene("Result"));
                    }
                }
            }
        } else {
            onceRoutineDeadPlayerInterval = 0;
        }

        //===============================================================
        //初期に実行される

        //フィールドデータが生成されたなら
        if(IsFieldCreated) {

            //(プレイヤーが存在しない場合)プレイヤーオブジェクトの作成
            if(isCreatePlayer == false) {
                Player = CreatePlayer();
                isCreatePlayer = true;
            }

            //(敵がが存在しない場合)敵オブジェクトの作成
            if(isCreateEnemies == false) {
                for(int i = 0;i < GetEnemyNum();i++) {
                    Enemies.Add(CreateEnemy(i));
                }
                isCreateEnemies = true;
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

    //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■
    //シーン遷移
    public void SceneJump (string sceneName) {
        switch(sceneName) {
            case "GameMain":
            parameterManager.Floor++;
            parameterManager.InitializeEnemies();
            break;

            case "Result":
            break;

            case "Title":
            parameterManager.InitializePlayer();
            break;

            default:
            break;
        }

        SceneManager.LoadScene("Loading");
    }

    //フェード(シーン遷移も行う)
    public IEnumerator FadeScene (string sceneName) {
        yield return new WaitForSeconds(0.2f);

        CreateFade(0.5f);

        yield return new WaitForSeconds(0.5f);

        SceneJump(sceneName);
        yield break;
    }

    //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■
    //戦闘システム
    //==============================================================
    //行動を管理する(主に敵の行動)

    private IEnumerator ActControl () {
        while(true) {
            if(ActQueue.Count > 0) {
                yield return StartCoroutine(ActQueue[0]);
                ActQueue.RemoveAt(0);
            }
            yield return new WaitForEndOfFrame();
        }

        yield break;
    }

    //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■
    //他オブジェクト参照

    //==============================================================
    //特定の位置ののフィールドデータを取得する
    // _posX -> 位置
    // _posY -> 位置
    public int GetFieldData (int _posX,int _posY) {
        return fieldManager.fieldData[_posX,_posY];
    }

    //==============================================================
    //プレイヤーの初期位置を取得する
    public Vector2 GetPlayerFirstPos () {
        return fieldManager.FirstPlayerPos;
    }

    //==============================================================
    //プレイヤーの位置を取得する
    public Vector2Int GetPlayerPos () {
        if(Player != null) {
            return new Vector2Int(Player.GetComponent<Player>().PosX,Player.GetComponent<Player>().PosY);
        } else {
            return new Vector2Int(-1,-1);
        }
    }

    //==============================================================
    //敵の初期位置を取得する
    //index -> 何番目の敵か
    public Vector2 GetEnemiesFirstPos (int index) {
        return fieldManager.enemiesPos[index];
    }

    //==============================================================
    //敵の位置を取得する
    //index -> 何番目の敵か
    public Vector2 GetEnemyPos (int index) {
        //むりやりtry-catch
        try {
            if(index <= (GetEnemyNum() - 1)) {
                return new Vector2(Enemies[index].GetComponent<Enemy>().PosX,Enemies[index].GetComponent<Enemy>().PosY);
            } else {
                return new Vector2(-1,-1);
            }
        } catch {
            return new Vector2(-1,-1);
        }
    }

    //==============================================================
    //敵の数を取得する
    public int GetEnemyNum () {
        return fieldManager.EnemyNum();
    }

    //==============================================================
    //階段の初期位置を取得する
    public Vector2 GetStairFirstPos () {
        return fieldManager.stairPos;
    }

    //==============================================================
    //位置情報に応じてどの分轄フィールドにいるのかを返す
    // _posX -> 位置
    // _posY -> 位置
    public Vector2 GetFieldDataConcernDivide (int _posX,int _posY) {
        if(!(_posX == -1 || _posY == -1)) {
            return fieldManager.fieldDataConcernDivide[_posX,_posY];
        } else {
            return new Vector2(0,0);
        }
    }

    //==============================================================
    //空腹値を取得する
    public int GetOnakaPoint () {
        return parameterManager.playerParameters.OnakaPoint;
    }

    //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■
    //オブジェクト生成

    //==============================================================
    //プレイヤーを作成する
    private GameObject CreatePlayer () {
        //カメラの重複をチェックする
        GameObject dummycam = GameObject.Find("DummyCam");
        if(GameObject.Find("DummyCam") != null) {
            Destroy(dummycam);
        }

        GameObject obj = Instantiate(Resources.Load("Prefabs/Player")) as GameObject;
        obj.name = "Player";

        return obj;
    }

    //==============================================================
    //敵を作成する
    private GameObject CreateEnemy (int index) {
        GameObject obj = Instantiate(Resources.Load("Prefabs/Enemy")) as GameObject;
        obj.name = "" + index;
        obj.GetComponent<Enemy>().Index = index;
        obj.transform.SetParent(GameObject.Find("Enemies").transform,false);

        parameterManager.AddEnemyParameters(index,parameterManager.Floor);
        return obj;
    }

    //==============================================================
    //HPに対する操作が行われたときに出すテキスト
    //applyer -> 出すところを指定
    //num -> 出す数字
    //colorType -> 色のタイプ
    public void CreateApplyingHPText (GameObject applyer,int num,int colorType) {
        GameObject obj = Instantiate(Resources.Load("Prefabs/ApplyingHPText")) as GameObject;
        obj.transform.position = applyer.transform.position;

        obj.GetComponent<ApplyHPText>().ApplyingHP = num;
        obj.GetComponent<ApplyHPText>().ChangeColor(colorType);
        obj.GetComponent<ApplyHPText>().ChangeTypeByHP();
    }

    //==============================================================
    //フェードを生成する
    //timeLength -> フェードの時間
    private void CreateFade (float timeLength) {
        GameObject obj = Instantiate(Resources.Load("Prefabs/Fade")) as GameObject;
        obj.transform.SetParent(GameObject.Find("Canvas").transform,false);
        obj.GetComponent<Fade>().TimeLength = timeLength;
    }

    //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■
    //アニメーション

    //==============================================================
    public IEnumerator ShakeCheractor (Transform _transform,float timeLength) {
        Vector3 prePos = _transform.position;
        float time = 0;
        float shakeRange = 0.1f;

        while(true) {
            _transform.position = prePos + new Vector3(0,Mathf.PingPong(time,shakeRange) - shakeRange / 2,0);

            time += Time.deltaTime * 3;
            if(time >= timeLength) {
                break;
            }
            yield return new WaitForEndOfFrame();
        }

        _transform.position = prePos;
        yield break;
    }
}
