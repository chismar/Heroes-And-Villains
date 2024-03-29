
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;
using System.Reflection;
using System.Linq;
using UIO;
using UIOBinding;

public abstract class ModRoot : Root
{
	
}

public class ModsManager : Root
{
	public Definitions Defs { get; internal set; }

	public Converters Conv { get; internal set; }

	Script globalContext;
	List<ModDesc> mods;
	List<ModDesc> activeMods;
	Mod globalMod;
	Scribe scribe;
	List<Type> allTypes;
	Dictionary<string, Type> typesByName;

	public bool IsTechnical (ITable table, object key)
	{
		try
		{
			if ((string)key == "global")
				return true;
			if (table.GetTable (key).GetBool ("global") == true)
				return true;
			return false;
		} catch
		{
			return false;
		}
	}

	public ITable GetTable (string name)
	{
		ITable table = null;
		globalMod.Tables.TryGetValue (name, out table);
		if (table == null)
		{
			Table internalTable = new Table (globalContext);

			table = new BindingTable (internalTable, name);
			globalMod.Tables.Add (name, table);
		}
		return table;

	}

	public ITable GetGlobalTable ()
	{
		return new BindingTable (globalContext.Globals, "global_table");
	}

	public void SetTableAsGlobal (string name)
	{
		ITable table = GetTable (name);
		table.Set ("global", true);
		foreach (var tablePair in globalMod.Tables)
			tablePair.Value.Set (name, table);
	}

	public List<Type> GetAllTypes ()
	{
		return allTypes;
	}


	public Type GetType (string name)
	{
		Type type = null;
		typesByName.TryGetValue (name, out type);
		return type;
	}

	public Assembly GetModAssembly (string modName)
	{
		return globalMod.ModAssembly;
	}

	protected override void PreSetup ()
	{
		base.PreSetup ();
		globalContext = new Script ();
		scribe = Scribes.Find ("Mods manager");
		globalContext.Options.ScriptLoader = new FileSystemScriptLoader ();
		mods = SearchForMods ();
		foreach (var mod in mods)
			Debug.Log (mod.Name);
		activeMods = DetermineActiveMods ();
		foreach (var mod in activeMods)
			Debug.Log (mod.Name);
		activeMods = DetermineOrder ();
		foreach (var mod in activeMods)
			Debug.Log (mod.Name);
		globalMod = CreateGlobalMod ();
		foreach (var table in globalMod.Tables)
			Debug.Log (table.Key);

		SearchForModRoots ();

		allTypes = new List<Type> (globalMod.ModAssembly.GetTypes ());

		Conv = new Converters ();
		typesByName = new Dictionary<string, Type> ();
		foreach (var type in allTypes)
			typesByName.Add (type.FullName, type);
		Defs = new Definitions (Conv, typesByName);
		Conv.AttachDefinitions (Defs);
		Conv.ReceiveConverters (allTypes.Where (x => x.IsSubclassOf (typeof(IConverter))));

		typesByName = new Dictionary<string, Type> ();
		foreach (var type in allTypes)
			typesByName.Add (type.FullName, type);
	}

	protected override void CustomSetup ()
	{
		Fulfill.Dispatch ();
	}

	void SearchForModRoots ()
	{
		Assembly assembly = Assembly.GetExecutingAssembly ();
		var modsRoots = from type in assembly.GetTypes ()
		                where type.IsSubclassOf (typeof(ModRoot))
		                select type;
		foreach (var rootType in modsRoots)
			Find.Register (rootType);
	}

	Mod CreateGlobalMod ()
	{
		Mod mod = new Mod ();
		foreach (var activeMod in activeMods)
		{
			activeMod.SharedLibraries = DetermineOrderOfLibraries (activeMod.SharedLibraries);
			foreach (var shared in activeMod.SharedLibraries)
			{
				ITable table = null;
				mod.Tables.TryGetValue (shared.Name, out table);
				if (table == null)
				{
					Table newTable = new Table (globalContext);
					globalContext.Globals [shared.Name] = newTable;
					newTable ["global"] = true;
					table = new BindingTable (newTable, shared.Name);
					mod.Tables.Add (shared.Name, table);

				}
				foreach (var dep in shared.Dependencies)
				{
					ITable depTable = mod.Tables [dep];
					Debug.LogFormat ("{0} for {1} in {2}", dep, shared.Name, activeMod.Name);
					table.Set (dep, depTable);
				}
				Table globalTable = (table as BindingTable).Table;
				string[] files = Directory.GetFiles ("Mods\\" + activeMod.Name + "\\" + shared.Directory);
				string namespaceFilePath = "Mods\\" + activeMod.Name + "\\" + shared.Directory + "\\" + "namespace.lua";

				//scribe.Log (namespaceFilePath);
				if (File.Exists (namespaceFilePath))
				{
					scribe.LogFormat ("Loading file {0} for a table {1}", namespaceFilePath, shared.Name);
					globalContext.DoFile (namespaceFilePath, globalTable);
				}
				foreach (var file in files)
				{
					if (file != namespaceFilePath)
					{
						scribe.LogFormat ("Loading file {0} for a table {1}", file, shared.Name);
						globalContext.DoFile (file, globalTable);
					}
				}
			}
		}
		mod.ModAssembly = Assembly.GetExecutingAssembly ();
		return mod;
	}

	List<ModDesc.SharedLibrary> DetermineOrderOfLibraries (List<ModDesc.SharedLibrary> sharedLibraries)
	{
		List<ModDesc.SharedLibrary> ordered = new List<ModDesc.SharedLibrary> ();
		while (sharedLibraries.Count > 0)
		{
			for (int i = 0; i < sharedLibraries.Count; i++)
			{
				var lib = sharedLibraries [i];
				if (lib.Dependencies.Count == 0)
				{
					ordered.Add (lib);
					sharedLibraries.RemoveAt (i);
					i--;
					continue;
				} else
				{
					bool allSatisfied = true;
					foreach (var dep in lib.Dependencies)
						if (ordered.Find (x => x.Name == dep) == null)
						{
							allSatisfied = false;
							break;
						}
					if (allSatisfied)
					{
						ordered.Add (lib);
						sharedLibraries.RemoveAt (i);
						i--;
						continue;
					}
				}
                    
			}
		}
		return ordered;
	}

	List<ModDesc> SearchForMods ()
	{
		List<ModDesc> mods = new List<ModDesc> ();
		string[] manifests = Directory.GetFiles ("Mods", "*.mmf");
		foreach (var path in manifests)
		{
			ModDesc mod = new ModDesc ();

			mod.SharedLibraries = new List<ModDesc.SharedLibrary> ();
			Script script = new Script ();
			script.Options.ScriptLoader = new FileSystemScriptLoader ();
			script.DoFile (path);
			mod.Name = (string)script.Globals ["name"];
			mod.Description = (string)script.Globals ["description"];
			mod.Dependencies = new List<string> ();
			Table table = script.Globals ["shared_libraries"] as Table;
			foreach (var entry in table.Pairs)
			{

				List<string> deps = new List<string> ();
				string dir = "";
				if (entry.Value.Type == DataType.Table)
				{
					object depsTable = entry.Value.Table ["deps"];
					if (depsTable != null)
						foreach (var dep in ((Table)depsTable).Values)
							deps.Add (dep.ToPrintString ());
					dir = (string)entry.Value.Table ["dir"];
				} else
					dir = entry.Value.ToPrintString ();
				mod.SharedLibraries.Add (new ModDesc.SharedLibrary () {
					Name = entry.Key.ToPrintString (),
					Directory = dir,
					Dependencies = deps
				});

			}
			table = script.Globals ["dependencies"] as Table;
			foreach (var entry in table.Values)
			{
				mod.Dependencies.Add (entry.ToPrintString ());


			}
			mods.Add (mod);

		}
		return mods;
	}

	List<ModDesc> DetermineActiveMods ()
	{
		List<ModDesc> activeMods = new List<ModDesc> ();
		foreach (var modName in File.ReadAllLines("UserData\\ActiveMods.txt"))
		{
			ModDesc mod = mods.Find (x => x.Name == modName);
			if (mod != null)
				activeMods.Add (mod);
		}
		return activeMods;
        
	}

	List<ModDesc> DetermineOrder ()
	{
		List<ModDesc> mods = new List<ModDesc> ();
		while (activeMods.Count > 0)
		{
			for (int i = 0; i < activeMods.Count; i++)
			{
				ModDesc mod = activeMods [i];
				if (mod.Dependencies.Count == 0)
				{
					mods.Add (mod);
					activeMods.RemoveAt (i);
					i--;
					continue;
				} else
				{
					bool allSatisfied = true;
					foreach (var dep in mod.Dependencies)
						if (mods.Find (x => x.Name == dep) == null)
						{
							allSatisfied = false;
							break;
						}
					if (allSatisfied)
					{
						mods.Add (mod);
						activeMods.RemoveAt (i);
						i--;
						continue;
					}
				}
			}
		}

		return mods;
	}

	public List<string> GetFiles (string templatePath)
	{
		return new List<string> ();
	}

	public Type ResolveType (string typeID)
	{
		return Type.GetType (typeID);
	}

}




