using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour, IPointerDownHandler, IEndDragHandler, IBeginDragHandler
{
    public static bool isdragscroller { get; set; }
    private CanvasGroup canvasgroup = null;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (canvasgroup == null)
            return;
        canvasgroup.alpha = 0.9f;
        canvasgroup.blocksRaycasts = false;

    }
    public void OnEndDrag(PointerEventData eventData)
    {
        if (canvasgroup == null)
            return;
        canvasgroup.alpha = 1f;
        canvasgroup.blocksRaycasts = true;
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.pointerCurrentRaycast.gameObject != null)
        {
            if(eventData.pointerCurrentRaycast.gameObject.name.Equals("Viewport"))
                isdragscroller = true;
            else
                isdragscroller = false;

            if (eventData.pointerCurrentRaycast.gameObject.GetComponent<TowerData>() != null)
            {
                TowerDefence.TowerManager.isItemDrag = true;
                if(GameManager.total_coins < eventData.pointerCurrentRaycast.gameObject.GetComponent<TowerData>().towerprice)
                    GameManager.instance.StartCoroutine(GameManager.instance.ShowCustomMessage(Constant.str_nocoin_msg));
                else
                {
                    TowerDefence.TowerManager.instance.Tower = eventData.pointerCurrentRaycast.gameObject.GetComponent<TowerData>();
                    TowerDefence.TowerManager.instance.SetSelectedTower(eventData.position);
                    if(!UserData.GetTutorialState())
                    {
                        UserData.SetTutorialState(true);
                        UiManager.instance.tutorial_icon.SetActive(false);
                    }
                }
                SoundManager.instance.PlaySfx(SoundManager.instance.item_selcet_sfx, 0.2f);
            }
            if (TowerDefence.TowerManager.instance.Tower != null)
                canvasgroup = TowerDefence.TowerManager.instance.Tower.GetComponent<CanvasGroup>();
        }
    }
}
