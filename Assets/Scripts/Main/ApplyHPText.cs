using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.UI;
//using SystemCollections.Generic;

public class ApplyHPText : MonoBehaviour {
    //================================================================
    //public
    public int ApplyingHP;

    //================================================================
    //コンポーネント
    private MeshRenderer meshRenderer;
    private TextMesh textMesh;

    //================================================================
    //コンポーネント参照用
    private void ComponentRef () {
        meshRenderer = GetComponent<MeshRenderer>();
        textMesh = GetComponent<TextMesh>();
    }

    //================================================================
    //メイン
    private void Awake () {
        ComponentRef();
        meshRenderer.sortingLayerName = "UI";
    }

    private void Start () {
        StartCoroutine(ToShinText());
        StartCoroutine(ToUpText());
    }

    //================================================================
    //色を変化させる
    public void ChangeColor (int _colortype) {
        switch(_colortype) {
            case 0:
            textMesh.color = ValueDefinition.APPLYHPTEXTCOLOR_PLUS;
            break;

            case 1:
            textMesh.color = ValueDefinition.APPLYHPTEXTCOLOR_MINUS;
            break;

            case 2:
            textMesh.color = ValueDefinition.APPLYHPTEXTCOLOR_KUUHUKU;
            break;

            default:
            break;
        }
    }

    //================================================================
    //数値に応じて表示タイプを変化させる
    public void ChangeTypeByHP () {
        if(ApplyingHP >= 0) {
            textMesh.text = "" + ApplyingHP;
        } else {
            textMesh.text = "" + -ApplyingHP;
        }
    }

    //================================================================
    //テキストを上に移動させる
    private IEnumerator ToUpText () {
        while(true) {
            transform.position += Vector3.up / 500;

            yield return new WaitForEndOfFrame();
        }
    }

    //================================================================
    //テキストを薄くする
    private IEnumerator ToShinText () {
        Color preColor = textMesh.color;
        float time = 0;

        while(true) {
            textMesh.color = new Color(preColor.r,preColor.g,preColor.b,2 - time);

            time += Time.deltaTime * 2;
            if(time >= 2) {
                break;
            }
            yield return new WaitForEndOfFrame();
        }

        Destroy(this.gameObject);
        yield break;
    }
}
