using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LongTap : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public static float time = 0;
    public static bool isDown = false;

    /**
    <summary>
        長押し処理
        return : なし
    </summary>
    */
    public void OnPointerDown(PointerEventData eventData)
    {
        isDown = true;
        time = 0f;
    }

    /**
    <summary>
        離したとき
        return : なし
    </summary>
    */
    public void OnPointerUp(PointerEventData eventData)
    {
        isDown = false;
    }
    /**
    <summary>
        カーソルが長押しのところから外れたとき
        return : なし
    </summary>
    */
    public void OnPointerExit(PointerEventData eventData)
    {
        if (isDown)
            isDown = false;
    }
}
