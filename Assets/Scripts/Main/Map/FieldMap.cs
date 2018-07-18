using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.UI;
//using SystemCollections.Generic;

public class FieldMap : MonoBehaviour {
    //================================================================
    //定義値
    private readonly int CHIP_SIZE = 12;

    //================================================================
    //private
    private bool[,] IsCreateMapChip;
    private List<GameObject> mapChips = new List<GameObject>(); //マップチップ
    private List<GameObject> mapChipsNotActive = new List<GameObject>(); //マップチップ(アクティブを切ったやつ)
    List<Vector2Int> checkedGrid = new List<Vector2Int>(); //マップ生成したマス
    private int mapApplyingRoutineNum = 0; //マップチップ生成処理の数


    //================================================================
    //コンポーネント
    private FieldManager fieldManager;
    private Manager manager;
    private ItemManager itemManager;

    //================================================================
    //参照用
    private void ComponentRef () {
        fieldManager = GameObject.Find("Manager/FieldManager").GetComponent<FieldManager>();
        manager = GameObject.Find("Manager").GetComponent<Manager>();
        itemManager = GameObject.Find("Manager/FieldManager/ItemManager").GetComponent<ItemManager>();
    }

    //================================================================
    //メイン
    private void Start () {
        ComponentRef();
        IsCreateMapChip = new bool[ValueDefinition.FIELD_SIZE_X,ValueDefinition.FIELD_SIZE_Y];
    }

    private void Update () {
        MapApply();
    }

    //==================================================================
    public void MapApply () {
        for(int i = 0;i < mapChips.Count;i++) {
            //階段の位置を適用
            if(manager.GetStairFirstPos().x == mapChips[i].GetComponent<MapChip>().PosX && manager.GetStairFirstPos().y == mapChips[i].GetComponent<MapChip>().PosY) {
                mapChips[i].GetComponent<MapChip>().IsStair = true;
            } else {
                mapChips[i].GetComponent<MapChip>().IsStair = false;
            }

            //敵の位置を適用
            for(int j = manager.GetEnemyNum() - 1;j >= 0;j--) {
                if(manager.GetEnemyPos(j).x == mapChips[i].GetComponent<MapChip>().PosX && manager.GetEnemyPos(j).y == mapChips[i].GetComponent<MapChip>().PosY) {
                    mapChips[i].GetComponent<MapChip>().IsEnemy = true;
                    break;
                } else {
                    mapChips[i].GetComponent<MapChip>().IsEnemy = false;
                }
            }

            if(manager.GetEnemyNum() == 0) {
                mapChips[i].GetComponent<MapChip>().IsEnemy = false;
            }

            //プレイヤーの位置を適用
            if(manager.GetPlayerPos().x == mapChips[i].GetComponent<MapChip>().PosX && manager.GetPlayerPos().y == mapChips[i].GetComponent<MapChip>().PosY) {
                mapChips[i].GetComponent<MapChip>().IsPlayer = true;
            } else {
                mapChips[i].GetComponent<MapChip>().IsPlayer = false;
            }

            //アイテムの位置を適用
            for(int j = itemManager.Items.Count - 1;j >= 0;j--) {
                if(itemManager.Items[j].GetComponent<Item>().Pos.x == mapChips[i].GetComponent<MapChip>().PosX && itemManager.Items[j].GetComponent<Item>().Pos.y == mapChips[i].GetComponent<MapChip>().PosY) {
                    mapChips[i].GetComponent<MapChip>().IsItem = true;
                    break;
                } else {
                    mapChips[i].GetComponent<MapChip>().IsItem = false;
                }
            }

            if(itemManager.Items.Count == 0) {
                mapChips[i].GetComponent<MapChip>().IsItem = false;
            }
        }

        StartCoroutine(CreateMapChip(manager.GetPlayerPos().x,manager.GetPlayerPos().y,true));

        //同じ部屋、道を表示する
        checkedGrid.Add(new Vector2Int(manager.GetPlayerPos().x,manager.GetPlayerPos().y));
        StartCoroutine(MapApplyAroundSameRoom(manager.GetPlayerPos().x,manager.GetPlayerPos().y,checkedGrid,0));
    }

    //==================================================================
    //部屋に入ったら部屋を表示する  
    //regressNum -> 回帰数
    private IEnumerator MapApplyAroundSameRoom (int _x,int _y,List<Vector2Int> checkedGrid,int regressNum) {
        bool isRegress = false;//回帰したか

        if(!(_x == -1 && _y == -1)) {
            //部屋なら
            if(fieldManager.fieldDataConcernDivide[_x,_y].x >= 1 && fieldManager.fieldDataConcernDivide[_x,_y].x <= ValueDefinition.FIELD_DIVIDE_NUM_X && fieldManager.fieldDataConcernDivide[_x,_y].y >= 1 && fieldManager.fieldDataConcernDivide[_x,_y].y <= ValueDefinition.FIELD_DIVIDE_NUM_Y) {
                //8近傍
                for(int i = -1;i < 2;i++) {
                    for(int j = -1;j < 2;j++) {
                        //中心マスでないなら
                        if(!(i == 0 && j == 0)) {
                            //探索位置がフィールド内なら
                            if((_x + i) >= 0 && (_y + j) >= 0 && (_x + i) <= (ValueDefinition.FIELD_SIZE_X - 1) && (_y + j) <= (ValueDefinition.FIELD_SIZE_Y - 1)) {
                                //探索したマスじゃないなら
                                if(!IsCheckedGrid(_x + i,_y + j,checkedGrid)) {
                                    //特定のマスの上下左右が、特定のマスと同じ部屋のマスかを調べる
                                    if(fieldManager.fieldDataConcernDivide[_x + i,_y + j] == fieldManager.fieldDataConcernDivide[_x,_y]) {
                                        mapApplyingRoutineNum++;
                                        checkedGrid.Add(new Vector2Int(_x + i,_y + j));
                                        StartCoroutine(CreateMapChip(_x + i,_y + j,false));
                                        StartCoroutine(MapApplyAroundSameRoom(_x + i,_y + j,checkedGrid,regressNum + 1));
                                        isRegress = true;
                                    }
                                }
                            } else {
                                checkedGrid.Add(new Vector2Int(_x + i,_y + j));
                            }
                        }
                    }

                    yield return new WaitForEndOfFrame();
                }
            }
        }

        if(regressNum == 0) {
            //回帰数0の関数で、回帰処理が行われたなら、
            if(isRegress) {
                //マップチップ生成処理を待つ
                while(true) {
                    if(mapApplyingRoutineNum == 0) {
                        break;
                    }

                    yield return new WaitForEndOfFrame();
                }

                //部屋のマップ生成が完了したのでアクティブにする
                for(int i = mapChipsNotActive.Count - 1;i >= 0;i--) {
                    mapChipsNotActive[i].SetActive(true);
                    mapChipsNotActive.RemoveAt(i);
                }
            }
        } else {
            mapApplyingRoutineNum--;
        }

        yield break;
    }

    //==================================================================
    private bool IsCheckedGrid (int _x,int _y,List<Vector2Int> checkedGrid) {
        for(int k = 0;k < checkedGrid.Count;k++) {
            if(checkedGrid[k].x == _x && checkedGrid[k].y == _y) {
                return true;
            }
        }

        return false;
    }

    //==================================================================
    //isActive -> アクティブにするかどうか
    private IEnumerator CreateMapChip (int x,int y,bool isActive) {
        //フィールド生成時の処理をパスする(プレイヤーの位置が(-1,-1)のため)
        if(!((x == 0 && y == 0) || (x == -1 && y == -1))) {
            //一度もマップチップが生成されていないなら
            if(IsCreateMapChip[x,y] == false) {
                //Debug.Log("mapchip"+x+":"+y+":"+ (x - ValueDefinition.FIELD_SIZE_X / 2) * CHIP_SIZE+"::"+(y - ValueDefinition.FIELD_SIZE_Y / 2) * CHIP_SIZE + 80);
                GameObject obj = Instantiate(Resources.Load("Prefabs/MapChip")) as GameObject;
                obj.transform.SetParent(this.transform,false);
                obj.GetComponent<RectTransform>().localPosition = new Vector3((x - ValueDefinition.FIELD_SIZE_X / 2) * CHIP_SIZE,(y - ValueDefinition.FIELD_SIZE_Y / 2) * CHIP_SIZE + 80,0);
                obj.name = x + ":" + y;
                obj.GetComponent<MapChip>().PosX = x;
                obj.GetComponent<MapChip>().PosY = y;
                mapChips.Add(obj);
                IsCreateMapChip[x,y] = true;

                if(isActive == false) {
                    obj.SetActive(false);
                    mapChipsNotActive.Add(obj);
                }
            }
        }

        yield break;
    }
}
