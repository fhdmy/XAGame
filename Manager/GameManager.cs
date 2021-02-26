using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XAGame
{
	public class GameManager : MonoBehaviour
	{
		public TimerManager timerManager;

		public UIManager uiManager;

		public string GetPresentTime()
		{
			int hour = System.DateTime.Now.Hour;
			int minute = System.DateTime.Now.Minute;
			int second = System.DateTime.Now.Second;
			return string.Format("{0:D2}:{1:D2}:{2:D2}", hour, minute, second);
		}
	}
}

