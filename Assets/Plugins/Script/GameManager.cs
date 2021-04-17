﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;
using UnityEngine.SceneManagement;


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

    [SerializeField] Text ScoreText; // 追加
    [SerializeField] Text ComboText; // 追加
    [SerializeField] Text TitleText; // 追加
    [SerializeField] Slider LessHpgage;
    [SerializeField] Slider MoreHpGage;
    [SerializeField] Slider DelayChange;


    AudioSource Music;

    float PlayTime;
    float Distance;
    float During;
    bool isPlaying;
    int GoIndex;
    bool donclap;
 
    float ComboCount; // 追加
    float Score; // 追加
    float ScoreFirstTerm; // 追加
    float ScoreTorerance; // 追加
    float ScoreCeilingPoint; // 追加
    int CheckTimingIndex; // 追加


    float CheckRange;
    float BeatRange;
    float Delay = 330;
    List<float> NoteTimings;
    GameObject donclick, kaclick;
    DonTouchClick dontouchclick;
    RkaTouchClick rkatouchclick;
    LkaTouchClick lkatouchclick;
    ScoreNum scorenum;

    string Title;
    int BPM;
    List<GameObject> Notes;

    Subject<string> SoundEffectSubject = new Subject<string>();

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

    private void Awake()
    {
        scorenum = ScoreNum.Instance;   
    }


    void OnEnable()
    {
        donclick = GameObject.Find("DonButtonClick");
        kaclick = GameObject.Find("KaButtonClick");
        dontouchclick = donclick.GetComponent<DonTouchClick>();
       rkatouchclick = kaclick.GetComponent<RkaTouchClick>();
        lkatouchclick = kaclick.GetComponent<LkaTouchClick>();
        Music = this.GetComponent<AudioSource>();

        Distance = Math.Abs(BeatPoint.position.x - SpawnPoint.position.x);
        During = 2 * 1000;
        isPlaying = false;
        GoIndex = 0;

        CheckRange = 120;
        BeatRange = 80;

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
          .Where(_ => Notes.Count > CheckTimingIndex)
          .Where(_ => NoteTimings[CheckTimingIndex] == -1)
          .Subscribe(_ => CheckTimingIndex++);

        // 追加
        this.UpdateAsObservable()
          .Where(_ => isPlaying)
          .Where(_ => Notes.Count > CheckTimingIndex)
          .Where(_ => NoteTimings[CheckTimingIndex] != -1)
          .Where(_ => NoteTimings[CheckTimingIndex] < ((Time.time * 1000 - PlayTime) - CheckRange / 2))
          .Subscribe(_ => {
              updateScore("failure");
              CheckTimingIndex++;
          });

        this.UpdateAsObservable()
          .Where(_ => isPlaying)
          .Where(_ => Notes.Count <= CheckTimingIndex)
          .Subscribe(_ => {
              scorenum.setScore(Score.ToString());
              scorenum.setHp(hp);
              SceneManager.LoadScene("ScoreSene");
          });

        this.UpdateAsObservable()
          .Where(_ => isPlaying)
          .Where(_ => dontouchclick.Clap)//touchclick.clap ||
          .Subscribe(_ => {
              beat("don", Time.time * 1000 - PlayTime);
              SoundEffectSubject.OnNext("don");
              dontouchclick.Clap = false;
          });

        this.UpdateAsObservable()
          .Where(_ => isPlaying)
          .Where(_ => rkatouchclick.Clap || lkatouchclick.Clap)
          .Subscribe(_ => {
              beat("ka", Time.time * 1000 - PlayTime);
              SoundEffectSubject.OnNext("ka");
              rkatouchclick.Clap = false;
              lkatouchclick.Clap = false;
          });
    }
    bool dontap = false;
    public bool hantei() {
        dontap = true;

        return dontap;
    }

    void loadChart()
    {
        Notes = new List<GameObject>();
        NoteTimings = new List<float>();

        string jsonText = Resources.Load<TextAsset>(FilePath).ToString();
        Music.clip = (AudioClip)Resources.Load(ClipPath);

        JsonNode json = JsonNode.Parse(jsonText);
        Title = json["title"].Get<string>();
        BPM = int.Parse(json["bpm"].Get<string>());

        foreach (var note in json["notes"])
        {
            string type = note["type"].Get<string>();
            float timing = float.Parse(note["timing"].Get<string>())+DelayChange.value;

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
            NoteTimings.Add(timing);
        }

    TitleText.text = Title;  // 追加

    // 追加
    if(Notes.Count< 10) {
      ScoreFirstTerm = (float) Math.Round(ScoreCeilingPoint/Notes.Count);
    ScoreTorerance = 0;
    } else if (10 <= Notes.Count && Notes.Count < 30)
{
    ScoreFirstTerm = 300;
    ScoreTorerance = (float)Math.Floor((ScoreCeilingPoint - ScoreFirstTerm * Notes.Count) / (Notes.Count - 9));
}
else if (30 <= Notes.Count && Notes.Count < 50)
{
    ScoreFirstTerm = 300;
    ScoreTorerance = (float)Math.Floor((ScoreCeilingPoint - ScoreFirstTerm * Notes.Count) / (2 * (Notes.Count - 19)));
}
else if (50 <= Notes.Count && Notes.Count < 100)
{
    ScoreFirstTerm = 300;
    ScoreTorerance = (float)Math.Floor((ScoreCeilingPoint - ScoreFirstTerm * Notes.Count) / (4 * (Notes.Count - 39)));
}
else
{
    ScoreFirstTerm = 300;
    ScoreTorerance = (float)Math.Floor((ScoreCeilingPoint - ScoreFirstTerm * Notes.Count) / (4 * (3 * Notes.Count - 232)));
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

    void beat(string type, float timing)
    {
        float minDiff = -1;
        int minDiffIndex = -1;

        for (int i = 0; i < Notes.Count; i++)
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

                MessageEffectSubject.OnNext("good"); // イベントを通知
                //Debug.Log("beat " + type + " success.");
                updateScore("good"); // 追加
            }
            else
            {
                NoteTimings[minDiffIndex] = -1;
                Notes[minDiffIndex].SetActive(false);

                MessageEffectSubject.OnNext("failure"); // イベントを通知
                //Debug.Log("beat " + type + " failure.");
                updateScore(" failure"); // 追加
            }
        }
        else
        {
            Debug.Log("through");
        }
    }

  float hp=0;

    void HpGageAdd(float n) {
        if ((hp + n >= 0) && (100 >= hp + n))
        {
            hp += n;
            if (hp >= 80) { MoreHpGage.value += n; }
            else { LessHpgage.value += n; }
        }
    }

    void updateScore(string result)
    {
        float HpUp;
        if (result == "good")
        {
            ComboCount++;

            float plusScore;
            if (ComboCount < 10)
            {
                plusScore = ScoreFirstTerm;
                HpUp = 0.5f;
            }
            else if (10 <= ComboCount && ComboCount < 30)
            {
                plusScore = ScoreFirstTerm + ScoreTorerance;
                HpUp = 0.7f;
            }
            else if (30 <= ComboCount && ComboCount < 50)
            {
                plusScore = ScoreFirstTerm + ScoreTorerance * 2;
                HpUp = 0.8f;

            }
            else if (50 <= ComboCount && ComboCount < 100)
            {
                plusScore = ScoreFirstTerm + ScoreTorerance * 4;
                HpUp = 0.9f;
            }
            else
            {
                plusScore = ScoreFirstTerm + ScoreTorerance * 8;
                HpUp = 1.0f;
            }
            Score += plusScore;
        }
        else if (result == "failure")
        {
            ComboCount = 0;
            HpUp = -1.0f;
        }
        else
        {
            ComboCount = 0; // default failure
            HpUp = -2.0f;
            Debug.Log("hantei");
        }

        HpGageAdd(HpUp);
        ComboText.text = ComboCount.ToString();
        ScoreText.text = Score.ToString();

    }
}
