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
    [SerializeField] string ClipPath; //　追加

    [SerializeField] Button Play;
    [SerializeField] Button SetChart;

    [SerializeField] GameObject Don;
    [SerializeField] GameObject Ka;

    [SerializeField] Transform SpawnPoint;
    [SerializeField] Transform BeatPoint;

    AudioSource Music; // 追加

    float PlayTime;
    float Distance;
    float During;
    bool isPlaying;
    int GoIndex;

    string Title;
    int BPM;
    List<GameObject> Notes;

    void OnEnable()
    {
        Music = this.GetComponent<AudioSource>(); // 追加

        Distance = Math.Abs(BeatPoint.position.x - SpawnPoint.position.x);
        During = 2 * 1000;
        isPlaying = false;
        GoIndex = 0;

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
    }

    void loadChart()
    {
        Notes = new List<GameObject>();

        string jsonText = Resources.Load<TextAsset>(FilePath).ToString();
        Music.clip = (AudioClip)Resources.Load(ClipPath); // 追加

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
        }
    }

    void play()
    {
        Music.Stop(); // 追加
        Music.Play(); // 追加
        PlayTime = Time.time * 1000;
        isPlaying = true;
        Debug.Log("Game Start!");
    }
}