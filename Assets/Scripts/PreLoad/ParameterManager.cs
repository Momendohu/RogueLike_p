using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParameterManager : MonoBehaviour {
    //================================================================
    //パラメータ(プレイヤー)
    public class PlayerParameters : AgentParameters {
        //コンストラクタ
        public PlayerParameters () {
            parameterInitialize();
        }

        public void parameterInitialize () {
            Level = 1;
            ExperiencePoint = 0;
            HitPoint = 100;
            MaxHitPoint = HitPoint;
            OnakaPoint = 100;
            AttackPower = 35;
            localTurn = 0;
            //Name = "テストデバッグ太郎";
            //Name = "プレイヤー01";
            // Name = "プレイヤー02";
            //Name = "プレイヤー03";
            //Name = "プレイヤー04";
            //Name = "プレイヤー05";
            //Name = "プレイヤー06";
            //Name = "プレイヤー07";
            //Name = "プレイヤー08";
            //Name = "プレイヤー09";
            //Name = "プレイヤー10";
            //Name = "プレイヤー11";
            //Name = "プレイヤー12";
            //Name = "プレイヤー13";
            //Name = "プレイヤー14";
            //Name = "プレイヤー15";
            //Name = "プレイヤー16";
            //Name = "プレイヤー17";
            //Name = "プレイヤー18";
            //Name = "プレイヤー19";
            //Name = "プレイヤー20";
            //Name = "プレイヤー21";
            //Name = "プレイヤー22";
            Name = "プレイヤー";

            Index = 0;
            UseItemNum = 0;

            HoldItems.Add(0);
            HoldItems.Add(1);
            HoldItems.Add(2);
            HoldItems.Add(3);
        }
    }

    //================================================================
    //パラメータ(敵)
    public class EnemyParameters : AgentParameters {
        //コンストラクタ
        public EnemyParameters (int index,int level) {
            parameterInitialize(index,level);
        }

        public void parameterInitialize (int index,int level) {
            Level = level;
            ExperiencePoint = 15 + 3 * (level - 1);
            HitPoint = 50 + 10 * (level - 1);
            MaxHitPoint = HitPoint;
            OnakaPoint = 1;
            AttackPower = 20 + 5 * (level - 1);
            localTurn = 1;

            if(Level >= 9) {
                Name = "アビスケサラン";
            } else if(Level >= 6) {
                Name = "ダークケサラン";
            } else if(Level >= 3) {
                Name = "ブラックケサラン";
            } else {
                Name = "ケサラン";
            }

            Index = index;
            UseItemNum = 0;
        }
    }

    //================================================================
    //public
    public string Mode;//ゲームモード
    public int TransPattern; //シーン遷移パターン
    public string StageName; //ステージの名前
    public int Floor; //階数
    public int DefeatEnemyNum; //倒した敵の数
    public int DefeatEnemyNumInThisFloor; //階層ごとの倒した敵の数
    public int NeuralNetworkID; //現在使用しているニューラルネットワークのID
    public int PlayingSkill; //プレイングスキル
    public PlayerParameters playerParameters;
    public List<EnemyParameters> enemiesParameters = new List<EnemyParameters>();

    //================================================================
    //初期化
    public void Initialize () {
        Mode = "A";
        TransPattern = 0;
        StageName = "はじまりの洞窟";
        Floor = 1;
        DefeatEnemyNum = 0;
        DefeatEnemyNumInThisFloor = 0;
        NeuralNetworkID = 0;
        InitializePlayer();
        InitializeEnemies();
    }

    //================================================================
    //プレイヤーのパラメータの初期化
    public void InitializePlayer () {
        playerParameters = new PlayerParameters();
    }

    //================================================================
    //敵のパラメータの初期化
    public void InitializeEnemies () {
        for(int i = enemiesParameters.Count - 1;i >= 0;i--) {
            enemiesParameters.RemoveAt(i);
        }
    }

    //================================================================
    //敵のパラメータを用意する
    //index -> 番号
    public void AddEnemyParameters (int index,int level) {
        enemiesParameters.Add(new EnemyParameters(index,level));
    }

    //================================================================
    //メイン
    private void Awake () {
        Initialize();
        DontDestroyOnLoad(this.gameObject);
    }

    //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■
    //エージェントのパラメータ
    public class AgentParameters {
        public int Level; //レベル
        public int ExperiencePoint; //経験値
        public int HitPoint; //体力値
        public int MaxHitPoint; //体力の最大値
        public int OnakaPoint; //満腹値
        public int AttackPower; //攻撃力
        public int localTurn; //ターン数
        public string Name; //名前
        public int Index; //番号
        public List<int> HoldItems = new List<int>(); //所持してるアイテム
        public int UseItemNum; //アイテム使用数

        //================================================================
        //アイテムを使う
        public void UseItem (int _index) {
            if(_index < HoldItems.Count) {
                int id = HoldItems[_index];

                switch(id) {
                    case 0:
                    Recover(50);
                    break;

                    case 1:
                    RecoverOnaka(20);
                    break;

                    default:
                    break;
                }

                UseItemNum++;
                HoldItems.RemoveAt(_index);
            }
        }

        //================================================================
        //レベルアップ
        public void LevelUp () {
            Level++;
            MaxHitPoint += 3;
            HitPoint = MaxHitPoint;
            OnakaPoint = 100;
            AttackPower += 3;
            localTurn = 0;
        }

        //================================================================
        //レベルアップできるかどうかチェック
        public bool CheckLevelUpPlayer () {
            if(ExperiencePoint >= 10 + 20 * (Level - 1)) {
                ExperiencePoint = 0;
                return true;
            }

            return false;
        }

        //================================================================
        //ターン進行による回復、おなかを減らす判定
        //num -> 判定を出すターン数
        public int CheckHungryAndRecoverForTurn (int num) {
            localTurn++;

            //死亡時に処理をパス
            if(CheckDead()) {
                return -1;
            }

            if(localTurn >= num) {
                localTurn = 0;

                return Hungry(1);
            }

            return 0;
        }

        //================================================================
        //おなかを減らす
        //num ->減らす数値
        public int Hungry (int num) {
            OnakaPoint -= num;
            if(OnakaPoint <= 0) {
                OnakaPoint = 0;
            }

            //空腹値が30以下なら
            if(OnakaPoint == 30) {
                return 1;
            }

            //空腹値が10以下なら
            if(OnakaPoint == 10) {
                return 2;
            }

            //空腹値が0なら
            if(OnakaPoint <= 0) {
                Damage(2);
                return 3;
            }

            Recover(1);

            return 0;
        }

        //================================================================
        //ダメージ
        //num -> ダメージ値
        public void Damage (int num) {
            HitPoint -= num;
            if(HitPoint <= 0) {
                HitPoint = 0;
            }
        }

        //================================================================
        //体力の回復
        //num -> 回復する数値
        public void Recover (int num) {
            HitPoint += num;
            if(HitPoint > MaxHitPoint) {
                HitPoint = MaxHitPoint;
            }
        }

        //================================================================
        //空腹値の回復
        //num -> 回復する数値
        public void RecoverOnaka (int num) {
            OnakaPoint += num;
            if(OnakaPoint > 100) {
                OnakaPoint = 100;
            }
        }

        //================================================================
        //死んでいるか
        public bool CheckDead () {
            return HitPoint <= 0;
        }

        //================================================================
        //経験値を追加する
        //num -> 追加する経験値
        public void AddExperiencePoint (int num) {
            ExperiencePoint += num;
        }
    }
}
