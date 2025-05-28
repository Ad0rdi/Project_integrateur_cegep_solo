using UnityEngine;
using UnityEngine.UI;

public class ShowIp:MonoBehaviour
{
    [SerializeField] private Text ipTMP;

    void OnEnable()
    {
            ipTMP.text = Networking.NetworkFunction.GetServerIp();
    }
}