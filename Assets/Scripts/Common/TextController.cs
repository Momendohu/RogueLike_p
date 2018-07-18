using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;
using System;

/// <summary>
/// テキストを操作するスクリプト
/// </summary>
public class TextController : MonoBehaviour {
    public string TextPath;

    private string originalData;
    private string[] splitData;
    private string[,] data;
    private int divNum; //1列のデータ数
    private int dataNum; //列データ数

    private string path; //テキストファイルのパス

    private void Awake () {
        path = Application.dataPath + TextPath;
        ReadText();
    }

    //=========================================================================================
    //テキストファイルを読み込み配列に格納する
    public void ReadText () {
        Encoding encoding = Encoding.GetEncoding("UTF-8");

        try {
            using(StreamReader sr = new StreamReader(path,encoding)) {
                originalData = sr.ReadToEnd();
                splitData = originalData.Trim().Split('\n');

                divNum = splitData[0].Split(',').Length;
                data = new string[splitData.Length,divNum];
                dataNum = splitData.Length;

                for(int i = 0;i < dataNum;i++) {
                    for(int j = 0;j < divNum;j++) {
                        //Debug.Log("i:" + i + " j:" + j);
                        if(j < splitData[i].Split(',').Length) {
                            data[i,j] = splitData[i].Split(',')[j];
                        } else {
                            data[i,j] = "-1";
                        }
                    }
                }
            }
        } catch(Exception e) {
            Debug.Log("Create Text File:" + path);
            using(StreamWriter sw = new StreamWriter(path,true,encoding)) {
                sw.Flush();
            }
        }
    }

    //=========================================================================================
    //テキストファイルを読み込み配列に格納する(コルーチン)
    public IEnumerator C_ReadText () {
        Encoding encoding = Encoding.GetEncoding("UTF-8");

        try {
            using(StreamReader sr = new StreamReader(path,encoding)) {
                originalData = sr.ReadToEnd();
                splitData = originalData.Trim().Split('\n');

                divNum = splitData[0].Split(',').Length;
                data = new string[splitData.Length,divNum];
                dataNum = splitData.Length;

                for(int i = 0;i < dataNum;i++) {
                    for(int j = 0;j < divNum;j++) {
                        //Debug.Log("i:" + i + " j:" + j);
                        if(j < splitData[i].Split(',').Length) {
                            data[i,j] = splitData[i].Split(',')[j];
                        } else {
                            data[i,j] = "-1";
                        }
                    }
                }
            }
        } catch(Exception e) {
            Debug.Log("Create Text File:" + path);
            using(StreamWriter sw = new StreamWriter(path,true,encoding)) {
                sw.Flush();
            }
        }

        yield break;
    }

    //=========================================================================================
    //テキストファイルの最終行に書き足す
    public void WriteText (string str) {
        Encoding encoding = Encoding.GetEncoding("UTF-8");

        try {
            using(StreamWriter sw = new StreamWriter(path,true,encoding)) {
                sw.WriteLine(str);
                sw.Flush();
            }

            Debug.Log("Wrote");
        } catch(Exception e) {
            Debug.LogError(e);
        }
    }

    //=========================================================================================
    //配列に格納したデータをテキストファイルに書く
    public void ResetText () {
        Encoding encoding = Encoding.GetEncoding("UTF-8");

        try {
            using(StreamWriter sw = new StreamWriter(path,false,encoding)) {
                for(int i = 0;i < GetDataSize();i++) {
                    for(int j = 0;j < GetDataLength();j++) {
                        if(j == GetDataLength() - 1) {
                            sw.WriteLine(GetData(i,j));
                        } else {
                            sw.Write(GetData(i,j) + ",");
                        }
                    }
                }

                sw.Flush();
                Debug.Log("Reset");
            }
        } catch(Exception e) {
            Debug.LogError(e);
        }
    }

    //=========================================================================================
    //指定した行のテキストを消す
    public void RemoveText (int num) {
        for(int i = num;i < GetDataSize() - 1;i++) {
            for(int j = 0;j < GetDataLength();j++) {
                SetData(GetData(i + 1,j),i,j);
            }
        }

        for(int j = 0;j < GetDataLength();j++) {
            SetData("",GetDataSize() - 1,j);
        }

        Encoding encoding = Encoding.GetEncoding("UTF-8");

        try {
            using(StreamWriter sw = new StreamWriter(path,false,encoding)) {
                for(int i = 0;i < GetDataSize() - 1;i++) {
                    for(int j = 0;j < GetDataLength();j++) {
                        if(j == GetDataLength() - 1) {
                            sw.WriteLine(GetData(i,j));
                        } else {
                            sw.Write(GetData(i,j) + ",");
                        }
                    }
                }

                sw.Flush();
                Debug.Log("Remove and Reset");
            }
        } catch(Exception e) {
            Debug.LogError(e);
        }
    }

    //=========================================================================================
    //指定した部分のテキストを変更しテキストファイルに適用する
    public void UpdateText (string str,int i,int j) {
        SetData(str,i,j);
        ResetText();
    }

    //=========================================================================================
    //[GETTER]

    //列↓、行→
    public string GetData (int i,int j) {
        if(i >= 0 && j >= 0 && i <= data.GetLength(0) - 1 && j <= data.GetLength(1) - 1) {
            return data[i,j];
        } else {
            return "none";
        }
    }

    //データの列数を取得
    public int GetDataSize () {
        return dataNum;
    }

    //データ列の長さを取得
    public int GetDataLength () {
        return divNum;
    }

    //===========================================================================================
    //[SETTER]

    //列↓、行→
    public void SetData (string str,int i,int j) {
        if(i < GetDataSize() && j < GetDataLength()) {
            data[i,j] = str;
        }
    }
}
