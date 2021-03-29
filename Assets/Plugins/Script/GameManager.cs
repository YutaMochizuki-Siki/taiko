using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;

public class GameManager : MonoBehaviour
{

    [SerializeField] string FilePath;

    [SerializeField] string ClipPath;

    [SerializeField] Button Play;
    [SerializeField] Button SetChart;

    [SerializeField] GameObject Don;
    [SerializeField] GameObject Ka;

    [SerializeField] Transform SpawnPoint;
    [SerializeField] Transform BeatPoint;

    AudioSource Music;

    //　ノーツを動かすために必要になる変数を追加
    float PlayTime;
    float Distance;
    float During;
    bool isPlaying;
    int GoIndex;

    string Title;
    int BPM;

    float CheckRange;
    float BeatRange;
    List<float> NoteTimings; // 追加
    List<GameObject> Notes;

    void OnEnable()
    {
        Music = this.GetComponent<AudioSource>(); // 追加

        // 追加した変数に値をセット
        Distance = Math.Abs(BeatPoint.position.x - SpawnPoint.position.x);
        During = 2 * 1000;
        isPlaying = false;
        GoIndex = 0;

        CheckRange = 120; // 追加
        BeatRange = 80; // 追加

        Debug.Log(Distance);

        Play.onClick
          .AsObservable()
          .Subscribe(_ => play());

        SetChart.onClick
          .AsObservable()
          .Subscribe(_ => loadChart());

        this.UpdateAsObservable()
    .Where(_ => isPlaying)
    .Where(_ => Notes.Count > GoIndex)
    .Where(_ => Notes[GoIndex].GetComponent<NoteController>().getTiming() <= ((Time.time * 1000 - PlayTime) + During))
    .Subscribe(_ =>
    {
        Notes[GoIndex].GetComponent<NoteController>().go(Distance, During);
        GoIndex++;
    });

        this.UpdateAsObservable()
    .Where(_ => isPlaying)
    .Where(_ => Input.GetKeyDown(KeyCode.D))
    .Subscribe(_ => {
        beat("don", Time.time * 1000 - PlayTime);
    });

        // 追加
        this.UpdateAsObservable()
          .Where(_ => isPlaying)
          .Where(_ => Input.GetKeyDown(KeyCode.K))
          .Subscribe(_ =>
          {
              beat("ka", Time.time * 1000 - PlayTime);
          });

        // 追加
        /*

        // ノーツを発射するタイミングかチェックし、go関数を発火
        this.UpdateAsObservable()
          .Where(_ => isPlaying)
          .Where(_ => Notes.Count > GoIndex)
          .Where(_ => Notes[GoIndex].GetComponent<NoteController>().getTiming() <= ((Time.time * 1000 - PlayTime) + During))
          .Subscribe(_ =>
          {
              Notes[GoIndex].GetComponent<NoteController>().go(Distance, During);
              GoIndex++;
          });*/
    }
    

    void loadChart()
    {
        Notes = new List<GameObject>();
        NoteTimings = new List<float>(); // 追加
        Music.clip = (AudioClip)Resources.Load(ClipPath);

        string jsonText = Resources.Load<TextAsset>(FilePath).ToString();

        JsonNode json = JsonNode.Parse(jsonText);
        Title = json["title"].Get<string>();
        BPM = int.Parse(json["bpm"].Get<string>());

        foreach (var note in json["notes"])
        {
            string type = note["type"].Get<string>();
            float timing = float.Parse(note["timing"].Get<string>());

            GameObject Note;
            if (type == "don")
            {
                Note = Instantiate(Don, SpawnPoint.position, Quaternion.identity);
            }
            else if (type == "ka")
            {
                Note = Instantiate(Ka, SpawnPoint.position, Quaternion.identity);
            }
            else
            {
                Note = Instantiate(Don, SpawnPoint.position, Quaternion.identity); // default don
            }

            // setParameter関数を発火
            Note.GetComponent<NoteController>().setParameter(type, timing);

            Notes.Add(Note);
            NoteTimings.Add(timing); // 追加
        }
    }

    // ゲーム開始時に追加した変数に値をセット
    void play()
    {
        Music.Stop(); // 追加
        Music.Play(); // 追加
        PlayTime = Time.time * 1000;
        isPlaying = true;
        Debug.Log("Game Start!");
    }
    void beat(string type, float timing)
    {
        float minDiff = -1;
        int minDiffIndex = -1;

        for (int i = 0; i < NoteTimings.Count; i++)
        {
            if (NoteTimings[i] > 0)
            {
                float diff = Math.Abs(NoteTimings[i] - timing);
                if (minDiff == -1 || minDiff > diff)
                {
                    minDiff = diff;
                    minDiffIndex = i;
                }
            }
        }

        if (minDiff != -1 & minDiff < CheckRange)
        {
            if (minDiff < BeatRange & Notes[minDiffIndex].GetComponent<NoteController>().getType() == type)
            {
                NoteTimings[minDiffIndex] = -1;
                Notes[minDiffIndex].SetActive(false);
                Debug.Log("beat " + type + " success.");
            }
            else
            {
                NoteTimings[minDiffIndex] = -1;
                Notes[minDiffIndex].SetActive(false);
                Debug.Log("beat " + type + " failure.");
            }
        }
        else
        {
            Debug.Log("through");
        }
    }
}
