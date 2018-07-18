using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleSelect : MonoBehaviour {
    //================================================================
    //コンポーネント
    private Image image;
    private Text text;
    private ParameterManager parameterManager;

    //================================================================
    //コンポーネント参照用
    private void ComponentRef () {
        image = transform.Find("Image").GetComponent<Image>();
        text = transform.Find("Text").GetComponent<Text>();
        parameterManager = GameObject.Find("ParameterManager").GetComponent<ParameterManager>();
    }

    //================================================================
    //public
    public bool IsSelect; //選択されているか

    //================================================================
    //private
    private Color preCol_Image; //画像色の初期値
    private Color preCol_Text; //テキスト色の初期値

    //================================================================
    private void Initialize () {
        IsSelect = false;
        preCol_Image = image.color;
        preCol_Text = text.color;
    }

    //================================================================
    //メイン
    private void Start () {
        ComponentRef();
        Initialize();
    }

    private void Update () {
        //Debug.Log(gameObject.name + ":" + IsSelect);
        if(IsSelect) {
            image.color = preCol_Image;
            text.color = preCol_Text;
        } else {
            image.color = new Color(preCol_Image.r,preCol_Image.g,preCol_Image.b,0.3f);
            text.color = new Color(preCol_Text.r,preCol_Text.g,preCol_Text.b,0.3f);
        }
    }

    //================================================================
    public IEnumerator Tobig () {
        float time = 0;
        text.gameObject.SetActive(false);
        transform.SetAsLastSibling();

        while(true) {
            image.GetComponent<RectTransform>().localScale += Vector3.one * (time) * 0.01f;

            time++;
            if(time >= 90) {
                break;
            }

            yield return new WaitForEndOfFrame();
        }

        transitionToMainGame();
    }

    //================================================================
    //メインゲームに遷移する処理
    private void transitionToMainGame () {
        parameterManager.TransPattern = 1;
        SceneManager.LoadScene("Loading");
    }
}
