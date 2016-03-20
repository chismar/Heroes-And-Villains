﻿using UnityEngine;
using System.Collections;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace AI
{
	[RootDependencies (typeof(ModsManager))]
	public class AIRoot : Root
	{
		Dictionary<Type, ActionsPool> pools = new Dictionary<Type, ActionsPool> ();
		ActionGraph fullActionsGraph;

		protected override void CustomSetup ()
		{
			var allTypes = Find.Root<ModsManager> ().GetAllTypes ();
			var actionsTypes = from type in allTypes
			                   where type.IsSubclassOf (typeof(Action)) && !type.IsGenericType && !type.IsAbstract
			                   select type;

			foreach (var actionType in actionsTypes)
			{
				var newPool = new ActionsPool (actionType);
				pools.Add (actionType, newPool);
				var poolField = actionType.BaseType.GetField ("pool", BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.NonPublic);
				poolField.SetValue (null, newPool);
			}

			fullActionsGraph = new ActionGraph (actionsTypes, pools);
			Fulfill.Dispatch ();
		}

		public Action GetAction (Type type)
		{
			return pools [type].GetFreeAction ();
		}

		public void ReleaseAction (Action action)
		{
			pools [action.GetType ()].ReturnAction (action);
		}

		public Dictionary<Type, List<ActionsPool>> GetActionsForPrefab (GameObject go)
		{
			return new Dictionary<Type, List<ActionsPool>> ();
		}

	}

	public class ActionsPool
	{
		Stack<Action> actions = new Stack<Action> ();
		Type t;

		public ActionsPool (Type t)
		{
			this.t = t;
		}

		public Action GetFreeAction ()
		{
			if (actions.Count == 0)
				return Activator.CreateInstance (t) as Action;
			return actions.Pop ();
		}

		public void ReturnAction (Action action)
		{
			actions.Push (action);
		}
	}

}


