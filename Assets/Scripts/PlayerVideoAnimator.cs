using UnityEngine;
using UnityEngine.Video;

public class PlayerVideoAnimator : MonoBehaviour
{
    [Header("영상 클립 연결")]
    public VideoClip idleClip;   // still (1)_1
    public VideoClip runClip;    // run_1_1_1
    public VideoClip actionClip; // jump_1_1_1_1

    private VideoPlayer videoPlayer;
    private Rigidbody2D playerRb;
    private VideoClip currentClip;
    private RenderTexture renderTexture;

    void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        playerRb = GetComponentInParent<Rigidbody2D>();

        // 1. 하얀색 깜빡임을 막아주는 전용 도화지(RenderTexture) 생성
        renderTexture = new RenderTexture(1024, 1024, 0);
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = renderTexture;
        videoPlayer.skipOnDrop = false;

        // 2. Quad 머티리얼에 도화지 연결
        MeshRenderer mr = GetComponent<MeshRenderer>();
        if (mr != null)
        {
            mr.material.mainTexture = renderTexture;
        }
    }

    void Update()
    {
        if (videoPlayer == null || playerRb == null) return;

        float moveInput = Input.GetAxisRaw("Horizontal");

        // 1. 점프 중일 때
        if (Mathf.Abs(playerRb.velocity.y) > 0.2f && actionClip != null)
        {
            PlayClip(actionClip);
        }
        // 2. A/D 키를 눌러 걷고 있을 때
        else if (Mathf.Abs(moveInput) > 0.01f && runClip != null)
        {
            PlayClip(runClip);
        }
        // 3. 가만히 서 있을 때
        else if (idleClip != null)
        {
            PlayClip(idleClip);
        }
    }

    void PlayClip(VideoClip clip)
    {
        if (currentClip == clip) return; // 이미 같은 영상이면 건드리지 않음!
        currentClip = clip;

        // Stop()을 쓰지 않고 클립만 바로 교체해서 하얀 화면 방지
        videoPlayer.clip = clip;
        videoPlayer.Play();
    }

    void OnDestroy()
    {
        if (renderTexture != null) renderTexture.Release();
    }
}