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
        
        // Параметры аниматора - точно как в Unity Animator
        private static readonly int IsMovingParam = Animator.StringToHash("isMoving");
        private static readonly int IsDrawingBowParam = Animator.StringToHash("isDrawningBow");
        private static readonly int ReleaseArrowParam = Animator.StringToHash("releaseArrow");
        
        private bool isDrawingBow = false;
    
        private void Start()
        {
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }
            
            // Проверяем параметры при старте
            Debug.Log("Checking animator parameters:");
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                Debug.Log($"Parameter: {param.name}, Type: {param.type}");
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
            Debug.Log($"PlayIdle: isMoving={animator.GetBool(IsMovingParam)}, isDrawingBow={animator.GetBool(IsDrawingBowParam)}");
        }
    
        public void StartDrawingBow()
        {
            isDrawingBow = true;
            animator.SetBool(IsDrawingBowParam, true);
            animator.SetBool(ReleaseArrowParam, false);
            Debug.Log($"StartDrawingBow: isDrawingBow={animator.GetBool(IsDrawingBowParam)}, releaseArrow={animator.GetBool(ReleaseArrowParam)}");
        }
        
        public void BowDrawn()
        {
            animator.SetBool(IsDrawingBowParam, true);
            Debug.Log($"BowDrawn: isDrawingBow={animator.GetBool(IsDrawingBowParam)}");
        }
    
        public void ReleaseArrow()
        {
            isDrawingBow = false;
            animator.SetBool(IsDrawingBowParam, false);
            animator.SetBool(ReleaseArrowParam, true);
            Debug.Log($"ReleaseArrow: isDrawingBow={animator.GetBool(IsDrawingBowParam)}, releaseArrow={animator.GetBool(ReleaseArrowParam)}");
        }
    
        public void PlayWalk()
        {
            animator.SetBool(IsMovingParam, true);
            if (!isDrawingBow)
            {
                animator.SetBool(IsDrawingBowParam, false);
                animator.SetBool(ReleaseArrowParam, false);
            }
            Debug.Log($"PlayWalk: isMoving={animator.GetBool(IsMovingParam)}, isDrawingBow={animator.GetBool(IsDrawingBowParam)}");
        }
    
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