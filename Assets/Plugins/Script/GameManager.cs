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

    float PlayTime;
    float Distance;
    float During;
    bool isPlaying;
    int GoIndex;

    float CheckRange;
    float BeatRange;
    List<float> NoteTimings; // 追加

    AudioSource Music;

    string Title;
    int BPM;
    List<GameObject> Notes;

    Subject<string> SoundEffectSubject = new Subject<string>();

    // イベントを検知するオブザーバーを追加
    public IObservable<string> OnSoundEffect
    {
        get { return SoundEffectSubject; }
    }

    // イベントを通知するサブジェクトを追加
    Subject<string> MessageEffectSubject = new Subject<string>();

    // イベントを検知するオブザーバーを追加
    public IObservable<string> OnMessageEffect
    {
        get { return MessageEffectSubject; }
    }


    void OnEnable()
    {
        Music = this.GetComponent<AudioSource>();

        Distance = Math.Abs(BeatPoint.position.x - SpawnPoint.position.x);
        During = 2 * 1000;
        isPlaying = false;
        GoIndex = 0;

        CheckRange = 120; // 追加
        BeatRange = 80; // 追加

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
          .Subscribe(_ => {
              Notes[GoIndex].GetComponent<NoteController>().go(Distance, During);
              GoIndex++;
          });

        // 追加
        this.UpdateAsObservable()
          .Where(_ => isPlaying)
          .Where(_ => Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.J))
          .Subscribe(_ => {
              beat("don", Time.time * 1000 - PlayTime);
              SoundEffectSubject.OnNext("don"); // イベントを通知

          });

        // 追加
        this.UpdateAsObservable()
          .Where(_ => isPlaying)
          .Where(_ => Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.K))
          .Subscribe(_ => {
              beat("ka", Time.time * 1000 - PlayTime);
              SoundEffectSubject.OnNext("ka"); // イベントを通知
          });
    }

    void loadChart()
    {
        Notes = new List<GameObject>();
        NoteTimings = new List<float>(); // 追加

        string jsonText = Resources.Load<TextAsset>(FilePath).ToString();
        Music.clip = (AudioClip)Resources.Load(ClipPath);

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

            Note.GetComponent<NoteController>().setParameter(type, timing);

            Notes.Add(Note);
            NoteTimings.Add(timing); // 追加
        }
    }

    void play()
    {
        Music.Stop();
        Music.Play();
        PlayTime = Time.time * 1000;
        isPlaying = true;
        Debug.Log("Game Start!");
    }

    // 追加
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
                MessageEffectSubject.OnNext("good"); // イベントを通知
            }
            else
            {
                NoteTimings[minDiffIndex] = -1;
                Notes[minDiffIndex].SetActive(false);
                Debug.Log("beat " + type + " failure.");
                MessageEffectSubject.OnNext("failure"); // イベントを通知
            }
        }
        else
        {
            Debug.Log("through");
        }
    }
}