using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUD_UI : UIBase
{
    [Header("체력 / 마나 게이지 바 연결")]
    [SerializeField] private Slider hpSlider;           // 체력바 UI
    [SerializeField] private Slider mpSlider;           // 마나바 UI

    [Header("점수 및 일시정지 버튼 연결")]
    [SerializeField] private TextMeshProUGUI scoreText; // 점수 글자
    [SerializeField] private Button pauseButton;        // 일시정지 버튼

    private void Awake()
    {
        // 일시정지 버튼을 누르면 UIManager를 통해 PauseUI 팝업을 엽니다.
        pauseButton.onClick.AddListener(() =>
        {
            UIManager.Instance.OpenUI<PauseUI>();
        });
    }

    // PlayerStats에서 체력이 바뀔 때마다 불러줄 함수
    public void UpdateHP(float currentHp, float maxHp)
    {
        if (maxHp <= 0) return;
        hpSlider.value = currentHp / maxHp;
    }

    // PlayerStats에서 마나가 바뀔 때마다 불러줄 함수
    public void UpdateMP(float currentMp, float maxMp)
    {
        if (maxMp <= 0) return;
        mpSlider.value = currentMp / maxMp;
    }

    // 점수가 바뀔 때마다 불러줄 함수
    public void UpdateScore(int currentScore)
    {
        scoreText.text = $"Score: {currentScore}";
    }
}
