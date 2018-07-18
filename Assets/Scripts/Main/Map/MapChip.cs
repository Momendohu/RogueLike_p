using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using SystemCollections.Generic;

public class MapChip : MonoBehaviour {
    //================================================================
    public Sprite[] mapChipSprite;

    //================================================================
    private Image image;

    private void ComponentRef () {
        image = GetComponent<Image>();
    }
    //================================================================
    public int PosX;
    public int PosY;
    public bool IsEnemy; //敵の位置かどうか
    public bool IsPlayer; //プレイヤーの位置かどうか
    public bool IsStair; //階段の位置かどうか
    public bool IsItem; //アイテムがどうか

    //================================================================
    //メイン
    private void Awake () {
        ComponentRef();
    }

    private void Start () {
        StartCoroutine(ColorControl());
    }

    //==================================================================
    private IEnumerator ColorControl () {
        while(true) {
            while(true) {
                if(IsPlayer) {
                    image.color = ValueDefinition.MAPCOLOR_PLAYER_POS;
                    image.sprite = mapChipSprite[0];

                    break;
                }

                if(IsEnemy) {
                    image.color = ValueDefinition.MAPCOLOR_ENEMY_POS;
                    image.sprite = mapChipSprite[0];

                    break;
                }

                if(IsStair) {
                    image.color = ValueDefinition.MAPCOLOR_STAIR_POS;
                    image.sprite = mapChipSprite[1];

                    break;
                }

                if(IsItem) {
                    image.color = ValueDefinition.MAPCOLOR_ITEM_POS;
                    image.sprite = mapChipSprite[0];

                    break;
                }

                //ただの道
                image.color = ValueDefinition.MAPCOLOR_NORMAL;
                image.sprite = mapChipSprite[0];
                break;
            }

            yield return new WaitForEndOfFrame();
        }
    }
}
