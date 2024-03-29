﻿using UnityEngine;
using System.Collections;
using UIO;

namespace CoreMod
{

	[ASlotComponent ("Nest")]
	[AShared]
	public class NestComponent : SlotComponent
	{

	}

	public class NestsSlotsModule : SlotsProcessor
	{
		public override void Work ()
		{
			OutputObjects = InputObjects;
			foreach (var inputObject in InputObjects)
				inputObject.AddComponent<NestComponent> ();
			FinishWork ();
		}

	}

}


