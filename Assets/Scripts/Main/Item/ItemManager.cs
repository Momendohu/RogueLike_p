using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.UI;

public class ItemManager : MonoBehaviour {
    //================================================================
    //シリアライズ(private)
    [SerializeField]
    private Sprite[] sprites;

    //================================================================
    //アイテムリスト
    public List<GameObject> Items = new List<GameObject>(); //アイテムリスト

    //================================================================
    //アイテムが特定の座標に存在するかどうか
    public int IsExistItemOnThisPos (int _x,int _y) {
        for(int i = 0;i < Items.Count;i++) {
            if(Items[i].GetComponent<Item>().Pos.x == _x && Items[i].GetComponent<Item>().Pos.y == _y) {
                return i;
            }
        }

        return -1;
    }

    //================================================================
    //消去する
    //num -> 番号
    public void Remove (int num) {
        if(num != -1) {
            GameObject tmp = Items[num];
            Items.RemoveAt(num);
            Destroy(tmp);
        }
    }

    //================================================================
    //アイテムを生成する
    //id -> アイテムID
    //x,y -> 座標

    public void CreateItem (int _id,int _x,int _y) {
        //オブジェクトの生成
        GameObject obj = Instantiate(Resources.Load("Prefabs/ItemPref")) as GameObject;

        //idに応じてアイテムの情報を更新
        if(_id >= 0 && _id <= 3) {
            obj.name = ValueDefinition.ITEMS_NAME[_id];
            obj.GetComponent<SpriteRenderer>().sprite = sprites[_id];
        } else {
            obj.name = "なぞのアイテム";
            obj.GetComponent<SpriteRenderer>().sprite = sprites[0];
        }

        //アイテムの情報を更新
        obj.GetComponent<Item>().ID = _id;
        obj.GetComponent<Item>().Name = obj.name;
        obj.GetComponent<Item>().Pos = new Vector2Int(_x,_y);

        //アイテムリストへの追加
        Items.Add(obj);

        //ItemManagerの子オブジェクトに登録
        obj.transform.SetParent(GameObject.Find("Manager/FieldManager/ItemManager").transform,false);

        //Itemの位置更新
        obj.transform.position = new Vector3(_x * ValueDefinition.FIELD_CHIP_SIZE,_y * ValueDefinition.FIELD_CHIP_SIZE,0);
    }
}
