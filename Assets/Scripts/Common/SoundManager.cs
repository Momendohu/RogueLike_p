using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

/// <summary>
/// BGM、SEを管理
/// </summary>
public class SoundManager : MonoBehaviour {
    public bool IsDontDestroy;

    private List<AudioSource> audioList = new List<AudioSource>();

    private void Awake () {
        if(IsDontDestroy) {
            GameObject.DontDestroyOnLoad(this.gameObject);
        }
        EntryMusic();
    }

    //1===============================================================================
    //設定した子オブジェクトの数に応じて音データを登録する
    private void EntryMusic () {
        for(int i = 0;i < transform.childCount;i++) {
            AddMusic(transform.GetChild(i).name);
        }
    }

    //1===============================================================================
    //シーンに応じて条件分岐
    private bool CheckScene (string str) {
        if(SceneManager.GetActiveScene().name == str) {
            return true;
        } else {
            return false;
        }
    }

    //1===============================================================================
    //オーディオを鳴らす
    public void Trigger (int num,bool useLoop) {
        if(audioList[num].isPlaying == false && num < audioList.Count) {
            audioList[num].Play();
            audioList[num].loop = useLoop;
        }
    }

    //1===============================================================================
    //オーディオを鳴らす(独立して鳴らす)
    public void TriggerSE (int num) {
        if(num < audioList.Count) {
            //PlayOneShotは引数にAudioClip(音源)を必要とするため、子から直接参照する
            audioList[num].PlayOneShot(transform.Find(transform.GetChild(num).name).GetComponent<AudioSource>().clip);
        }
    }

    //1===============================================================================
    //オーディオを止める
    public void StopMusic (int num) {
        if(audioList[num].isPlaying && num < audioList.Count) {
            audioList[num].Stop();
        }
    }

    //1===============================================================================
    //オーディオのピッチを変更する
    public void SetPitch (float pitch,int num) {
        audioList[num].pitch = pitch;
    }

    //1===============================================================================
    //オーディオのボリュームを変更する
    public void SetVolume (float volume,int num) {
        audioList[num].volume = volume;
    }

    //1===============================================================================
    //オーディオの現在の再生時間を取得する
    public float GetTime (int num) {
        return audioList[num].time;
    }

    //1===============================================================================
    //オーディオの再生時間の長さを取得する
    public float GetTimeLength (int num) {
        return audioList[num].clip.length;
    }

    //1===============================================================================
    //再生しているかどうか
    public bool IsPlaying (int num) {
        return audioList[num].isPlaying;
    }

    //2===============================================================================
    //データを参照して追加する
    public void AddMusic (string name) {
        audioList.Add(transform.Find(name).GetComponent<AudioSource>());
    }
}