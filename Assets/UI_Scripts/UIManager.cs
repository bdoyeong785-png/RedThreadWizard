using System;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    // [싱글톤 패턴] 게임 전체에서 UIManager를 딱 1개만 존재하게 만들고,
    // 다른 스크립트에서 'UIManager.Instance'로 언제든 쉽게 부를 수 있게 합니다.
    public static UIManager Instance { get; private set; }

    // 모든 UI를 보관하는 사전(Dictionary)입니다. 
    // '타입(예: TitleUI)'을 열쇠(Key)로 넣으면, 실제 'UI 오브젝트'를 꺼내줍니다.
    private Dictionary<Type, UIBase> uiDictionary = new Dictionary<Type, UIBase>();

    private void Awake()
    {
        // 싱글톤 초기화: 내가 첫 번째 매니저라면 등록하고, 이미 있다면 중복 방지를 위해 삭제합니다.
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 내 자식 오브젝트로 들어있는 모든 UIBase(TitleUI, OptionUI 등)를 한 번에 찾아옵니다.
        // true를 넣으면 비활성화(꺼져) 있는 UI까지 모두 찾아옵니다.
        UIBase[] childUIs = GetComponentsInChildren<UIBase>(true);

        foreach (UIBase ui in childUIs)
        {
            // 사전에 UI의 타입(이름표)과 실제 UI를 등록합니다.
            uiDictionary.Add(ui.GetType(), ui);
        }
    }

    // 제네릭(<T>) 함수: 원하는 UI 타입을 넣으면 찾아서 열어줍니다.
    // 사용 예시: UIManager.Instance.OpenUI<TitleUI>();
    public T OpenUI<T>() where T : UIBase
    {
        Type type = typeof(T); // 요청한 UI의 타입(이름표) 확인

        if (uiDictionary.TryGetValue(type, out UIBase ui))
        {
            ui.Open(); // 찾았으면 엽니다.
            return ui as T;
        }

        Debug.LogWarning($"{type.Name} UI를 찾을 수 없습니다!");
        return null;
    }

    // 원하는 UI를 닫아주는 함수
    // 사용 예시: UIManager.Instance.CloseUI<TitleUI>();
    public void CloseUI<T>() where T : UIBase
    {
        Type type = typeof(T);

        if (uiDictionary.TryGetValue(type, out UIBase ui))
        {
            ui.Close(); // 찾았으면 닫습니다.
        }
    }
}

