using UnityEngine;
using System.Collections;

public class ospray_roter : MonoBehaviour {

    public float m_speed = 10;

	// Update is called once per frame
	void Update () {
        transform.RotateAroundLocal(Vector3.up,Time.deltaTime * m_speed);

        
	}
}
