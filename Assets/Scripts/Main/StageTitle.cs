using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using SystemCollections.Generic;

public class StageTitle : MonoBehaviour {
    //================================================================
    private ParameterManager parameterManager;
    private Manager manager;

    private Image background;
    private Text title;
    private Text floorShadow;
    private Text floorMain;

    private void ComponentRef () {
        parameterManager = GameObject.Find("ParameterManager").GetComponent<ParameterManager>();
        manager = GameObject.Find("Manager").GetComponent<Manager>();
        background = transform.Find("Background").GetComponent<Image>();
        title = transform.Find("Title").GetComponent<Text>();
        floorShadow = transform.Find("Floor/Shadow").GetComponent<Text>();
        floorMain = transform.Find("Floor/Main").GetComponent<Text>();
    }

    //================================================================
    private bool onceThin; //一回だけ薄くする

    //================================================================
    //初期化
    private void Initialize () {
        floorMain.text = parameterManager.Floor + "F";
        floorShadow.text = parameterManager.Floor + "F";
    }


    //================================================================
    //メイン
    private void Start () {
        ComponentRef();
        Initialize();
    }

    private void Update () {
        //フィールドデータが生成されたなら
        if(manager.IsFieldCreated && onceThin == false) {
            StartCoroutine(ThinImage(background));
            StartCoroutine(ThinText(title));
            StartCoroutine(ThinText(floorShadow));
            StartCoroutine(ThinText(floorMain));

            onceThin = true;
        }
    }

    //================================================================
    private IEnumerator ThinText (Text _text) {
        float timeLength = 4;
        Color preColor = _text.color;
        float time = 0;

        while(true) {
            _text.color = new Color(preColor.r,preColor.g,preColor.b,timeLength - time);

            time += Time.deltaTime * 2;
            if(time >= timeLength) {
                _text.color = new Color(preColor.r,preColor.g,preColor.b,0);
                break;
            }

            yield return new WaitForEndOfFrame();
        }

        Destroy(_text.gameObject);
        yield break;
    }

    //================================================================
    private IEnumerator ThinImage (Image _image) {
        float timeLength = 4;
        Color preColor = _image.color;
        float time = 0;

        while(true) {
            _image.color = new Color(preColor.r,preColor.g,preColor.b,timeLength - time);

            time += Time.deltaTime * 2;
            if(time >= timeLength) {
                _image.color = new Color(preColor.r,preColor.g,preColor.b,0);
                break;
            }

            yield return new WaitForEndOfFrame();
        }

        Destroy(_image.gameObject);
        manager.IsDisplayingStageTitle = false; //操作を可能に
        yield break;
    }
}
