using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XAGame
{
	public class Bullet : MonoBehaviour
	{
		public Unit unit;
		public GameObject target;
		public int speed;

		void FixedUpdate()
		{
			if(unit!=null && target != null)
			{
				if(Vector3.Distance(transform.position, target.transform.position) < 0.7f)
				{
					unit.GetAttacked();
					Destroy(gameObject);
				}
				transform.position = Vector3.MoveTowards(transform.position,new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z), speed * Time.deltaTime);
			}
		}
	}
}

