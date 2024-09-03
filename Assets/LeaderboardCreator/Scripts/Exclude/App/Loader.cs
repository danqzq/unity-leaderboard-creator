using System.Collections;
using UnityEngine;

namespace Dan.App
{
    public class Loader : MonoBehaviour
    {
        public bool IsActive => _canvasGroup.alpha > 0;
        
        private CanvasGroup _canvasGroup;
        
        private void Start()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }
        
        public void Activate(float timeout = 30f)
        {
            _canvasGroup.alpha = 1;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            StartCoroutine(DeactivateAfter(timeout));
        }
        
        public void Deactivate()
        {
            _canvasGroup.alpha = 0;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }
        
        private IEnumerator DeactivateAfter(float timeout)
        {
            yield return new WaitForSeconds(timeout);
            Deactivate();
        }
    }
}