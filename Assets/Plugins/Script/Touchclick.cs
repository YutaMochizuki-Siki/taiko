using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public  class Touchclick : MonoBehaviour
{
   public  bool clap{
        get { return clap; }
        set { clap = false; }
    }
    // Start is called before the first frame update

    public  void Don_Click() {
        clap = true;
    }

   public  void Don_OffClick() {
        clap = false;
    }
}
