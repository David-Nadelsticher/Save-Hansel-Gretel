using UnityEngine;
using System.Collections;

public class csDestroyEffect : MonoBehaviour {

	void Update ()
    {
	    if(Input.GetKeyDown(KeyCode.F10) )
        {
            Destroy(gameObject);
        }
	}
}
