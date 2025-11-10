using UnityEngine;

public class TorchDesync : MonoBehaviour
{
    void Start()
    {
        var anim = GetComponent<Animator>();
        if (anim != null)
        {
            anim.playbackTime = Random.Range(0f, 1f); // começa em frames diferentes
            anim.speed = Random.Range(0.9f, 1.2f);    // leve variação de velocidade
        }
    }
}
