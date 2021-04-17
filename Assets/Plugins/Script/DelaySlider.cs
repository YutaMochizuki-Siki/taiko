using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class DelaySlider : MonoBehaviour
{
     Slider DelayChange;
    [SerializeField] Text DelayText;

    // Start is called before the first frame update
    void Start()
    {
      DelayChange= GetComponent<Slider>();
    }

    // Update is called once per frame
    void Update()
    {

        DelayText.text =DelayChange.value.ToString();

    }
}
