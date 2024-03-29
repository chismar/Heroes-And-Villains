
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Demiurg.Core;
using System;
using System.Reflection;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;
using UIOBinding;
using UIO;

[RootDependencies (typeof(ModsManager), typeof(ObjectsCreator), typeof(Sprites), typeof(MapRoot.Map))]
public class PlanetsGenerator : Root
{
	Scribe scribe;
	LuaContext luaContext;
	ModsManager modsManager;
	ITable wiring;

	protected override void PreSetup ()
	{
		base.PreSetup ();
		scribe = Scribes.Find ("PlanetsGenerator");
	}

	protected override void CustomSetup ()
	{
		

		wiring = Find.Root<ModsManager> ().GetTable ("wiring");

		int seed = Find.Root<ModsManager> ().GetTable ("defines").GetInt ("SEED");
		DemiurgEntity dem = new DemiurgEntity (FindAvatarTypes (), PrepareAvatarsTables (), seed);
		Fulfill.Dispatch ();
	}

	Dictionary<string, Type> FindAvatarTypes ()
	{
		Dictionary<string, Type> nodes = new Dictionary<string, Type> ();
		Assembly asm = Assembly.GetExecutingAssembly ();
		foreach (var type in asm.GetTypes())
		{
			if (type.IsSubclassOf (typeof(Demiurg.Core.Avatar)) && !type.IsAbstract && !type.IsGenericType)
				nodes.Add (type.FullName, type);
		}
		return nodes;
	}

	Dictionary<string, ITable> PrepareAvatarsTables ()
	{
		Dictionary<string, ITable> tables = new Dictionary<string, ITable> ();
		foreach (var avatarKey in wiring.GetKeys())
		{
			if ((string)avatarKey == "global")
				continue;
			ITable avatarTable = wiring.GetTable (avatarKey) as ITable;
			if (avatarTable != null)
			if (!avatarTable.Contains ("global"))
			{
				Debug.Log (avatarKey.GetType ());
				tables.Add ((string)avatarKey, avatarTable);
				scribe.LogFormat ("{0} has been added ", avatarKey);
			}
                
		}
		return tables;
	}


	/*
    Dictionary<string, Type> FindNodeTypes ()
    {
        
    }

    public Dictionary<string, Tag> FormTags (Table tagsTable)
    {
        Dictionary<string, Tag> tags = new Dictionary<string, Tag> ();
        foreach (var entry in tagsTable.Pairs)
        {
            if (entry.Value.Table ["expression"] == null)
                continue;
            Tag tag = new Tag (entry.Key.ToPrintString (), tags.Count, entry.Value.Table ["expression"] as Closure, entry.Value.Table ["criteria"] as Table);
            Debug.LogWarning (entry.Key.ToPrintString ());
            tags.Add (entry.Key.ToPrintString (), tag);
        }
        return tags;
    }

    public Dictionary<string, GameObject> FormReplacers (Table replacersTable)
    {
        Dictionary<string, GameObject> gos = new Dictionary<string, GameObject> ();
        foreach (var entry in replacersTable.Pairs)
        {
            GameObject go = new GameObject ("prototype replacer " + entry.Key.ToPrintString ());
            go.SetActive (false);
            foreach (var compEntry in entry.Value.Table.Pairs)
            {
                string compName = compEntry.Key.ToPrintString ();
                if (compName == "graphics")
                    go.AddComponent<CoreMod.EntityGraphics> ().LoadFromTable (entry.Value.Table);
                else
                if (compName == "settlement")
                    go.AddComponent<CoreMod.Settlement> ().LoadFromTable (entry.Value.Table);
            }
            gos.Add (entry.Key.ToPrintString (), go);
        }
        return gos;
    }*/

	void RegisterSlotComponents (BindingTable table)
	{
		/*script.Globals ["component"] = new Table (script);
        Table table = script.Globals ["component"] as Table;
        Type[] types = Assembly.GetExecutingAssembly ().GetTypes ();
        List<Type> slotComponents = new List<Type> ();
        Type slotComponentType = typeof(SlotComponent);
        UserData.RegisterType (typeof(SlotComponentsProvider));
        for (int i = 0; i < types.Length; i++)
            if (types [i].IsSubclassOf (slotComponentType))
            {
                UserData.RegisterType (types [i]);
                table [types [i].Name] = slotComponents.Count;
                slotComponents.Add (types [i]);
            }
                
        SlotComponentsProvider.Types = slotComponents;*/

	}
}




