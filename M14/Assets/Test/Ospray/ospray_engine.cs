using UnityEngine;
using System.Collections;

public class ospray_engine : MonoBehaviour {

    public float m_angle= 10;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	    transform.localEulerAngles = Vector3.right * m_angle;
	}
}
