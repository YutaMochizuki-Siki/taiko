
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreNum : MonoBehaviour
{

    private static ScoreNum mInstance;
    private string score_num;
    private float hp;

    public static ScoreNum Instance
    {
        get
        {
            if (mInstance == null)
            {
                GameObject obj = new GameObject("ScoreNum");
                mInstance = obj.AddComponent<ScoreNum>();
            }
            return mInstance;
        }
        set
        {

        }
    }

    void Awake()
    {
        if (mInstance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void setScore(string n)
    {
        this.score_num = n;
    }
    public string getScore()
    {
        return this.score_num;
    }

    public void setHp(float n)
    {
        this.hp = n;
    }
    public float getHp()
    {
        return this.hp;
    }

}