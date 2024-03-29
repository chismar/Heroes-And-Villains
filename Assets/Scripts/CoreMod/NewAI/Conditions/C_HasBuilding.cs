﻿using UnityEngine;
using System.Collections;

namespace CoreMod
{
	public class C_HasBuilding : NewAI.Condition
	{
		public BuildingType BuildingType;
		public int Count;
		public City City;

		public override void Setup (GameObject target)
		{
			City = target.GetComponent<City> ();
			BuildingType = null;
			Count = -1;
		}

		public override NewAI.Task CreateTask (NewAI.Agent agent)
		{
			var task = new T_BuildBuilding ();
			task.Setup (City.gameObject.GetComponent<NewAI.Agent> (), this);
			return task;
		}

		public override GameObject TargetAgent {
			get
			{
				return this.City.gameObject;
			}
		}

		public override bool Satisfied {
			get
			{
				int count = 0;
				for (int i = 0; i < City.buildings.Count; i++)
					if (City.buildings [i].Type == BuildingType)
						count++;
				return count >= Count;
			}
		}
	}

}

