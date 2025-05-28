
using UnityEngine;

public static class CameraUtils
{
    public static void SetCameraOnPlayer()
    {
        
        var cam = Camera.main;
        if (!cam) return;
        var players = GameObject.FindGameObjectsWithTag("Player");
        if (players == null) return;
        foreach (var player in players)
        {
            if (player.name != "LocalPlayer") continue;

            cam.transform.SetParent(player.transform);
            cam.transform.localPosition = new Vector3(0, 0, -11);
        }
    }
}