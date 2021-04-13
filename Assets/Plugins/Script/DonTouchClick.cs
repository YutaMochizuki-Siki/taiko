using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public  class DonTouchClick : MonoBehaviour
{
    bool clap = false;
   public  bool Clap{
        get { return clap; }
    }
    // Start is called before the first frame update

    public  void Don_Click() {
        clap = true;
    }

   public  void Don_OffClick() {
        clap = false;
    }
}
