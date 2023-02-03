using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace _Project.UI.Scripts.Tooltips
{
    /// <summary>
    /// When attached to a UI element this class will show a tooltip when the mouse hovers over it.
    /// </summary>
    public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Serializable]
        public class TooltipShown : UnityEvent { }
        public TooltipShown onTooltipShown;
        /// <summary>
        /// The content of the tooltip.
        /// </summary>
        public string Content;

        private static IEnumerator delayCoroutine;
        private TooltipManager tooltipManager;

        public void OnPointerEnter(PointerEventData eventData)
        {
            delayCoroutine = DelayedShow(0.5f); // Wait a bit before showing the tooltip.
            StartCoroutine(delayCoroutine);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            StopCoroutine(delayCoroutine); // Cancel showing the tooltip.
            tooltipManager.Hide();
        }

        private void Start()
        {
            tooltipManager = TooltipManager.Get();
        }

        private IEnumerator DelayedShow(float delay)
        {
            yield return new WaitForSeconds(delay);
            tooltipManager.Show(Content);
            onTooltipShown?.Invoke();
        }
    }
}
