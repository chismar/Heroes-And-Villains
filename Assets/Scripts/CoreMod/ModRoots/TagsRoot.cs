﻿using UnityEngine;
using System.Collections;
using UIO;
using System.Collections.Generic;
using System.Linq;

namespace CoreMod
{
	[RootDependencies (typeof(ModsManager))]
	public class TagsRoot : ModRoot
	{
		Scribe scribe;
		Dictionary<string, Dictionary<string, Tag>> tags;
		static int id = 0;

		protected override void CustomSetup ()
		{
			
			var modsManager = Find.Root<ModsManager> ();
			ITable tagsTable = modsManager.GetTable ("tags");
			tags = new Dictionary<string, Dictionary<string, Tag>> ();
			foreach (var key in tagsTable.GetKeys())
			{
				if (modsManager.IsTechnical (tagsTable, key))
					continue;
				ITable namespaceTable = tagsTable.GetTable (key);

				string strKey = key as string;
				if (strKey == null)
					continue;
				tags.Add (strKey, GetTags (namespaceTable));

			}
			Fulfill.Dispatch ();
		}

		protected override void PreSetup ()
		{
			scribe = Scribes.Find ("Tags root");
		}

		Dictionary<string, Tag> GetTags (ITable table)
		{
			Dictionary<string, Tag> tags = new Dictionary<string, Tag> ();
			foreach (var key in table.GetKeys())
			{
				ITable tagTable = table.GetTable (key) as ITable;
				if (tagTable == null)
					continue;
				Tag tag = new Tag (key as string, id++, tagTable.GetCallback ("expression"), tagTable.GetTable ("criteria"));
				tags.Add (tag.Name, tag);

			}

			return tags;
		}

		public IEnumerable<Tag> GetAllTags ()
		{
			List<Tag> tagsList = new List<Tag> ();
			foreach (var tagsDict in tags)
				foreach (var tagPair in tagsDict.Value)
					tagsList.Add (tagPair.Value);
			return tagsList;
		}

		public Dictionary<string, Tag> GetTags (string tagsNamespace)
		{
			if (tags.ContainsKey (tagsNamespace))
				return tags [tagsNamespace];
			else
			{
				scribe.LogWarning ("Can't find tags namespace: " + tagsNamespace);
				return null;
				//new Dictionary<string, Tag> ();
			}
		}

		public Tag GetTag (string name, Dictionary<string, Tag> dict)
		{
			Tag tag = null;
			dict.TryGetValue (name, out tag);
			return tag;
		}

		public Tag CreateNewTag (string tagsNamespace, string name)
		{
			Tag tag = new Tag (name, id++, null, null);
			if (!tags.ContainsKey (tagsNamespace))
				tags [tagsNamespace] = new Dictionary<string, Tag> ();
			tags [tagsNamespace].Add (name, tag);
			return tag;
		}

		public Dictionary<string, Tag> CreateNamespace (string name)
		{
			if (!tags.ContainsKey (name))
			{
				var names = new Dictionary<string, Tag> ();
				tags.Add (name, names);
				return names;
			} else
				return tags [name];
		}
	}

}

