using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RkaTouchClick : MonoBehaviour
{
    bool clap = false;
    public bool Clap
    {
        set { this.clap = value; }
        get { return clap; }
    }
    // Start is called before the first frame update

    public void Ka_Click()
    {
        clap = true;
    }

    public void Ka_OffClick()
    {
        clap = false;
    }
}
