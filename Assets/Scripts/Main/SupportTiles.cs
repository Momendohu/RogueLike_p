using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.UI;
//using SystemCollections.Generic;

public class SupportTiles : MonoBehaviour {
    //================================================================
    //private
    private int notActiveTime; //キー入力アクティブが切れてる時間
    private bool onceCreateSupportFile; //一回だけサポートを作成する
    private int playerDirection; //プレイヤーの方向

    //================================================================
    //初期化
    private void Initialize () {
        notActiveTime = 0;
        onceCreateSupportFile = false;
    }

    //================================================================
    //コンポーネント
    private List<GameObject> supportTiles = new List<GameObject>();
    private InputManager inputManager;
    private Manager manager;
    private FieldManager fieldManager;

    //================================================================
    //コンポーネント参照用
    private void ComponentRef () {
        inputManager = GameObject.Find("InputManager").GetComponent<InputManager>();
        manager = GameObject.Find("Manager").GetComponent<Manager>();
        fieldManager = GameObject.Find("Manager/FieldManager").GetComponent<FieldManager>();
    }

    //================================================================
    //メイン
    private void Start () {
        ComponentRef();
        Initialize();
    }

    private void Update () {
        //ボタンをおしたら + 行動キューが空なら + オプション表示なし + ステージタイトル表示なし + アイテムメニュー表示なし
        if(inputManager.InputGetter("lr") && manager.ActQueue.Count == 0 && manager.IsOperateOptionMenu == false && manager.IsDisplayingStageTitle == false && manager.IsDisplayingItemDisplayer == false) {
            //方向選択フラグを立てる
            manager.IsSelectDirection = true;

            //一度だけサポート表示を作成
            if(onceCreateSupportFile == false) {
                //プレイヤーの方向の保存
                playerDirection = manager.Player.GetComponent<Player>().Direction;

                RemoveSupportTiles();
                CreateSupportTiles(4);
                onceCreateSupportFile = true;
            }

            //保存された方向と現在の方向が違うならサポート表示を更新
            if(playerDirection != manager.Player.GetComponent<Player>().Direction) {
                onceCreateSupportFile = false;
            }

            //切り替え用時間を0に(いわゆるボタンを押してる状態)
            notActiveTime = 0;
        } else {

            //切り替え用時間加算
            notActiveTime++;

            //切り替え用時間が決めた時間に到達したなら
            if(notActiveTime >= ValueDefinition.SUPPORT_TILE_ACTIVE_TIME) {

                //サポート表示を消す
                RemoveSupportTiles();

                //方向選択フラグを切る
                manager.IsSelectDirection = false;

                //初期化
                Initialize();
            }
        }
    }

    //=================================================================
    //特定の座標に他のオブジェクトがあるかどうか
    //x -> x座標
    //y -> y座標

    private bool IsWall (int x,int y) {
        //壁かどうかを検索
        if(x >= 0 && x <= ValueDefinition.FIELD_SIZE_X - 1 && y >= 0 && y <= ValueDefinition.FIELD_SIZE_Y - 1) {
            if(fieldManager.fieldData[x,y] == 1) {
                return true;
            }
        }

        return false;
    }

    //================================================================
    //プレイヤーの方向に応じて移動できる方向を示す
    private Vector2Int MovablePosByPlayerDirection (int _direction,int _distance) {
        switch(_direction) {
            case 0:
            return new Vector2Int(1,0) * _distance;

            case 45:
            return new Vector2Int(1,1) * _distance;

            case 90:
            return new Vector2Int(0,1) * _distance;

            case 135:
            return new Vector2Int(-1,1) * _distance;

            case 180:
            return new Vector2Int(-1,0) * _distance;

            case 225:
            return new Vector2Int(-1,-1) * _distance;

            case 270:
            return new Vector2Int(0,-1) * _distance;

            case 315:
            return new Vector2Int(1,-1) * _distance;

            default:
            return new Vector2Int(0,0);
        }
    }

    //================================================================
    //サポートを作る
    //_range -> 範囲

    private void CreateSupportTiles (int _range) {
        for(int i = -_range;i < _range + 1;i++) {
            for(int j = -_range;j < _range + 1;j++) {
                bool flag = false;

                for(int k = 0;k < _range + 1;k++) {
                    Vector2Int tmp = MovablePosByPlayerDirection(playerDirection,k);

                    if(tmp.x == i && tmp.y == j) {
                        flag = true;
                        break;
                    } else {
                        flag = false;
                    }
                }

                //壁じゃないなら
                if(IsWall(manager.GetPlayerPos().x + i,manager.GetPlayerPos().y + j) == false) {
                    CreateSupportTile(manager.GetPlayerPos().x + i,manager.GetPlayerPos().y + j,flag);
                }
            }
        }
    }

    //================================================================
    //サポートの一つを作る
    //_x,_y -> 位置
    //_isMovable -> 進める方向か
    private void CreateSupportTile (int _x,int _y,bool _isMoveable) {
        GameObject obj = Instantiate(Resources.Load("Prefabs/SupportTile")) as GameObject;
        obj.transform.SetParent(this.transform,false);
        obj.transform.position = new Vector3(_x * ValueDefinition.FIELD_CHIP_SIZE,_y * ValueDefinition.FIELD_CHIP_SIZE,0);
        obj.GetComponent<SupportTile>().IsMovable = _isMoveable;
        supportTiles.Add(obj);
    }

    //================================================================
    //サポートを消す
    private void RemoveSupportTiles () {
        for(int i = supportTiles.Count - 1;i >= 0;i--) {
            Destroy(supportTiles[i]);
            supportTiles.RemoveAt(i);
        }
    }
}
