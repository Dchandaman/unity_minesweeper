using UnityEngine;
using UnityEngine.Events;

public class NewGameStateMachineBehaviour : StateMachineBehaviour
{
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		MinesweeperController controller = animator.gameObject.GetComponent<MinesweeperController>();
		controller.NewGame();
	}
}