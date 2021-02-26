using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace XAGame
{
	public enum GameStage
	{
		decisionStage,
		moveStage,
		moveOrAttackStage
	}

	public class TimerManager : MonoBehaviour
	{
		public int presentRound { get; private set; }

		public int lastTime { get; private set; }

		public GameStage presentStage { get; private set; }

		public int decisionTime;

		public int moveTime;

		public int moveOrAttackTime;

		public List<Player> player;

		public Text timerText;
		public Text roundText;

		private float timer;

		private UIManager uiManager;
	
		void Start()
		{
			presentRound = 1;
			presentStage = GameStage.decisionStage;
			lastTime = decisionTime;
			timer = 0;
			uiManager= GameObject.Find("UIManager").GetComponent<UIManager>();
		}

		void FixedUpdate()
		{
			timer += Time.deltaTime;
			if (timer >= 1)
			{
				timer = 0;
				if (lastTime > 0)
				{
					lastTime -= 1;
					timerText.text = lastTime.ToString();
					if (presentStage == GameStage.moveOrAttackStage)
					{
						//提前结束执行回合
						bool flag = true;
						foreach (Player pl in player)
						{
							foreach (Unit u in pl.units)
							{
								if (u.HasPath())
									flag = false;
								if (u.isAttacking)
									flag = false;
							}
						}
						if (flag)
							NextRound();
					}
					//进入可进攻阶段
					if (presentStage == GameStage.moveStage)
					{
						if (lastTime <= moveOrAttackTime)
						{
							presentStage = GameStage.moveOrAttackStage;
						}
					}
				}
				else
				{
					if (presentStage == GameStage.decisionStage)
					{
						NextStage();
					}
					else
					{
						NextRound();
					}
				}
			}
		}

		public void NextStage()
		{
			if (presentStage == GameStage.decisionStage)
			{
				//打印决策信息
				foreach (Player pl in player)
				{
					pl.GetDecisionInform();
					pl.ClearAddGrids();
				}
				timer = 0;
				lastTime = moveTime + moveOrAttackTime;
				presentStage = GameStage.moveStage;
				timerText.text = lastTime.ToString();
				ExecutionStage();
			}
			else
			{
				uiManager.AlertText(AlertTextType.nextRoundAlert);
			}
		}

		private void NextRound()
		{
			//计算加钱和更新状态
			foreach (Player pl in player)
			{
				pl.NewRound();
			}

			presentRound++;
			lastTime = decisionTime;
			timerText.text = lastTime.ToString();
			presentStage = GameStage.decisionStage;
			roundText.text = "第" + presentRound.ToString() + "回合";
		}

		private void ExecutionStage()
		{
			foreach (Player pl in player)
			{
				pl.Execute();
			}
		}
	}
}

