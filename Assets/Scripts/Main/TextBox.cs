using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextBox : MonoBehaviour {
    //===============================================================
    //参照
    private GameObject frame;
    private Text text;
    private GameObject cursor;
    private GameObject frameDesign1;
    private Manager manager;
    private InputManager inputManager; //インプットマネージャー
    private SoundManager soundManagerSE;

    private void ComponentRef () {
        frame = transform.Find("Frame").gameObject;
        text = transform.Find("Frame/Text").GetComponent<Text>();
        cursor = transform.Find("TextBoxCursor").gameObject;
        frameDesign1 = transform.Find("Frame/FrameDesign1").gameObject;
        manager = GameObject.Find("Manager").GetComponent<Manager>();
        inputManager = GameObject.Find("InputManager").GetComponent<InputManager>();
        soundManagerSE = GameObject.Find("SoundManagerSE").GetComponent<SoundManager>();
    }

    //===============================================================
    //private
    //private int Mode; //0なら通常、1なら文字送り
    private List<string> waitText = new List<string>(); //表示するテキスト
    private List<string> waitSendText = new List<string>(); //表示する文字送りテキスト
    private Vector3 prePos; //処理直前の位置
    private int seFlag;

    //================================================================
    //public 
    public bool TextProgress; //テキストを進行させるかどうか
    public bool SendTextProgress; //文字送りテキストを進行させるかどうか

    //================================================================
    //メイン
    private void Awake () {
        seFlag = -1;
        ComponentRef();
        cursor.SetActive(false);
        frame.SetActive(false);

        TextProgress = false;

        prePos = text.GetComponent<RectTransform>().localPosition; //処理直前の位置
    }

    private void Start () {
        StartCoroutine(MainLoop());
    }

    private void Update () {
    }

    //=================================================================
    //テキストを適用する
    public void Apply (string _text,bool IsSend) {
        if(IsSend) {
            waitSendText.Add(_text);
        } else {
            waitText.Add(_text);
        }
    }

    //=================================================================
    //特定のインデクスのテキストを表示する
    private void Display (int index,bool IsSend) {
        if(IsSend) {
            text.text = waitSendText[index];
        } else {
            text.text = waitText[index];
        }
    }

    //=================================================================
    //特定のインデクスのテキストを削除する
    private void RemoveAt (int index,bool IsSend) {
        if(IsSend) {
            waitSendText.RemoveAt(index);
        } else {
            waitText.RemoveAt(index);
        }
    }

    //=================================================================
    //テキストに色をつける
    public string ColorText (string _text) {
        return "<color=#ff4040>" + _text + "</color>";
    }

    //=================================================================
    public void SetSE (int _num) {
        seFlag = _num;
    }

    //=================================================================
    //送るテキストの行数に応じて送る回数を指定する
    private int SendNum (string _text) {
        float num = 0;

        for(int i = 0;i < _text.Length;i++) {
            if(_text[i] == '\n') {
                num++;
            }
        }

        if(num >= 1) {
            return (int)(num / 2);
        } else {
            return 1;
        }
    }

    //=================================================================
    //送るテキストを通常のテキストに変換する
    public List<string> ChangeSendTextToText (string _text) {
        List<string> tmp = new List<string>();
        string tmpPart = "";
        int sendNum = 0;

        for(int i = 0;i < _text.Length;i++) {
            if(_text[i] == '\n') {
                sendNum++;
                if(sendNum >= 2) {
                    tmp.Add(tmpPart);
                    tmpPart = "";
                    sendNum = 0;
                } else {
                    tmpPart += _text[i];
                }
            } else {
                tmpPart += _text[i];
            }


        }
        tmp.Add(tmpPart); //最後に溜めたテキストを適用する

        return tmp;
    }

    //=================================================================
    private IEnumerator MainLoop () {
        int activeSwitchTime = 0;

        while(true) {
            bool isTextStore = waitText.Count >= 1; //テキストがたまっているなら
            bool isSendTextStore = waitSendText.Count >= 1; //文字送りテキストがたまっているなら

            bool f00 = !isTextStore && !isSendTextStore;
            bool f01 = !isTextStore && isSendTextStore;
            bool f10 = isTextStore && !isSendTextStore;
            bool f11 = isTextStore && isSendTextStore;

            if(f10) {
                activeSwitchTime = 0;
                frame.SetActive(true);
                cursor.SetActive(false);
                frameDesign1.SetActive(false);
                Display(0,false);

                while(true) {
                    activeSwitchTime++;
                    text.GetComponent<RectTransform>().localPosition = prePos; //文字送り時の位置変更を元に戻す
                    if(TextProgress || activeSwitchTime >= 40) {
                        //Debug.Log("textbox:" + TextProgress + ":" + activeSwitchTime);
                        TextProgress = false;
                        RemoveAt(0,false);
                        break;
                    }

                    yield return new WaitForEndOfFrame();
                }
            }

            if(f01 || f11) {
                //待ちテキストの処理に変わった地点で通常のテキストを消す
                for(int i = waitText.Count - 1;i >= 0;i--) {
                    RemoveAt(i,false);
                }

                activeSwitchTime = 0;
                frame.SetActive(true);
                cursor.SetActive(true);
                frameDesign1.SetActive(true);
                Display(0,true);

                manager.ActQueue.Add(SendToUpText());

                text.GetComponent<RectTransform>().localPosition = prePos; //文字送り時の位置変更を元に戻す

                while(true) {
                    if(SendTextProgress) {
                        yield return new WaitForSeconds(0.1f);
                        SendTextProgress = false;
                        RemoveAt(0,true);
                        break;
                    }

                    yield return new WaitForEndOfFrame();
                }
            }

            if(f00) {
                cursor.SetActive(false);
                frameDesign1.SetActive(false);
                activeSwitchTime++;
                if(activeSwitchTime >= 50) {
                    TextProgress = false;
                    text.GetComponent<RectTransform>().localPosition = prePos; //文字送り時の位置変更を元に戻す
                    text.text = "";
                    frame.SetActive(false);
                }
            }

            yield return new WaitForEndOfFrame();
        }

        yield break;
    }

    //====================================================================
    //文字を上に送る
    private IEnumerator SendToUpText () {
        float time = 0; //管理時間
        bool sendFlag = false; //文字送り待機用

        int sendNum = SendNum(waitSendText[0]);
        int nowSendNum = 0;

        while(true) {
            //キー処理の重なりを回避するための処理(バグ起きそう注意)
            if(time >= 10) {
                if(inputManager.InputGetter("b1") && sendFlag == false) {
                    sendFlag = true;
                    soundManagerSE.SetVolume(0.3f,6);
                    soundManagerSE.TriggerSE(6);
                    nowSendNum++;
                }

                //文字送りフラグが立ったら
                if(sendFlag) {
                    text.GetComponent<RectTransform>().localPosition += Vector3.up * 6.35f;
                    time++;

                    if(nowSendNum >= sendNum && seFlag != -1) {
                        soundManagerSE.TriggerSE(seFlag);
                        seFlag = -1;
                    }

                    //送るテキストがたまっていないなら
                    if(time > 10 + 20) {
                        if(nowSendNum >= sendNum) {

                            break;
                        } else {
                            time = 0;
                            sendFlag = false;
                        }
                    }
                }
            } else {
                time++;
            }

            yield return new WaitForEndOfFrame();
        }

        SendTextProgress = true;
        yield break;
    }
}
