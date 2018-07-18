using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeuralNetworkManager : MonoBehaviour {
    //================================================================
    //public
    public bool IsCalculate; //計算をしているか

    //================================================================
    //private
    private double[] inputLayer; //入力層
    private double[] hiddenLayer; //隠れ層
    private double[] outputLayer; //出力層

    private double[] trainingData; //教師データ

    private double[,] weightI_H; //重み
    private double[,] weightH_O; //重み

    private double[,] weightInputI_H; //重み付き入力
    private double[,] weightInputH_O; //重み付き入力

    private double[] weightInputSumH; //重み付き入力を足し合わせたもの
    private double[] weightInputSumO; //重み付き入力を足し合わせたもの

    private double[] biasHiddenLayer; //バイアス
    private double[] biasOutputLayer; //バイアス

    private double[] errorsHiddenLayer; //誤差
    private double[] errorsOutputLayer; //誤差

    private double squareError; //二乗誤差

    private double[,] pd_of_squareError_wrt_weightI_H; //偏微分
    private double[,] pd_of_squareError_wrt_weightH_O; //偏微分
    private double[] pd_of_squareError_wrt_biasHiddenLayer; //偏微分
    private double[] pd_of_squareError_wrt_biasOutputLayer; //偏微分

    private double[,] gradient_weightI_H; //勾配
    private double[,] gradient_weightH_O; //勾配
    private double[] gradient_biasHiddenLayer; //勾配
    private double[] gradient_biasOutputLayer; //勾配

    private int calculateNum; //計算ループ数


    private float playingBias; //プレイングに応じて教師データにかかるバイアス

    //==============================================================
    //コンポーネント
    private ParameterManager parameterManager;
    private TextController weightAndBias;
    private TextController output;

    ///==============================================================
    //コンポーネント参照用
    private void ComponentRef () {
        parameterManager = GameObject.Find("ParameterManager").GetComponent<ParameterManager>();
        weightAndBias = transform.Find("WeightAndBias").GetComponent<TextController>();
        output = transform.Find("Output").GetComponent<TextController>();
    }

    //================================================================
    //ジグモイド関数
    private double Sigmoid (double _num) {
        return 1 / (1 + Math.Exp(_num));
    }

    //================================================================
    //プレイングバイアスを計算する
    //_mag -> 倍率
    public void ApplyPlayingBias (float _mag) {
        playingBias *= _mag;
        if(playingBias <= 0.1f) {
            playingBias = 0.1f;
        }

        if(playingBias >= 10) {
            playingBias = 10;
        }
    }

    //=================================================================
    public IEnumerator WriteOutput () {
        string tmp = "";
        tmp += parameterManager.NeuralNetworkID + 1 + "," + System.DateTime.Now.ToString("yyyy : MM : dd : HH : mm : ss") + ",";

        for(int i = 0;i < ValueDefinition.OUTPUT_LAYER_NUM;i++) {
            tmp += GetOutput(i) + ",";
        }

        output.WriteText(tmp);

        yield break;
    }


    //================================================================
    //入力層に入力する
    public void SetInput (int _num,double _input) {
        if(_num >= 0 && _num <= ValueDefinition.INPUT_LAYER_NUM - 1) {
            inputLayer[_num] = _input;
        } else {
            Debug.Log("NN入力ミス");
        }
    }

    //================================================================
    //出力層の数値を得る
    public double GetOutput (int _num) {
        if(_num >= 0 && _num <= ValueDefinition.OUTPUT_LAYER_NUM - 1) {
            return outputLayer[_num];
        } else {
            Debug.Log("NN出力ミス");
            return -1;
        }
    }

    //================================================================
    //初期化
    private void Initialize () {
        inputLayer = new double[ValueDefinition.INPUT_LAYER_NUM];
        hiddenLayer = new double[ValueDefinition.HIDDEN_LAYER_NUM];
        outputLayer = new double[ValueDefinition.OUTPUT_LAYER_NUM];

        trainingData = new double[ValueDefinition.OUTPUT_LAYER_NUM];

        weightI_H = new double[ValueDefinition.INPUT_LAYER_NUM,ValueDefinition.HIDDEN_LAYER_NUM];
        weightH_O = new double[ValueDefinition.HIDDEN_LAYER_NUM,ValueDefinition.OUTPUT_LAYER_NUM];

        weightInputI_H = new double[ValueDefinition.INPUT_LAYER_NUM,ValueDefinition.HIDDEN_LAYER_NUM];
        weightInputH_O = new double[ValueDefinition.HIDDEN_LAYER_NUM,ValueDefinition.OUTPUT_LAYER_NUM];

        weightInputSumH = new double[ValueDefinition.HIDDEN_LAYER_NUM];
        weightInputSumO = new double[ValueDefinition.OUTPUT_LAYER_NUM];

        biasHiddenLayer = new double[ValueDefinition.HIDDEN_LAYER_NUM];
        biasOutputLayer = new double[ValueDefinition.OUTPUT_LAYER_NUM];

        errorsHiddenLayer = new double[ValueDefinition.HIDDEN_LAYER_NUM];
        errorsOutputLayer = new double[ValueDefinition.OUTPUT_LAYER_NUM];

        squareError = 0;

        pd_of_squareError_wrt_weightI_H = new double[ValueDefinition.INPUT_LAYER_NUM,ValueDefinition.HIDDEN_LAYER_NUM];
        pd_of_squareError_wrt_weightH_O = new double[ValueDefinition.HIDDEN_LAYER_NUM,ValueDefinition.OUTPUT_LAYER_NUM];
        pd_of_squareError_wrt_biasHiddenLayer = new double[ValueDefinition.HIDDEN_LAYER_NUM];
        pd_of_squareError_wrt_biasOutputLayer = new double[ValueDefinition.OUTPUT_LAYER_NUM];

        gradient_weightI_H = new double[ValueDefinition.INPUT_LAYER_NUM,ValueDefinition.HIDDEN_LAYER_NUM];
        gradient_weightH_O = new double[ValueDefinition.HIDDEN_LAYER_NUM,ValueDefinition.OUTPUT_LAYER_NUM];
        gradient_biasHiddenLayer = new double[pd_of_squareError_wrt_biasHiddenLayer.Length];
        gradient_biasOutputLayer = new double[pd_of_squareError_wrt_biasOutputLayer.Length];

        playingBias = 1f;
    }

    //================================================================
    //メイン
    private void Awake () {
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start () {
        ComponentRef();
        Initialize();
    }

    //================================================================
    //計算
    public IEnumerator Calcalate (string _type) {
        IsCalculate = true;

        switch(_type) {
            case "First":
            yield return ReadWeightAndBias();
            break;

            case "StartStage":
            yield return FirstOutput();
            break;

            case "NextStage":
            if(parameterManager.Mode == "A") {
                yield return weightAndBias.C_ReadText();
                yield return ReadWeightAndBias();
                yield return CalculateBPNN();
                yield return CalculateFFNN();

                yield return WriteOutput();

                parameterManager.NeuralNetworkID++;

                yield return WriteWeightAndBias();
            } else {
                yield return new WaitForSeconds(1);
            }
            break;

            case "Result":
            if(parameterManager.Mode == "A") {
                yield return weightAndBias.C_ReadText();
                yield return ReadWeightAndBias();
                yield return CalculateBPNN();
                yield return CalculateFFNN();

                yield return WriteOutput();

                parameterManager.NeuralNetworkID++;

                yield return WriteWeightAndBias();
            } else {
                yield return new WaitForSeconds(1);
            }
            break;

            default:
            break;
        }

        parameterManager.DefeatEnemyNumInThisFloor = 0;
        playingBias = 1;
        IsCalculate = false;
        yield break;
    }

    //================================================================
    //最初のアウトプット
    private IEnumerator FirstOutput () {
        for(int i = 0;i < outputLayer.Length;i++) {
            outputLayer[i] = UnityEngine.Random.Range(0.1f,0.7f);
        }
        yield break;
    }

    //================================================================
    //テキストファイルの重み、バイアスを読み込む
    private IEnumerator ReadWeightAndBias () {
        string s_id = weightAndBias.GetData(weightAndBias.GetDataSize() - 5,0);
        Debug.Log(weightAndBias.GetDataSize() - 5 + "/" + s_id);
        if(s_id != "none") {
            int id = int.Parse(s_id);
            parameterManager.NeuralNetworkID = id;

            for(int i = 0;i < hiddenLayer.Length;i++) {
                for(int j = 0;j < inputLayer.Length;j++) {
                    weightI_H[j,i] = double.Parse(weightAndBias.GetData(1 + id * 5,10 + j + i * inputLayer.Length));
                }

                biasHiddenLayer[i] = double.Parse(weightAndBias.GetData(3 + id * 5,10 + i));
            }

            for(int i = 0;i < outputLayer.Length;i++) {
                for(int j = 0;j < hiddenLayer.Length;j++) {
                    weightH_O[j,i] = double.Parse(weightAndBias.GetData(2 + id * 5,10 + j + i * inputLayer.Length));
                }

                biasOutputLayer[i] = double.Parse(weightAndBias.GetData(4 + id * 5,10 + i));
            }
        } else {
            parameterManager.NeuralNetworkID = 0;

            for(int i = 0;i < hiddenLayer.Length;i++) {
                for(int j = 0;j < inputLayer.Length;j++) {
                    weightI_H[j,i] = UnityEngine.Random.Range(0f,1f);
                    //Debug.Log("w:" + weightI_H[j,i]);
                }

                biasHiddenLayer[i] = UnityEngine.Random.Range(0f,1f);
                //Debug.Log("b:" + biasHiddenLayer[i]);
            }

            for(int i = 0;i < outputLayer.Length;i++) {
                for(int j = 0;j < hiddenLayer.Length;j++) {
                    weightH_O[j,i] = UnityEngine.Random.Range(0f,1f);
                    //Debug.Log("w:" + weightH_O[j,i]);
                }

                biasOutputLayer[i] = UnityEngine.Random.Range(0f,1f);
                //Debug.Log("b:" + biasOutputLayer[i]);
            }

            yield return WriteWeightAndBias();
        }

        yield break;
    }

    //================================================================
    //テキストファイルに重み、バイアスを書き込む
    private IEnumerator WriteWeightAndBias () {
        string tmpWIH = "";
        string tmpBH = "";
        string tmpWHO = "";
        string tmpBO = "";

        tmpWIH += "weightI_H,x,x,x,x,x,x,x,x,x,";
        tmpBH += "biasHiddenLayer,x,x,x,x,x,x,x,x,x,";
        for(int i = 0;i < hiddenLayer.Length;i++) {
            for(int j = 0;j < inputLayer.Length;j++) {
                tmpWIH += weightI_H[j,i] + ",";
            }
            tmpBH += biasHiddenLayer[i] + ",";
        }

        tmpWHO += "weightH_O,x,x,x,x,x,x,x,x,x,";
        tmpBO += "biasOutputLayer,x,x,x,x,x,x,x,x,x,";
        for(int i = 0;i < outputLayer.Length;i++) {
            for(int j = 0;j < hiddenLayer.Length;j++) {
                tmpWHO += weightH_O[j,i] + ",";
            }
            tmpBO += biasOutputLayer[i] + ",";
        }

        weightAndBias.WriteText(
           parameterManager.NeuralNetworkID + "," +
           DateTime.Now.ToString("yyyy : MM : dd : HH : mm : ss") + "," +
           ValueDefinition.INPUT_LAYER_NUM + "," +
           ValueDefinition.HIDDEN_LAYER_NUM + "," +
           ValueDefinition.OUTPUT_LAYER_NUM + "," +
           playingBias + "," +
           "x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,"
            );
        weightAndBias.WriteText(tmpWIH);
        weightAndBias.WriteText(tmpWHO);
        weightAndBias.WriteText(tmpBH);
        weightAndBias.WriteText(tmpBO);

        yield break;
    }

    //================================================================
    //順伝播
    private IEnumerator CalculateFFNN () {
        //入力層 -> 隠れ層
        for(int i = 0;i < hiddenLayer.Length;i++) {
            for(int j = 0;j < inputLayer.Length;j++) {
                weightInputI_H[j,i] = inputLayer[j] * weightI_H[j,i];
                weightInputSumH[i] += weightInputI_H[j,i];
            }

            hiddenLayer[i] = Sigmoid(weightInputSumH[i] + biasHiddenLayer[i]);
        }


        //隠れ層 -> 出力層
        for(int i = 0;i < outputLayer.Length;i++) {
            for(int j = 0;j < hiddenLayer.Length;j++) {
                weightInputH_O[j,i] = hiddenLayer[j] * weightH_O[j,i];
                weightInputSumO[i] += weightInputH_O[j,i];
            }

            outputLayer[i] = Sigmoid(weightInputSumO[i] + biasOutputLayer[i]);
        }

        yield break;
    }

    //================================================================
    //誤差逆伝播
    private IEnumerator CalculateBPNN () {
        yield return CalculateFFNN();

        Debug.Log(playingBias);

        //教師データの作成(プレイングバイアスを出力にかける)
        for(int i = 0;i < ValueDefinition.FIELD_DIVIDE_NUM_Y;i++) {
            for(int j = 0;j < ValueDefinition.FIELD_DIVIDE_NUM_X;j++) {
                int index = i * ValueDefinition.FIELD_DIVIDE_NUM_X + j;
                double tmp = outputLayer[index] * (playingBias * UnityEngine.Random.Range(1f,1.3f));
                trainingData[index] = tmp >= 1 ? 1 : tmp;
                Debug.Log("origin:" + outputLayer[index] + "t:" + index + ";" + trainingData[index]);
            }
        }

        //教師データの作成(出力をプレイングバイアスで割る)
        for(int i = 0;i < ValueDefinition.FIELD_DIVIDE_NUM_Y;i++) {
            for(int j = 0;j < ValueDefinition.FIELD_DIVIDE_NUM_X;j++) {
                int index = ValueDefinition.FIELD_DIVIDE_NUM_X * ValueDefinition.FIELD_DIVIDE_NUM_Y + i * ValueDefinition.FIELD_DIVIDE_NUM_X + j;
                double tmp = outputLayer[index] / (playingBias * UnityEngine.Random.Range(1f,1.3f));
                trainingData[index] = tmp >= 1 ? 1 : tmp;
                Debug.Log("origin:" + outputLayer[index] + "t:" + index + ";" + trainingData[index]);
            }
        }



        /*for(int i = 0;i < ValueDefinition.OUTPUT_LAYER_NUM;i++) {
            double tmp = outputLayer[i] * playingBias * UnityEngine.Random.Range(1f,2f);
            trainingData[i] = tmp >= 1 ? 1 : tmp;
            Debug.Log("origin:" + outputLayer[i] + "t:" + i + ";" + trainingData[i]);
        }*/

        while(true) {
            yield return CalculateFFNN();

            //二乗誤差の計算
            double tmp = 0;

            for(int i = 0;i < ValueDefinition.OUTPUT_LAYER_NUM;i++) {
                tmp = +Math.Pow(trainingData[i] - outputLayer[i],2);
            }

            squareError = tmp / 2d;

            //誤差の計算
            for(int i = 0;i < ValueDefinition.OUTPUT_LAYER_NUM;i++) {
                errorsOutputLayer[i] = (trainingData[i] - outputLayer[i]) * Sigmoid(weightInputSumO[i]) * (1 - Sigmoid(weightInputSumO[i]));
                //Debug.Log("errorsOutputLayer:" +i+"::"+ errorsOutputLayer[i]);
            }

            for(int i = 0;i < ValueDefinition.HIDDEN_LAYER_NUM;i++) {
                for(int j = 0;j < ValueDefinition.OUTPUT_LAYER_NUM;j++) {
                    errorsHiddenLayer[i] += (errorsOutputLayer[j] * weightH_O[i,j]);
                }

                errorsHiddenLayer[i] *= Sigmoid(weightInputSumH[i]) * (1 - weightInputSumH[i]);
                //Debug.Log("errorsHiddenLayer:" + i + "::" + errorsHiddenLayer[i]);
            }
            //Debug.Log("");

            //勾配の計算
            for(int i = 0;i < ValueDefinition.HIDDEN_LAYER_NUM;i++) {
                for(int j = 0;j < ValueDefinition.INPUT_LAYER_NUM;j++) {
                    pd_of_squareError_wrt_weightI_H[j,i] = errorsHiddenLayer[i] * inputLayer[j];
                    gradient_weightI_H[j,i] = pd_of_squareError_wrt_weightI_H[j,i] * ValueDefinition.LEARNING_RATE * (-1);
                    //Debug.Log("gradient_weightI_H:" + pd_of_squareError_wrt_weightI_H[j,i]);
                }
            }

            for(int i = 0;i < ValueDefinition.OUTPUT_LAYER_NUM;i++) {
                for(int j = 0;j < ValueDefinition.HIDDEN_LAYER_NUM;j++) {
                    pd_of_squareError_wrt_weightH_O[j,i] = errorsOutputLayer[i] * hiddenLayer[j];
                    gradient_weightH_O[j,i] = pd_of_squareError_wrt_weightH_O[j,i] * ValueDefinition.LEARNING_RATE * (-1);
                    //Debug.Log("gradient_weightH_O:" + pd_of_squareError_wrt_weightH_O[j,i]);
                }
            }
            //Debug.Log("");

            for(int i = 0;i < ValueDefinition.HIDDEN_LAYER_NUM;i++) {
                pd_of_squareError_wrt_biasHiddenLayer[i] = errorsHiddenLayer[i];
                gradient_biasHiddenLayer[i] = pd_of_squareError_wrt_biasHiddenLayer[i] * ValueDefinition.LEARNING_RATE * (-1);
                //Debug.Log("gradient_biasHiddenLayer:" + gradient_biasHiddenLayer[i]);
            }

            for(int i = 0;i < ValueDefinition.OUTPUT_LAYER_NUM;i++) {
                pd_of_squareError_wrt_biasOutputLayer[i] = errorsOutputLayer[i];
                gradient_biasOutputLayer[i] = pd_of_squareError_wrt_biasOutputLayer[i] * ValueDefinition.LEARNING_RATE * (-1);
                //Debug.Log("gradient_biasOutputLayer:" + gradient_biasOutputLayer[i]);
            }

            //勾配の適用
            for(int i = 0;i < ValueDefinition.HIDDEN_LAYER_NUM;i++) {
                for(int j = 0;j < ValueDefinition.INPUT_LAYER_NUM;j++) {
                    weightI_H[j,i] += gradient_weightI_H[j,i];
                }
            }

            for(int i = 0;i < ValueDefinition.OUTPUT_LAYER_NUM;i++) {
                for(int j = 0;j < ValueDefinition.HIDDEN_LAYER_NUM;j++) {
                    weightH_O[j,i] += gradient_weightH_O[j,i];
                }
            }

            for(int i = 0;i < ValueDefinition.HIDDEN_LAYER_NUM;i++) {
                biasHiddenLayer[i] += gradient_biasHiddenLayer[i];
            }

            for(int i = 0;i < ValueDefinition.OUTPUT_LAYER_NUM;i++) {
                biasOutputLayer[i] += gradient_biasOutputLayer[i];
            }

            calculateNum++;
            if(calculateNum % 100 == 0) {
                Debug.Log(calculateNum);
                Debug.Log("squared Error:" + squareError);
            }

            //二乗誤差の値がかなり小さくなったら、または学習回数が1000を超えたらループを出る
            if(Math.Abs(squareError) <= 0.00001d || calculateNum >= 1000) {
                Debug.Log(calculateNum);
                calculateNum = 0;
                break;
            }

            yield return new WaitForEndOfFrame();
        }

        yield break;
    }
}
