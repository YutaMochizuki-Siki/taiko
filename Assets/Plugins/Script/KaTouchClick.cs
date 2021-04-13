using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KaTouchClick : MonoBehaviour
{
    bool clap = false;
    public bool Clap
    {
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
