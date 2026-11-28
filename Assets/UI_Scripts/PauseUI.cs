using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // 씬 이동(타이틀로 돌아가기) 기능을 쓰기 위해 필수!

public class PauseUI : UIBase
{
    [Header("팝업창 내부 버튼들")]
    [SerializeField] private Button resumeButton; // '계속하기' 버튼
    [SerializeField] private Button titleButton;  // '타이틀로 나가기' 버튼

    private void Awake()
    {
        // '계속하기' 버튼을 누르면 나 자신(PauseUI)을 닫는 Close() 함수를 실행합니다.
        resumeButton.onClick.AddListener(Close);

        // '타이틀로' 버튼을 누르면 OnClickTitle() 함수를 실행합니다.
        titleButton.onClick.AddListener(OnClickTitle);
    }

    // 부모(UIBase)가 가진 Open() 기능을 물려받되, 기능을 추가(override)합니다.
    public override void Open()
    {
        base.Open();         // 1. 부모의 원래 기능 실행 (화면에서 이 팝업창을 켬)
        Time.timeScale = 0f; // 2. 게임 내 시간의 흐름을 0으로 만들어 일시정지 시킴! (캐릭터, 몬스터 멈춤)
    }

    // 부모(UIBase)가 가진 Close() 기능을 물려받되, 기능을 추가(override)합니다.
    public override void Close()
    {
        Time.timeScale = 1f; // 1. 게임 내 시간의 흐름을 다시 정상 속도(1)로 되돌림!
        base.Close();        // 2. 부모의 원래 기능 실행 (화면에서 이 팝업창을 끔)
    }

    // '타이틀로 나가기' 버튼을 눌렀을 때 실행될 함수
    private void OnClickTitle()
    {
        // ★실무 필수 주의사항★
        // 씬을 이동하기 전에 반드시 시간을 1로 되돌려야 합니다! 
        // 안 그러면 타이틀 화면으로 넘어가서도 게임 시간이 멈춰있는 버그가 생깁니다.
        Time.timeScale = 1f;

        // 타이틀 씬으로 화면을 이동합니다.
        SceneManager.LoadScene("TitleScene");
    }
}
