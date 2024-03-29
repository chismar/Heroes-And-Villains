using UnityEngine;
using System.Collections;
using Demiurg;
using System.Collections.Generic;
using System;
using Demiurg.Core;


namespace CoreMod
{
	public class SlotsReplacer : SlotsProcessor
	{
		IEnumerable<CreationNamespace> namespaces;

		public override void Work ()
		{
			//var tagsRoot = Find.Root<TagsRoot> ();
			var objectsRoot = Find.Root<ObjectsRoot> ();

			//var tags = tagsRoot.GetAllTags ();
			namespaces = objectsRoot.GetAllNamespaces ();


			OutputObjects = new List<GameObject> ();
			foreach (var slotGO in InputObjects)
			{
				Slot slot = slotGO.GetComponent<Slot> ();
				slot.Replacer = Replacement (slot);
				SlotComponent[] components = slotGO.GetComponents<SlotComponent> ();

				for (int i = 0; i < components.Length; i++)
					components [i].FillComponent (slot.Replacer);
				EntityComponent[] entComponents = slot.Replacer.GetComponents<EntityComponent> ();
				for (int i = 0; i < entComponents.Length; i++)
					entComponents [i].PostCreate ();
				
				slot.Replacer.SetActive (true);
				OutputObjects.Add (slot.Replacer);
			}
			FinishWork ();
		}

		GameObject Replacement (Slot slot)
		{
			int maxSimilarity = int.MinValue;
			List<ObjectCreationHandle> similarObjects = new List<ObjectCreationHandle> ();
			var plot = slot.gameObject.GetComponent<Plot> ();
			foreach (var space in namespaces)
			{
				
				IEnumerable<ObjectCreationHandle> available = null;
				if (plot == null)
					available = space.FindAvailable (slot.Tags);
				else
					available = space.FindAvailable (slot.Tags, plot.Size, plot.PlotType);
				int similarity = int.MinValue;
				int plotSize = plot == null ? 0 : plot.Size;
				var similar = space.FindSimilar (slot.Tags, out similarity, available, plotSize);
				if (similarity > maxSimilarity)
				{
					maxSimilarity = similarity;
					similarObjects.Clear ();
					similarObjects.AddRange (similar);
				} else if (similarity == maxSimilarity)
					similarObjects.AddRange (similar);
			}
			if (similarObjects.Count == 0)
				return new GameObject ("Broken replacement");
			else
			{
				ObjectCreationHandle handle = similarObjects [Random.Next () % similarObjects.Count];
				GameObject go = handle.CreateObject (slot.Tags);
				return go;
			}
		}
	}
}


