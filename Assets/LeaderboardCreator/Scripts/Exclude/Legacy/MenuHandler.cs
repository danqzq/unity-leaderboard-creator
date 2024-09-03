using UnityEngine;

namespace Dan.Legacy
{
    public class MenuHandler : MonoBehaviour
    {
        private int _currentMenuIndex;
        private static int _indexParameter = Animator.StringToHash("index");
        
        private Animator _anim;

        private void Start() => _anim = GetComponent<Animator>();

        public void Left() => _anim.SetInteger(_indexParameter, _currentMenuIndex > 0 ? --_currentMenuIndex : 0);
        
        public void Right() => _anim.SetInteger(_indexParameter, _currentMenuIndex < 2 ? ++_currentMenuIndex : 2);
    }
}