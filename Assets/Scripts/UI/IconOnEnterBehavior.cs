using RoboRyanTron.Unite2017.Events;
using UnityEngine;

namespace UI
{
    public class IconOnEnterBehavior : StateMachineBehaviour
    {
        public GameEvent iconEntranceDone;
        
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo,
            int layerIndex)
        {
            iconEntranceDone.Raise();
        }
    }
}