using UnityEngine;

// abstract(추상클래스): 이 스크립트 단독으로는 오브젝트에 붙일 수 없고,
// 반드시 다른 UI 스크립트가 물려받아서(상속) 쓰도록 강제하는 키워드
public abstract class UIBase : MonoBehaviour
{
    // virtual(가상 함수): 자식 클래스에서 필요하면 내용을 고쳐 쓸 수 있게 열어둡니다
    public virtual void Open()
    {
        gameObject.SetActive(true); // 이 UI 오브젝트를 화면에 킵니다.
    }
    
    public virtual void Close()
    {
        gameObject.SetActive(false); // 이 UI 오브젝트를 화면에서 끕니다.
    }

}
