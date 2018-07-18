using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.UI;
//using SystemCollections.Generic;

public class StoneEffect : MonoBehaviour {
    //==============================================================
    //コンポーネント
    private ItemManager itemManager;

    //==============================================================
    //コンポーネント参照用
    private void ComponentRef () {
        itemManager = GameObject.Find("Manager/FieldManager/ItemManager").GetComponent<ItemManager>();
    }

    //メイン
    private void Awake () {
        ComponentRef();
    }

    //================================================================
    //投げる
    //fromX,fromY,toX,toY -> 飛び始めの位置と飛び終わりの位置
    //isDamaged -> ダメージを与えたかどうか
    public IEnumerator Throw (int fromX,int fromY,int toX,int toY,bool isDamaged) {
        float time = 0;
        Vector3 from = new Vector3(fromX * ValueDefinition.FIELD_CHIP_SIZE,fromY * ValueDefinition.FIELD_CHIP_SIZE,0);
        Vector3 to = new Vector3(toX * ValueDefinition.FIELD_CHIP_SIZE,toY * ValueDefinition.FIELD_CHIP_SIZE,0);

        while(true) {
            transform.position = Vector3.Lerp(from,to,time);

            time += Time.deltaTime * 6;
            if(time >= 1) {
                transform.position = to;
                break;
            }
            yield return new WaitForEndOfFrame();
        }

        //ダメージを与えていないなら着地地点にアイテムを増やす
        if(isDamaged == false) {
            itemManager.CreateItem(2,toX,toY);
        }

        Destroy(this.gameObject);

        yield break;
    }
}
