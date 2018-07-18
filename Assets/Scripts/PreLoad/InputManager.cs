using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.UI;
//using SystemCollections.Generic;

public class InputManager : MonoBehaviour {
    //================================================================
    //public
    public bool IsUseJoyStick; //ジョイスティックを使うかどうか

    //================================================================
    //初期化
    private void Initialize () {
        //ジョイスティックを認識していない状態なら
        if(Input.GetJoystickNames().Length != 0) {
            //ジョイスティックの名前が空白なら
            if(Input.GetJoystickNames()[0] == "") {
                IsUseJoyStick = false;
            } else {
                IsUseJoyStick = true;
            }
        } else {
            IsUseJoyStick = false;
        }

        Debug.Log(IsUseJoyStick);
    }

    //=================================================================
    private void Awake () {
        DontDestroyOnLoad(this.gameObject);

        Initialize();
    }

    //=================================================================
    //入力を得る

    //上 下 左 右 (13) (24) (lr) (select)
    //up down left right b1 b2 lr select
    public bool InputGetter (string _str) {
        int num = 0;

        switch(_str) {
            case "up":
            if(IsUseJoyStick) {
                if(Input.GetAxis("Vertical") > 0) {
                    return true;
                }
            } else {
                if(Input.GetKey(KeyCode.UpArrow)) {
                    return true;
                }
            }

            break;

            case "down":
            if(IsUseJoyStick) {
                if(Input.GetAxis("Vertical") < 0) {
                    return true;
                }
            } else {
                if(Input.GetKey(KeyCode.DownArrow)) {
                    return true;
                }
            }

            break;

            case "left":
            if(IsUseJoyStick) {
                if(Input.GetAxis("Horizontal") < 0) {
                    return true;
                }
            } else {
                if(Input.GetKey(KeyCode.LeftArrow)) {
                    return true;
                }
            }

            break;

            case "right":
            if(IsUseJoyStick) {
                if(Input.GetAxis("Horizontal") > 0) {
                    return true;
                }
            } else {
                if(Input.GetKey(KeyCode.RightArrow)) {
                    return true;
                }
            }

            break;

            case "b1":
            if(IsUseJoyStick) {
                if(Input.GetButton("b1")) {
                    return true;
                }
            } else {
                if(Input.GetKey(KeyCode.Space)) {
                    return true;
                }
            }

            break;

            case "b2":
            if(IsUseJoyStick) {
                if(Input.GetButton("b2")) {
                    return true;
                }
            } else {
                if(Input.GetKey(KeyCode.B)) {
                    return true;
                }
            }

            break;

            case "b3":
            if(IsUseJoyStick) {
                if(Input.GetButton("b3")) {
                    return true;
                }
            } else {
                if(Input.GetKey(KeyCode.C)) {
                    return true;
                }
            }

            break;

            case "b4":
            if(IsUseJoyStick) {
                if(Input.GetButton("b4")) {
                    return true;
                }
            } else {
                if(Input.GetKey(KeyCode.D)) {
                    return true;
                }
            }

            break;

            case "lr":
            if(IsUseJoyStick) {
                if(Input.GetButton("lr")) {
                    return true;
                }
            } else {
                if(Input.GetKey(KeyCode.L)) {
                    return true;
                }
            }

            break;

            case "lr2":
            if(IsUseJoyStick) {
                if(Input.GetButton("lr2")) {
                    return true;
                }
            } else {
                if(Input.GetKey(KeyCode.R)) {
                    return true;
                }
            }

            break;

            case "select":
            if(IsUseJoyStick) {
                if(Input.GetButton("select")) {
                    return true;
                }
            } else {
                if(Input.GetKey(KeyCode.S)) {
                    return true;
                }
            }

            break;

            default:
            Debug.Log("入力エラー");
            break;
        }

        return false;
    }
}
