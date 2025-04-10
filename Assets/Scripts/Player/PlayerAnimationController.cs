using UnityEngine;

namespace Player
{
    public class PlayerAnimationController : MonoBehaviour
    {
        [SerializeField] private Animator animator;
    
        // Состояния анимации
        private static readonly int IdleState = Animator.StringToHash("Idle");
        private static readonly int WalkState = Animator.StringToHash("Walk");
        private static readonly int FireStartState = Animator.StringToHash("FireStart");
        private static readonly int FireIdleState = Animator.StringToHash("FireIdle");
        private static readonly int FireEndState = Animator.StringToHash("FireEnd");
        
        // Параметры аниматора
        private static readonly int IsMovingParam = Animator.StringToHash("isMoving");
        private static readonly int IsDrawingBowParam = Animator.StringToHash("isDrawingBow");
        private static readonly int ReleaseArrowParam = Animator.StringToHash("releaseArrow");
        
        private bool isDrawingBow = false;
    
        private void Start()
        {
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }
        }
    
        public void PlayIdle()
        {
            animator.SetBool(IsMovingParam, false);
            if (!isDrawingBow)
            {
                animator.SetBool(IsDrawingBowParam, false);
                animator.SetBool(ReleaseArrowParam, false);
            }
        }
    
        public void StartDrawingBow()
        {
            isDrawingBow = true;
            animator.SetBool(IsDrawingBowParam, true);
            animator.SetBool(ReleaseArrowParam, false);
        }
        
        public void BowDrawn()
        {
            // Вызывается после завершения анимации натягивания лука
            animator.SetBool(IsDrawingBowParam, true);
        }
    
        public void ReleaseArrow()
        {
            isDrawingBow = false;
            animator.SetBool(IsDrawingBowParam, false);
            animator.SetBool(ReleaseArrowParam, true);
        }
    
        public void PlayWalk()
        {
            animator.SetBool(IsMovingParam, true);
            if (!isDrawingBow)
            {
                animator.SetBool(IsDrawingBowParam, false);
                animator.SetBool(ReleaseArrowParam, false);
            }
        }
    
        // Получение текущего состояния анимации
        public bool IsPlayingAnimation(string animationName)
        {
            return animator.GetCurrentAnimatorStateInfo(0).IsName(animationName);
        }
        
        public bool IsBowDrawn()
        {
            return isDrawingBow;
        }
    }
} 