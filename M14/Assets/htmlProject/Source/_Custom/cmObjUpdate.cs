using UnityEngine;
using System.Collections;

public class cmObjUpdate : MonoBehaviour {
    public float m_X_Axis_speed  = 0;
    public float m_Y_Axis_speed  = 0;
    public float m_Z_Axis_speed  = 0;

    IEnumerator Start()
    {
        yield return null;
        var renders = GetComponentsInChildren<Renderer>();
        foreach(var r in renders) r.material.renderQueue = 4000;
    }

    void Update()
    {
        float xd = m_X_Axis_speed * Time.deltaTime;
        float yd = m_Y_Axis_speed * Time.deltaTime;
        float zd = m_Z_Axis_speed * Time.deltaTime;
        transform.eulerAngles = new Vector3(transform.eulerAngles.x + xd, transform.eulerAngles.y + yd , transform.eulerAngles.z + zd);
    }
}
