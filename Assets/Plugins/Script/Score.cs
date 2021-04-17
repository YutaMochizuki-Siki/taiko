using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Score : MonoBehaviour
{
    [SerializeField] Slider LessHpGage;
    [SerializeField] Slider MoreHpGage;

    [SerializeField] Text HanteiText;
    [SerializeField] Text ScoreText;
    ScoreNum scorenum;

    // Start is called before the first frame update
    void Start()
    {
        scorenum = ScoreNum.Instance;
        ScoreText.text = scorenum.getScore();
        HpParamete(scorenum.getHp());
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void button_click() {
        SceneManager.LoadScene("SampleScene");
    }

    void HpParamete(float hp) {
        if (hp >= 80) {
            LessHpGage.value = 80;
            MoreHpGage.value = hp - 80;
            HanteiText.text = "çáäi";
        }
        else { 
            LessHpGage.value = hp;
            HanteiText.text = "ïsçáäi";
        }

    }
}
