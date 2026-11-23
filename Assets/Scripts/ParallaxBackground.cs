using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [Range(0f, 1f)]
    public float parallaxFactor = 0.5f; // 0.9 = æ∆¡÷ ∏’ «œ¥√, 0.4 = ¡ﬂ∞£ Ω£

    private Transform cam;
    private Vector3 lastCamPos;

    void Start()
    {
        cam = Camera.main.transform;
        lastCamPos = cam.position;
    }

    void LateUpdate()
    {
        Vector3 delta = cam.position - lastCamPos;
        transform.position += new Vector3(delta.x * parallaxFactor, delta.y * (parallaxFactor * 0.5f), 0f);
        lastCamPos = cam.position;
    }
}