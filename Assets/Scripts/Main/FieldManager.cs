using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.UI;

public class FieldManager : MonoBehaviour {
    //=================================================================
    //シリアライズ(private)
    [SerializeField]
    private Sprite[] fieldChipSprites; //マップチップ用のスプライト

    //=================================================================
    //public
    public int[,] fieldData; //フィールドデータ(グリッド)
    public Vector2[,] fieldDataConcernDivide; //分轄フィールドに関するフィールドデータ

    public Vector2 FirstPlayerPos; //プレイヤーの位置
    public Vector2Int stairPos; //階段の位置

    public List<Vector2Int> enemiesPos = new List<Vector2Int>(); //敵の位置

    //=================================================================
    //private
    private Vector2[,] centerPoint; //分割フィールドの中心座標(x,y)
    private int[,] divideFieldLeftEnd; //分割フィールドの左端(x,y)
    private int[,] divideFieldRightEnd; //分割フィールドの右端(x,y)
    private int[,] divideFieldUpEnd; //分割フィールドの上端(x,y)
    private int[,] divideFieldDownEnd; //分割フィールドの下端(x,y)
    private bool[,,] pathStretchable; //道を伸ばせるかどうか(x,y,上下左右) 
    private Vector2[,,] pathEnd; //道の先
    private int centerPointLimitRange; //分割フィールドの中心座標を決めるときの範囲制限(中心座標が壁と隣接するのを防ぐ)
    private int endPointLimitRange; //分割フィールドの端座標を決めるときの範囲制限

    private Vector2 playerPosInDivideField; //プレイヤーの位置はどの分割フィールドに位置するか
    private Vector2 stairPosInDivideField; //階段の位置はどの分割フィールドに位置するか

    private List<Vector2Int> enemiesPosInDivideField = new List<Vector2Int>(); //敵の位置はどの分轄フィールドに位置するか

    //=================================================================
    //コンポーネント参照用
    private void ComponentRef () {
        manager = GameObject.Find("Manager").GetComponent<Manager>();
        itemManager = transform.Find("ItemManager").GetComponent<ItemManager>();
        neuralNetworkManager = GameObject.Find("NeuralNetworkManager").GetComponent<NeuralNetworkManager>();
        parameterManager = GameObject.Find("ParameterManager").GetComponent<ParameterManager>();
    }

    //=================================================================
    //コンポーネント
    private Manager manager; //マネージャー
    private ItemManager itemManager; //アイテムマネージャー
    private NeuralNetworkManager neuralNetworkManager; //ニューラルネットワークマネージャー
    private ParameterManager parameterManager; //パラメータマネージャー
    

    //==============================================================
    //ニューラルネットワークマネージャーが存在するか確認する
    private void CheckPreloadtNeuralNetworkManager () {
        if(GameObject.FindWithTag("NeuralNetwork") == null) {
            GameObject obj = Instantiate(Resources.Load("Prefabs/NeuralNetworkManager")) as GameObject;
            obj.name = "NeuralNetworkManager";
        }
    }

    //=================================================================
    //敵の数
    public int EnemyNum () {
        return enemiesPos.Count;
    }

    //=================================================================
    //敵を増やす
    private void AddEnemyData (int _posX,int _posY) {
        //int _posX = Random.Range(0,ValueDefinition.FIELD_DIVIDE_NUM_X);
        //int _posY = Random.Range(0,ValueDefinition.FIELD_DIVIDE_NUM_Y);
        enemiesPosInDivideField.Add(new Vector2Int(_posX,_posY));
    }

    //=================================================================
    //初期化関数
    //numX -> 分割フィールドの横の数
    //numY -> 分割フィールドの縦の数
    private void Initialize (int numX,int numY) {
        fieldData = new int[ValueDefinition.FIELD_SIZE_X,ValueDefinition.FIELD_SIZE_Y];
        fieldDataConcernDivide = new Vector2[ValueDefinition.FIELD_SIZE_X,ValueDefinition.FIELD_SIZE_Y];

        centerPointLimitRange = 5;
        endPointLimitRange = 2;
        centerPoint = new Vector2[numX,numY];
        divideFieldLeftEnd = new int[numX,numY];
        divideFieldRightEnd = new int[numX,numY];
        divideFieldUpEnd = new int[numX,numY];
        divideFieldDownEnd = new int[numX,numY];
        pathStretchable = new bool[numX,numY,4];
        pathEnd = new Vector2[numX,numY,4];

        //分割フィールドの中心座標を決める
        for(int i = 0;i < numX;i++) {
            for(int j = 0;j < numY;j++) {
                divideFieldLeftEnd[i,j] = (ValueDefinition.FIELD_SIZE_X / numX) * i + endPointLimitRange;
                divideFieldRightEnd[i,j] = (ValueDefinition.FIELD_SIZE_X / numX) * (i + 1) - endPointLimitRange;
                divideFieldUpEnd[i,j] = (ValueDefinition.FIELD_SIZE_Y / numY) * (j + 1) - endPointLimitRange;
                divideFieldDownEnd[i,j] = (ValueDefinition.FIELD_SIZE_Y / numY) * j + endPointLimitRange;

                int rangeX = Random.Range(divideFieldLeftEnd[i,j] + centerPointLimitRange,divideFieldRightEnd[i,j] - centerPointLimitRange);
                int rangeY = Random.Range(divideFieldDownEnd[i,j] + centerPointLimitRange,divideFieldUpEnd[i,j] - centerPointLimitRange);

                //Debug.Log("zahyou->"+i+"/"+j+":range->"+rangeX+"/"+rangeY);
                centerPoint[i,j] = new Vector2(rangeX,rangeY);

                //道を伸ばすかどうかを保存
                //まずランダムに上下左右
                for(int k = 0;k < 4;k++) {
                    if(Random.value >= 0f) {
                        pathStretchable[i,j,k] = true;
                    }
                }

                //端かどうかの判定
                //上
                if(j == numY - 1) {
                    pathStretchable[i,j,0] = false;
                }

                //下
                if(j == 0) {
                    pathStretchable[i,j,1] = false;
                }

                //左
                if(i == 0) {
                    pathStretchable[i,j,2] = false;
                }

                //右
                if(i == numX - 1) {
                    pathStretchable[i,j,3] = false;
                }
            }
        }
    }

    //=================================================================
    //メイン関数
    private void Awake () {
        CheckPreloadtNeuralNetworkManager();
        ComponentRef();
        Initialize(ValueDefinition.FIELD_DIVIDE_NUM_X,ValueDefinition.FIELD_DIVIDE_NUM_Y);
        StartCoroutine(SetUpField());
    }

    //=================================================================
    private IEnumerator SetUpField () {
        yield return CreateFieldData(fieldData);
        Debug.Log("FieldData created.");
        yield return DecidePlayerPos();
        Debug.Log("Player pos set.");
        yield return DecideStairPos();
        Debug.Log("Stair pos set.");
        yield return DecideEnemiesPos();
        Debug.Log("Enemies pos set.");
        yield return CreateItems();
        Debug.Log("Item created.");
        yield return CreateField();
    }

    //=================================================================
    //グリッド状のフィールドデータを作成する
    //data -> フィールドデータ

    private IEnumerator CreateFieldData (int[,] data) {

        //最初にフィールド全部を壁に
        //x軸方向
        for(int i = 0;i < data.GetLength(0);i++) {
            //y軸方向
            for(int j = 0;j < data.GetLength(1);j++) {
                data[i,j] = 1;
                fieldDataConcernDivide[i,j] = new Vector2(-1,-1);
            }
        }

        //分割フィールドの中心座標に応じてフィールドを広げる
        //中心座標(x軸方向)
        for(int i = 0;i < centerPoint.GetLength(0);i++) {
            //中心座標(y軸方向)
            for(int j = 0;j < centerPoint.GetLength(1);j++) {

                //フィールドの広さを決める(上下左右端を定める)
                int dLeft = Random.Range(divideFieldLeftEnd[i,j],(int)centerPoint[i,j].x - (ValueDefinition.MIN_ROOM_SIZE_X - 1) + 1);
                int dRight = Random.Range((int)centerPoint[i,j].x,divideFieldRightEnd[i,j] + 1);
                int dDown = Random.Range(divideFieldDownEnd[i,j],(int)centerPoint[i,j].y - (ValueDefinition.MIN_ROOM_SIZE_Y - 1) + 1);
                int dUp = Random.Range((int)centerPoint[i,j].y,divideFieldUpEnd[i,j] + 1);

                //上下左右端を元の変数に適用する
                divideFieldLeftEnd[i,j] = dLeft;
                divideFieldRightEnd[i,j] = dRight;
                divideFieldUpEnd[i,j] = dUp;
                divideFieldDownEnd[i,j] = dDown;

                //フィールドを分割フィールド中心から広げる
                for(int k = dLeft;k < dRight + 1;k++) {
                    for(int l = dDown;l < dUp + 1;l++) {
                        data[k,l] = 0;
                        fieldDataConcernDivide[k,l] = new Vector2((i + 1),(j + 1));
                    }
                }

                //道を作る(枝先を作る)

                //道の終わり一時保存用変数
                int px = -1;
                int py = -1;

                //道の初期長さ
                int initPathLength = 2;

                //上
                if(pathStretchable[i,j,0]) {
                    for(int k = 0;k < initPathLength;k++) {
                        px = (int)centerPoint[i,j].x;
                        py = dUp + (k + 1);
                        data[px,py] = 0;
                        if(fieldDataConcernDivide[px,py].x == -1 || fieldDataConcernDivide[px,py].y == -1) {
                            fieldDataConcernDivide[px,py] = new Vector2((i + 1),(j + 1) * 10 + (j + 1 + 1));
                        }
                        pathEnd[i,j,0] = new Vector2(px,py); //道の終わりを保存
                    }
                }

                //下
                if(pathStretchable[i,j,1]) {
                    for(int k = 0;k < initPathLength;k++) {
                        px = (int)centerPoint[i,j].x;
                        py = dDown - (k + 1);
                        data[px,py] = 0;
                        if(fieldDataConcernDivide[px,py].x == -1 || fieldDataConcernDivide[px,py].y == -1) {
                            fieldDataConcernDivide[px,py] = new Vector2((i + 1),(j + 1) * 10 + (j + 1 - 1));
                        }
                        pathEnd[i,j,1] = new Vector2(px,py); //道の終わりを保存
                    }
                }

                //左
                if(pathStretchable[i,j,2]) {
                    for(int k = 0;k < initPathLength;k++) {
                        px = dLeft - (k + 1);
                        py = (int)centerPoint[i,j].y;
                        data[px,py] = 0;
                        if(fieldDataConcernDivide[px,py].x == -1 || fieldDataConcernDivide[px,py].y == -1) {
                            fieldDataConcernDivide[px,py] = new Vector2((i + 1) * 10 + (i + 1 - 1),(j + 1));
                        }
                        pathEnd[i,j,2] = new Vector2(px,py); //道の終わりを保存
                    }
                }

                //右
                if(pathStretchable[i,j,3]) {
                    for(int k = 0;k < initPathLength;k++) {
                        px = dRight + (k + 1);
                        py = (int)centerPoint[i,j].y;
                        data[px,py] = 0;
                        if(fieldDataConcernDivide[px,py].x == -1 || fieldDataConcernDivide[px,py].y == -1) {
                            fieldDataConcernDivide[px,py] = new Vector2((i + 1) * 10 + (i + 1 + 1),(j + 1));
                        }
                        pathEnd[i,j,3] = new Vector2(px,py); //道の終わりを保存
                    }
                }
            }
        }

        //道を作る(枝を伸ばしつなげる)
        //ATTENTION:バグおきそう//////////////////////////////////////////////////////////////////
        //MEMO:上下左右の柔軟性がない/////////////////////////////////////////////////////////////

        int bpx = -1;
        int bpy = -1;

        //中心座標(x軸方向)
        for(int i = 0;i < centerPoint.GetLength(0);i++) {
            //中心座標(y軸方向)
            for(int j = 0;j < centerPoint.GetLength(1);j++) {

                //上に伸ばす、その後横につながるまで伸ばす
                if(pathStretchable[i,j,0]) {

                    //ループ回数用変数を用意し、ループ回数が20回を超えた場合一時待機処理を入れる
                    int loopNest1 = 0;
                    while(true) {
                        bpx = (int)pathEnd[i,j,0].x;
                        bpy = (int)pathEnd[i,j,0].y + 1;

                        data[bpx,bpy] = 0;
                        if(fieldDataConcernDivide[bpx,bpy].x == -1 || fieldDataConcernDivide[bpx,bpy].y == -1) {
                            fieldDataConcernDivide[bpx,bpy] = new Vector2((i + 1),(j + 1) * 10 + (j + 1 + 1));
                        }
                        pathEnd[i,j,0].y = bpy;

                        //「枝先(上)のy座標」が「上隣の分割フィールドの枝先(下)のy座標より大きくなったら
                        if((int)pathEnd[i,j,0].y >= (int)pathEnd[i,j + 1,1].y) {

                            //ループ回数用変数を用意し、ループ回数が20回を超えた場合一時待機処理を入れる
                            int loopNest2 = 0;
                            while(true) {
                                //「枝先(上)のx座標」が「上隣りの分割フィールドの枝先(下)のx座標」より大きいなら左
                                if((int)pathEnd[i,j,0].x > (int)pathEnd[i,j + 1,1].x) {
                                    bpx = (int)pathEnd[i,j,0].x - 1;
                                    bpy = (int)pathEnd[i,j,0].y;

                                    data[bpx,bpy] = 0;
                                    if(fieldDataConcernDivide[bpx,bpy].x == -1 || fieldDataConcernDivide[bpx,bpy].y == -1) {
                                        fieldDataConcernDivide[bpx,bpy] = new Vector2((i + 1) * 10 + (i + 1 - 1),(j + 1) * 10 + (j + 1 + 1));
                                    }
                                    pathEnd[i,j,0] = new Vector2(bpx,bpy);
                                }

                                //「枝先(上)のx座標」が「上隣りの分割フィールドの枝先(下)のx座標」より小さいなら右
                                if((int)pathEnd[i,j,0].x < (int)pathEnd[i,j + 1,1].x) {
                                    bpx = (int)pathEnd[i,j,0].x + 1;
                                    bpy = (int)pathEnd[i,j,0].y;

                                    data[bpx,bpy] = 0;
                                    if(fieldDataConcernDivide[bpx,bpy].x == -1 || fieldDataConcernDivide[bpx,bpy].y == -1) {
                                        fieldDataConcernDivide[bpx,bpy] = new Vector2((i + 1) * 10 + (i + 1 + 1),(j + 1) * 10 + (j + 1 + 1));
                                    }
                                    pathEnd[i,j,0] = new Vector2(bpx,bpy);
                                }

                                //「枝先(上)のx座標」と「上隣りの分割フィールドの枝先(下)のx座標」が同じなら終わり
                                if((int)pathEnd[i,j,0].x == (int)pathEnd[i,j + 1,1].x) {
                                    break;
                                }

                                loopNest2++;
                                if(loopNest2 >= 20) {
                                    loopNest2 = 0;
                                    //Debug.Log("上横：横");
                                    yield return new WaitForEndOfFrame(); //待機
                                }
                            }

                            break;
                        }

                        loopNest1++;
                        if(loopNest1 >= 20) {
                            loopNest1 = 0;
                            //Debug.Log("上横：上");
                            yield return new WaitForEndOfFrame(); //待機
                        }
                    }
                }

                //右に伸ばす、その後上下につながるまで伸ばす
                if(pathStretchable[i,j,3]) {
                    //Debug.Log(i+":"+j+"///"+(int)pathEnd[i,j,2].x+":"+(int)pathEnd[i,j,2].y);

                    //ループ回数用変数を用意し、ループ回数が20回を超えた場合一時待機処理を入れる
                    int loopNest1 = 0;
                    while(true) {
                        bpx = (int)pathEnd[i,j,3].x + 1;
                        bpy = (int)pathEnd[i,j,3].y;

                        data[bpx,bpy] = 0;
                        if(fieldDataConcernDivide[bpx,bpy].x == -1 || fieldDataConcernDivide[bpx,bpy].y == -1) {
                            fieldDataConcernDivide[bpx,bpy] = new Vector2((i + 1) * 10 + (i + 1 + 1),(j + 1));
                        }
                        pathEnd[i,j,3].x = bpx;

                        //「枝先(右)のx座標」が「右隣の分割フィールドの枝先(左)のx座標より大きくなったら
                        if((int)pathEnd[i,j,3].x >= (int)pathEnd[i + 1,j,2].x) {

                            //ループ回数用変数を用意し、ループ回数が20回を超えた場合一時待機処理を入れる
                            int loopNest2 = 0;
                            while(true) {
                                //「枝先(右)のy座標」が「右隣りの分割フィールドの枝先(左)のy座標」より大きいなら下
                                if((int)pathEnd[i,j,3].y > (int)pathEnd[i + 1,j,2].y) {
                                    bpx = (int)pathEnd[i,j,3].x;
                                    bpy = (int)pathEnd[i,j,3].y - 1;

                                    data[bpx,bpy] = 0;
                                    if(fieldDataConcernDivide[bpx,bpy].x == -1 || fieldDataConcernDivide[bpx,bpy].y == -1) {
                                        fieldDataConcernDivide[bpx,bpy] = new Vector2((i + 1) * 10 + (i + 1 + 1),(j + 1) * 10 + (j + 1 - 1));
                                    }
                                    pathEnd[i,j,3] = new Vector2(bpx,bpy);
                                }

                                //「枝先(右)のy座標」が「右隣りの分割フィールドの枝先(左)のy座標」より小さいなら上
                                if((int)pathEnd[i,j,3].y < (int)pathEnd[i + 1,j,2].y) {
                                    bpx = (int)pathEnd[i,j,3].x;
                                    bpy = (int)pathEnd[i,j,3].y + 1;

                                    data[bpx,bpy] = 0;
                                    if(fieldDataConcernDivide[bpx,bpy].x == -1 || fieldDataConcernDivide[bpx,bpy].y == -1) {
                                        fieldDataConcernDivide[bpx,bpy] = new Vector2((i + 1) * 10 + (i + 1 + 1),(j + 1) * 10 + (j + 1 + 1));
                                    }
                                    pathEnd[i,j,3] = new Vector2(bpx,bpy);
                                }

                                //「枝先(右)のy座標」と「右隣りの分割フィールドの枝先(左)のy座標」同じなら終わり
                                if((int)pathEnd[i,j,3].y == (int)pathEnd[i + 1,j,2].y) {
                                    break;
                                }

                                loopNest2++;
                                if(loopNest2 >= 20) {
                                    loopNest2 = 0;
                                    //Debug.Log("横上：上");
                                    yield return new WaitForEndOfFrame(); //待機
                                }
                            }

                            break;
                        }

                        loopNest1++;
                        if(loopNest1 >= 20) {
                            loopNest1 = 0;
                            //Debug.Log("横上：横");
                            yield return new WaitForEndOfFrame(); //待機
                        }
                    }
                }
            }
        }
    }

    //=================================================================
    //プレイヤーの位置を決定する
    private IEnumerator DecidePlayerPos () {
        playerPosInDivideField = new Vector2((int)Random.Range(0,ValueDefinition.FIELD_DIVIDE_NUM_X),(int)Random.Range(0,ValueDefinition.FIELD_DIVIDE_NUM_Y)); //どの分割フィールドにいるかを決める

        //どの分割フィールドにいるかに応じて上下左右端を指定
        int playerPosLeftSide = divideFieldLeftEnd[(int)playerPosInDivideField.x,(int)playerPosInDivideField.y];
        int playerPosRightSide = divideFieldRightEnd[(int)playerPosInDivideField.x,(int)playerPosInDivideField.y];
        int playerPosUpSide = divideFieldUpEnd[(int)playerPosInDivideField.x,(int)playerPosInDivideField.y];
        int playerPosDownSide = divideFieldDownEnd[(int)playerPosInDivideField.x,(int)playerPosInDivideField.y];

        FirstPlayerPos = new Vector2((int)Random.Range(playerPosLeftSide,playerPosRightSide),(int)Random.Range(playerPosUpSide,playerPosDownSide)); //位置の決定

        yield break;
    }

    //=================================================================
    //階段の位置を決定する
    private IEnumerator DecideStairPos () {
        stairPosInDivideField = new Vector2((int)Random.Range(0,ValueDefinition.FIELD_DIVIDE_NUM_X),(int)Random.Range(0,ValueDefinition.FIELD_DIVIDE_NUM_Y)); //どの分割フィールドにいるかを決める

        do {
            //どの分割フィールドにいるかに応じて上下左右端を指定
            int stairPosLeftSide = divideFieldLeftEnd[(int)stairPosInDivideField.x,(int)stairPosInDivideField.y];
            int stairPosRightSide = divideFieldRightEnd[(int)stairPosInDivideField.x,(int)stairPosInDivideField.y];
            int stairPosUpSide = divideFieldUpEnd[(int)stairPosInDivideField.x,(int)stairPosInDivideField.y];
            int stairPosDownSide = divideFieldDownEnd[(int)stairPosInDivideField.x,(int)stairPosInDivideField.y];

            stairPos = new Vector2Int(Random.Range(stairPosLeftSide,stairPosRightSide),Random.Range(stairPosUpSide,stairPosDownSide)); //位置の決定

            yield return new WaitForEndOfFrame();
        } while(IsExistPlayerOnThisPos(stairPos.x,stairPos.y)); //階段とプレイヤーの位置が同じなら再計算

        fieldData[(int)stairPos.x,(int)stairPos.y] = 2; //フィールドデータ上で階段に
        yield break;
    }

    //=================================================================
    //敵の位置を決定する
    private IEnumerator DecideEnemiesPos () {
        switch(parameterManager.Mode) {
            case "A":
            for(int i = 0;i < ValueDefinition.FIELD_DIVIDE_NUM_Y;i++) {
                for(int j = 0;j < ValueDefinition.FIELD_DIVIDE_NUM_X;j++) {

                    //ニューラルネットワークの出力から敵数を決める
                    int enemyNumByNeuralNetwork = 0;
                    for(int k = 0;k < ValueDefinition.EVENT_TABLE_ENEMY_NUM.Length;k++) {
                        //Debug.Log(neuralNetworkManager.GetOutput(j + i * ValueDefinition.FIELD_DIVIDE_NUM_X));
                        if(neuralNetworkManager.GetOutput(j + i * ValueDefinition.FIELD_DIVIDE_NUM_X) < ValueDefinition.EVENT_TABLE_ENEMY_NUM[k]) {
                            enemyNumByNeuralNetwork = k - 1;
                            Debug.Log((j + i * ValueDefinition.FIELD_DIVIDE_NUM_X) + "::" + neuralNetworkManager.GetOutput(j + i * ValueDefinition.FIELD_DIVIDE_NUM_X) + "::" + enemyNumByNeuralNetwork);
                            break;
                        }
                    }

                    for(int k = 0;k < enemyNumByNeuralNetwork;k++) {
                        AddEnemyData(i,j);
                    }
                }
            }
            break;

            case "B":
            for(int i = 0;i < ValueDefinition.FIELD_DIVIDE_NUM_Y;i++) {
                for(int j = 0;j < ValueDefinition.FIELD_DIVIDE_NUM_X;j++) {
                    int enemyNum = Random.Range(0,3);
                    for(int k = 0;k < enemyNum;k++) {
                        AddEnemyData(i,j);
                    }
                }
            }
            break;

            case "C":
            for(int i = 0;i < ValueDefinition.FIELD_DIVIDE_NUM_Y;i++) {
                for(int j = 0;j < ValueDefinition.FIELD_DIVIDE_NUM_X;j++) {
                    int enemyNum = Random.Range(2,4);
                    for(int k = 0;k < enemyNum;k++) {
                        AddEnemyData(i,j);
                    }
                }
            }
            break;

            case "P":
            for(int i = 0;i < ValueDefinition.FIELD_DIVIDE_NUM_Y;i++) {
                for(int j = 0;j < ValueDefinition.FIELD_DIVIDE_NUM_X;j++) {
                    int enemyNum = Random.Range(0,3);
                    for(int k = 0;k < enemyNum;k++) {
                        AddEnemyData(i,j);
                    }
                }
            }
            break;

            default:
            Debug.Log("モードエラー");
            break;
        }

        Vector2Int tmp = new Vector2Int();
        for(int i = 0;i < enemiesPosInDivideField.Count;i++) {
            do {
                //どの分割フィールドにいるかに応じて上下左右端を指定
                int enemiesPosLeftSide = divideFieldLeftEnd[enemiesPosInDivideField[i].x,enemiesPosInDivideField[i].y];
                int enemiesPosRightSide = divideFieldRightEnd[enemiesPosInDivideField[i].x,enemiesPosInDivideField[i].y];
                int enemiesPosUpSide = divideFieldUpEnd[enemiesPosInDivideField[i].x,enemiesPosInDivideField[i].y];
                int enemiesPosDownSide = divideFieldDownEnd[enemiesPosInDivideField[i].x,enemiesPosInDivideField[i].y];

                tmp = new Vector2Int(Random.Range(enemiesPosLeftSide,enemiesPosRightSide),Random.Range(enemiesPosUpSide,enemiesPosDownSide));

                //Debug.Log(i + ":" + tmp.x + "/" + tmp.y);
                yield return new WaitForEndOfFrame();
            } while(
            IsExistStairOnThisPos(tmp.x,tmp.y) ||
            IsExistPlayerOnThisPos(tmp.x,tmp.y) ||
            IsExistEnemiesOnThisPos(tmp.x,tmp.y)
            ); //階段の位置が同じ、またはプレイヤーの位置が同じ、または他の敵の位置が同じなら再計算           

            enemiesPos.Add(tmp); //位置情報の追加
            parameterManager.PlayingSkill++; //プレイングスキルに適用
        }

        yield break;
    }

    //=================================================================
    //敵の位置を変更する
    public IEnumerator ChangeEnemiesPos (Enemy _enemy) {
        Vector2Int tmp = new Vector2Int();
        int index = int.Parse(_enemy.name);

        do {
            //どの分割フィールドにいるかを決める
            enemiesPosInDivideField[index] = new Vector2Int(Random.Range(0,ValueDefinition.FIELD_DIVIDE_NUM_X),Random.Range(0,ValueDefinition.FIELD_DIVIDE_NUM_Y));

            //どの分割フィールドにいるかに応じて上下左右端を指定
            int enemiesPosLeftSide = divideFieldLeftEnd[enemiesPosInDivideField[index].x,enemiesPosInDivideField[index].y];
            int enemiesPosRightSide = divideFieldRightEnd[enemiesPosInDivideField[index].x,enemiesPosInDivideField[index].y];
            int enemiesPosUpSide = divideFieldUpEnd[enemiesPosInDivideField[index].x,enemiesPosInDivideField[index].y];
            int enemiesPosDownSide = divideFieldDownEnd[enemiesPosInDivideField[index].x,enemiesPosInDivideField[index].y];

            tmp = new Vector2Int(Random.Range(enemiesPosLeftSide,enemiesPosRightSide),Random.Range(enemiesPosUpSide,enemiesPosDownSide));

            //Debug.Log(i + ":" + tmp.x + "/" + tmp.y);
            yield return new WaitForEndOfFrame();
        } while(
        IsExistStairOnThisPos(tmp.x,tmp.y) ||
        IsExistPlayerOnThisPos(tmp.x,tmp.y) ||
        IsExistEnemiesOnThisPos(tmp.x,tmp.y)
        ); //階段の位置が同じ、またはプレイヤーの位置が同じ、または他の敵の位置が同じなら再計算       

        //位置情報の変更
        _enemy.ApplyPositionStep(tmp.x,tmp.y);

        yield break;
    }

    //=================================================================
    //アイテムを生成する
    private IEnumerator CreateItems () {
        List<Vector2Int> tmpDivPos = new List<Vector2Int>();
        Vector2Int tmpPos = new Vector2Int();

        switch(parameterManager.Mode) {
            case "A":
            //ニューラルネットワークの出力からアイテム数を決める
            int itemNumByNeuralNetwork = 1;
            for(int i = 0;i < ValueDefinition.FIELD_DIVIDE_NUM_Y;i++) {
                for(int j = 0;j < ValueDefinition.FIELD_DIVIDE_NUM_X;j++) {
                    for(int k = 0;k < ValueDefinition.EVENT_TABLE_ITEM_NUM.Length;k++) {
                        int index = ValueDefinition.FIELD_DIVIDE_NUM_X * ValueDefinition.FIELD_DIVIDE_NUM_Y + j + i * ValueDefinition.FIELD_DIVIDE_NUM_X;

                        if(neuralNetworkManager.GetOutput(index) < ValueDefinition.EVENT_TABLE_ENEMY_NUM[k]) {
                            itemNumByNeuralNetwork = k - 1;
                            break;
                        }
                    }

                    for(int k = 0;k < itemNumByNeuralNetwork;k++) {
                        tmpDivPos.Add(new Vector2Int(i,j));
                    }
                }
            }
            break;

            case "B":
            for(int i = 0;i < ValueDefinition.FIELD_DIVIDE_NUM_Y;i++) {
                for(int j = 0;j < ValueDefinition.FIELD_DIVIDE_NUM_X;j++) {
                    int itemNum = Random.Range(1,3);
                    for(int k = 0;k < itemNum;k++) {
                        tmpDivPos.Add(new Vector2Int(i,j));
                    }
                }
            }
            break;

            case "C":
            for(int i = 0;i < ValueDefinition.FIELD_DIVIDE_NUM_Y;i++) {
                for(int j = 0;j < ValueDefinition.FIELD_DIVIDE_NUM_X;j++) {
                    int itemNum = Random.Range(0,2);
                    for(int k = 0;k < itemNum;k++) {
                        tmpDivPos.Add(new Vector2Int(i,j));
                    }
                }
            }
            break;

            case "P":
            for(int i = 0;i < ValueDefinition.FIELD_DIVIDE_NUM_Y;i++) {
                for(int j = 0;j < ValueDefinition.FIELD_DIVIDE_NUM_X;j++) {
                    int itemNum = Random.Range(0,3);
                    for(int k = 0;k < itemNum;k++) {
                        tmpDivPos.Add(new Vector2Int(i,j));
                    }
                }
            }
            break;

            default:
            Debug.Log("モードエラー");
            break;
        }

        for(int i = 0;i < tmpDivPos.Count;i++) {
            do {
                //どの分割フィールドにいるかに応じて上下左右端を指定
                int tmpDivLeftSide = divideFieldLeftEnd[tmpDivPos[i].x,tmpDivPos[i].y];
                int tmpDivRightSide = divideFieldRightEnd[tmpDivPos[i].x,tmpDivPos[i].y];
                int tmpDivUpSide = divideFieldUpEnd[tmpDivPos[i].x,tmpDivPos[i].y];
                int tmpDivDownSide = divideFieldDownEnd[tmpDivPos[i].x,tmpDivPos[i].y];

                tmpPos = new Vector2Int(Random.Range(tmpDivLeftSide,tmpDivRightSide),Random.Range(tmpDivUpSide,tmpDivDownSide));

                //Debug.Log(i + ":" + tmpPos.x + "/" + tmpPos.y);
                yield return new WaitForEndOfFrame();
            } while(
           IsExistStairOnThisPos(tmpPos.x,tmpPos.y) ||
           IsExistPlayerOnThisPos(tmpPos.x,tmpPos.y) ||
           itemManager.IsExistItemOnThisPos(tmpPos.x,tmpPos.y) != -1
           ); //階段の位置が同じ、またはプレイヤーの位置が同じ、または他のアイテムの位置が同じなら再計算   

            itemManager.CreateItem(Random.Range(0,4),tmpPos.x,tmpPos.y);
            parameterManager.PlayingSkill--; //プレイングスキルに適用
        }

        yield break;
    }

    //=================================================================
    //特定の座標にプレイヤーがいるか
    private bool IsExistPlayerOnThisPos (int _x,int _y) {
        return (_x == FirstPlayerPos.x && _y == FirstPlayerPos.y);
    }

    //=================================================================
    //特定の座標に階段があるか
    private bool IsExistStairOnThisPos (int _x,int _y) {
        return (_x == stairPos.x && _y == stairPos.y);
    }

    //=================================================================
    //特定の座標に敵がいるか
    private bool IsExistEnemiesOnThisPos (int _x,int _y) {
        for(int i = 0;i < enemiesPos.Count;i++) {
            if(_x == enemiesPos[i].x && _y == enemiesPos[i].y) {
                return true;
            }
        }

        return false;
    }

    //=================================================================
    //壁かどうか
    public bool IsWall (int _x,int _y) {
        if(fieldData[_x,_y] == 1) {
            return true;
        }

        return false;
    }

    //=================================================================
    //フィールドを作成する
    private IEnumerator CreateField () {
        for(int i = 0;i < ValueDefinition.FIELD_SIZE_X;i++) {
            for(int j = 0;j < ValueDefinition.FIELD_SIZE_Y;j++) {
                CreateFieldChip(new Vector3(i * ValueDefinition.FIELD_CHIP_SIZE,j * ValueDefinition.FIELD_CHIP_SIZE,0),fieldData[i,j],i + ":" + j,new Vector2Int(i,j)); //フィールドチップを生成する
            }
        }

        manager.IsFieldCreated = true; //フィールド生成完了判定をtrueに→プレイヤーやその他オブジェクト、ギミックの位置を適用

        yield break;
    }

    //=================================================================
    //フィールドチップを作成する
    //pos -> フィールドチップの位置
    //type -> フィールドチップのタイプ(壁、地面など)
    //    0:地面
    //    1:壁
    //    2:階段
    //name -> オブジェクトの名前
    //index -> 番地

    private void CreateFieldChip (Vector3 pos,int type,string name,Vector2Int index) {
        GameObject obj = Instantiate(Resources.Load("Prefabs/FieldChip")) as GameObject; //オブジェクト生成
        ApplySpriteToField(type,obj,index); //スプライトのデータを適用する
        obj.transform.position = pos; //位置の更新
        obj.transform.SetParent(GameObject.Find("Manager/FieldManager").transform,false); //親オブジェクトの指定
        obj.name = name;
    }

    //=================================================================
    //フィールドにスプライトのデータを適用する
    // _type -> スプライトの指定
    // _obj -> スプライトを適用するオブジェクト
    // _index -> 番地

    private void ApplySpriteToField (int _type,GameObject _obj,Vector2Int _index) {
        //    0:空白
        //    1:地面
        //    1-16:壁
        //    17:階段

        int baseType = -1; //ベース
        //int sub1Type = -1; //サブ

        switch(_type) {
            //地面
            case 0:
            baseType = 1;
            break;

            //壁
            case 1:

            switch(CheckWallType(_index)) {
                case 0: //全部壁
                baseType = 14;
                break;

                case 1: //上が道
                baseType = 13;
                break;

                case 2: //下が道
                baseType = 17;
                break;

                case 3: //上、下が道
                baseType = 10;
                break;

                case 4: //左が道
                baseType = 5;
                break;

                case 5: //左、上が道
                baseType = 3;
                break;

                case 6: //左、下が道
                baseType = 7;
                break;

                case 7: //上、下、左が道
                baseType = 16;
                break;

                case 8: //右が道
                baseType = 9;
                break;

                case 9: //上、右が道
                baseType = 15;
                break;

                case 10: //下、右が道
                baseType = 11;
                break;

                case 11: //上、下、右が道
                baseType = 8;
                break;

                case 12: //左、右が道
                baseType = 6;
                break;

                case 13: //上、左、右が道
                baseType = 4;
                break;

                case 14: //下、左、右が道
                baseType = 12;
                break;

                case 15: //全部道
                baseType = 2;
                break;

                default:
                baseType = 0;
                break;
            }

            break;

            //階段
            case 2:
            baseType = 18;
            break;

            default:
            break;
        }
        _obj.transform.Find("Base").GetComponent<SpriteRenderer>().sprite = fieldChipSprites[baseType]; //スプライトの更新
        //_obj.transform.Find("Sub1").GetComponent<SpriteRenderer>().sprite = fieldChipSprites[sub1Type]; //スプライトの更新
    }

    //=================================================================
    //どういう種類の壁か(隣接するマスの状態を確かめる)
    // _index -> 番地

    //[return]
    //(0x1->上)
    //(0x2->下)
    //(0x4->左)
    //(0x8->右)

    //0 -> 全部壁
    //1 -> 上が道
    //2 -> 下が道
    //3 -> 上、下が道
    //4 -> 左が道
    //5 -> 左、上が道
    //6 -> 左、下が道
    //7 -> 上、下、左が道
    //8 -> 右が道
    //9 -> 上、右が道
    //10 -> 下、右が道
    //11 -> 上、下、右が道
    //12 -> 左、右が道
    //13 -> 上、左、右が道
    //14 -> 下、左、右が道
    //15 -> 全部道

    private int CheckWallType (Vector2Int _index) {
        int byteFlag = 0x0;

        //上
        if(_index.y + 1 < ValueDefinition.FIELD_SIZE_Y) {
            if(fieldData[_index.x,_index.y + 1] == 0) {
                byteFlag += 0x1;
            }
        }

        //下
        if(_index.y - 1 >= 0) {
            if(fieldData[_index.x,_index.y - 1] == 0) {
                byteFlag += 0x2;
            }
        }

        //左
        if(_index.x - 1 >= 0) {
            if(fieldData[_index.x - 1,_index.y] == 0) {
                byteFlag += 0x4;
            }
        }

        //右
        if(_index.x + 1 < ValueDefinition.FIELD_SIZE_X) {
            if(fieldData[_index.x + 1,_index.y] == 0) {
                byteFlag += 0x8;
            }
        }

        return byteFlag;
    }

    //=============================================================
    //部屋にいるかどうか
    //_x,_y -> 座標
    public bool IsRoom (int _x,int _y) {
        if(_x >= 0 && _y >= 0 && _x <= ValueDefinition.FIELD_SIZE_X - 1 && _y <= ValueDefinition.FIELD_SIZE_Y - 1) {
            Vector2 tmp = fieldDataConcernDivide[_x,_y];
            if(tmp.x == -1 || tmp.y == -1) {
                return false;
            }

            //部屋内にいるなら
            if(tmp.x >= 1 && tmp.x <= ValueDefinition.FIELD_DIVIDE_NUM_X && tmp.y >= 1 && tmp.y <= ValueDefinition.FIELD_DIVIDE_NUM_Y) {
                return true;
            } else {
                return false;
            }
        }

        return false;
    }
}
