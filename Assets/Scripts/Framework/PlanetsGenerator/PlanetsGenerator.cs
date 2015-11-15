
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Demiurg;
using System;
using System.Reflection;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;

[RootDependencies(typeof(LuaContext), typeof(ModsManager), typeof(ObjectsCreator))]
public class PlanetsGenerator : Root
{
    public const string WiringTable = "wiring";
    LuaContext luaContext;
    ModsManager modsManager;
    WorldCreator creator;
    protected override void CustomSetup ()
    {
        creator = new WorldCreator ();

		

        luaContext = Find.Root<LuaContext> ();
        //luaContext.DeclareLibrary("Demiurg", new NameFuncPair[]{new NameFuncPair( "module_outputs", Outputs)});
        modsManager = Find.Root<ModsManager> ();
        //luaContext.LoadScripts (modsManager.GetFiles("Demiurg\\" + WiringTable + ".lua"), WiringTable);
        //luaContext.LoadScript ("Mods\\CoreMod\\Demiurg\\Wiring\\Wiring.lua", WiringTable);
        Dictionary<string, Type> nodes = FindNodeTypes ();

        Script script = new Script ();
        RegisterSlotComponents (script);
        script.Globals ["wiring"] = new Table (script);
        script.Globals ["tags"] = new Table (script);
        script.Globals ["tag_expressions"] = new Table (script);
        (script.Globals ["tag_expressions"] as Table) ["component"] = script.Globals ["component"];
        script.Options.ScriptLoader = new FileSystemScriptLoader ();
        script.DoFile ("Mods\\CoreMod\\Demiurg\\TagExpressions\\Expressions.lua", script.Globals ["tag_expressions"] as Table);
        
        (script.Globals ["tags"] as Table) ["tag_expressions"] = script.Globals ["tag_expressions"];
        script.DoFile ("Mods\\CoreMod\\Demiurg\\Tags\\Tags.lua", script.Globals ["tags"] as Table);
        script.DoFile ("Mods\\CoreMod\\Demiurg\\Wiring\\Wiring.lua", script.Globals ["wiring"] as Table);


        //script.Globals.Values
        Dictionary<string, Table> tables = new Dictionary<string, Table> ();
        foreach (var pair in ((Table)script.Globals ["wiring"]).Pairs)
        {
            Debug.Log (pair.Key.CastToString ());
            tables.Add (pair.Key.CastToString (), pair.Value.Table);
        }
        creator.SetupTags (FormTags (script.Globals ["tags"] as Table));
        creator.InitWiring (tables, nodes);

        Fulfill.Dispatch ();
    }

    Dictionary<string, Type> FindNodeTypes ()
    {
        Dictionary<string, Type> nodes = new Dictionary<string, Type> ();
        Assembly asm = Assembly.GetExecutingAssembly ();
        foreach (var type in asm.GetTypes())
        {
            if (type.IsSubclassOf (typeof(CreationNode)) && !type.IsAbstract && !type.IsGenericType)
                nodes.Add (type.FullName, type);
        }
        return nodes;
    }

    public Dictionary<string, Tag> FormTags (Table tagsTable)
    {
        Dictionary<string, Tag> tags = new Dictionary<string, Tag> ();
        foreach (var entry in tagsTable.Pairs)
        {
            if (entry.Value.Table ["expression"] == null)
                continue;
            Tag tag = new Tag (entry.Key.ToPrintString (), tags.Count, entry.Value.Table ["expression"] as Closure, entry.Value.Table ["criteria"] as Table);
            tags.Add (entry.Key.ToPrintString (), tag);
        }
        return tags;
    }
 
    void RegisterSlotComponents (Script script)
    {
        script.Globals ["component"] = new Table (script);
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
                
        SlotComponentsProvider.Types = slotComponents;

    }
}




