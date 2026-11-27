using UnityEngine;
using UnityEngine.UI; // Button 같은 UI 컴포넌트를 쓰기 위해 필수!
using UnityEngine.SceneManagement; // 씬 이동 기능을 쓰기 위해 필수!

// MonoBehaviour 대신 우리가 만든 'UIBase'를 상속받습니다.
public class TitleUI : UIBase
{
    // [SerializeField]: private 변수지만 유니티 인스펙터 창에 노출시켜 버튼을 드래그해 넣을 수 있게 합니다.
    [SerializeField] private Button startButton;
    [SerializeField] private string mainSceneName = "SampleScene"; // 이동할 씬 이름

    private void Awake()
    {
        // 인스펙터에서 OnClick 이벤트를 수동으로 연결하지 않고, 코드로 자동 연결합니다.
        // (협업할 때 버튼 연결이 풀리는 대형 사고를 막아줍니다!)
        startButton.onClick.AddListener(OnClickStartButton);
    }

    // 시작 버튼을 눌렀을 때 실행될 함수
    private void OnClickStartButton()
    {
        Debug.Log("메인 씬으로 이동합니다!");
        SceneManager.LoadScene(mainSceneName); // 설정한 이름의 씬을 불러옵니다.
    }
}
