using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;

public class CameraClip : MonoBehaviour
{
    private bool attached = false;
    private Renderer target;
    private Transform player;

    private void OnTriggerEnter(Collider c)
    {
        if (attached || !c.CompareTag("CamClip")) return;
        
        CameraClip cc = c.gameObject.AddComponent<CameraClip>();
        cc.target = c.gameObject.GetComponent<Renderer>();
        cc.attached = true;
    }

    public void ConnectPlayer(Transform p)
    {
        player = p;
    }

    private void Start()
    {
        if (!attached) return;
        target.enabled = false;
    }

    private void Update()
    {
        if (attached || !player) return;
        float distance = (Vector3.Distance(transform.parent.position, player.position) - 1.5f) / 2;
        transform.localScale = new Vector3(1f, distance, 1f);
        transform.localPosition = new Vector3(0, 0, distance);
    }

    private void OnTriggerExit(Collider c)
    {
        if (attached) return;

        Destroy(c.gameObject.GetComponent<CameraClip>());
    }

    private void OnDestroy()
    {
        if (attached)
        {
            target.enabled = true;
        }
    }
}
