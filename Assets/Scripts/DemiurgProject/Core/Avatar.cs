using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Linq;
using UnityEngine;
using System.Text;
using UIO;

namespace Demiurg.Core
{
	public abstract class Avatar
	{
		protected static System.Random Random;
		Scribe scribe = Scribes.Find ("Avatars");

		public int Seed { get; internal set; }

		public static Avatar Create (Type type, string name, int seed)
		{
			Avatar avatar = Activator.CreateInstance (type) as Avatar;
			avatar.SetupIO ();
			avatar.Configure (name, seed);
			return avatar;
		}

		protected List<AvatarInput> Inputs = new List<AvatarInput> ();
		protected Dictionary<string, AvatarOutput> Outputs = new Dictionary<string, AvatarOutput> ();
		protected List<AvatarConfig> Configs = new List<AvatarConfig> ();


		public string Name { get; internal set; }

		protected DemiurgEntity Demiurg { get; set; }

		public void Configure (string name, int seed)
		{
			Name = name;
			Seed = seed;
			Random = new System.Random (Seed);

		}

		public void Configure (DemiurgEntity demiurg, ITable wiringTable, ITable configs)
		{
			Demiurg = demiurg;
			SetupWiring (wiringTable);
			SetupConfigs (configs);
                
		}

		public void TryWork ()
		{

			if (Inputs.Count == 0)
			{
				Debug.LogFormat ("[AVATARS WORKFLOW] {0} started working", this.Name);
				Work ();
			}
		}

		public AvatarOutput GetOutput (string name)
		{
			AvatarOutput output = null;
			Outputs.TryGetValue (name, out output);
			return output;
		}

		void SetupWiring (ITable wiringTable)
		{
			Debug.LogFormat ("{0} started wiring inputs {1}", Name, Inputs.Count);
			foreach (var input in Inputs)
			{
				ITable table = wiringTable.GetTable (input.Name);
				if (table == null)
				{
					scribe.LogFormatError ("INPUT DATA MISSING: Can't find wiring reference for avatar {0} input {1}", Name, input.Name);
					continue;
				}
				string targetAvatarName = table.GetString (1);
				string targetOutputName = table.GetString (2);
				if (targetAvatarName == null || targetOutputName == null)
				{
					scribe.LogFormatError ("INPUT DATA MISSING: Can't retrieve wiring reference data for avatar {0} input {1}\n Retrieved: {2} | {3}", Name, input.Name, targetAvatarName, targetOutputName);
					continue;
				}
				Avatar targetAvatar = Demiurg.FindAvatar (targetAvatarName);
				if (targetAvatar == null)
				{
					scribe.LogFormatError ("INPUT DATA MISSING: Can't find target avatar {2} for avatar {0} input {1}", Name, input.Name, targetAvatarName);
					continue;
				}
				AvatarOutput output = targetAvatar.GetOutput (targetOutputName);
				if (output == null)
				{
					scribe.LogFormatError ("INPUT DATA MISSING: Can't find output {2} in avatar {3} for avatar {0} input {1}", Name, input.Name, targetOutputName, targetAvatarName);
					continue;
				}
				input.ConnectTo (output);
			}
		}

		void SetupConfigs (ITable configs)
		{
			if (configs != null)
				Find.Root<ModsManager> ().Defs.LoadObjectAs<Avatar> (this, configs);
//			if (configs == null)
//			{
//				scribe.LogFormatWarning ("No configs table for: {0}", Name);
//
//				return;
//			}
//			foreach (var config in Configs)
//			{
//				IConfigLoader loader = loaders.FindLoader (config.FieldType ());
//
//				object cfg = configs.GetTable (config.Name);
//				if (cfg == null)
//				{
//					scribe.LogFormatError ("Config not found {0} {1} {2} {3} {4}", Name, config.Name, config.FieldType (), loader, cfg);
//					if (!configs.Contains (config.Name))
//					{
//						foreach (var key in configs.GetKeys())
//							scribe.LogError (key.ToString ());
//					}
//					continue;
//				}
//				object value = loader.Load (configs, config.Name, config.FieldType (), loaders);
//				if (value == null)
//				{
//					scribe.LogFormatError ("Value not loaded properly {0} {1} {2} {3}", Name, config.Name, config.FieldType (), loader);
//					if (!configs.Contains (config.Name))
//					{
//						foreach (var key in configs.GetKeys())
//							scribe.LogError (key.ToString ());
//					}
//					continue;
//				}
//				config.SetValue (value);
//			}
		}

		public abstract void Work ();

		protected void FinishWork ()
		{
			Debug.LogFormat ("[AVATARS WORKFLOW] {0} finished working", this.Name);
			foreach (var output in Outputs)
				output.Value.Finish ();
		}

		class FieldData
		{
			public FieldInfo Field { get; internal set; }

			public object ID { get; internal set; }

			public FieldData (FieldInfo field, object id)
			{
				Field = field;
				ID = id;
			}


		}

		class InheritedClassData
		{
			public List<FieldData> inputs = new List<FieldData> ();
			public List<FieldData> outputs = new List<FieldData> ();
			public List<FieldData> configs = new List<FieldData> ();
		}

		static Dictionary<Type, InheritedClassData> usedAvatars = new Dictionary<Type, InheritedClassData> ();

		static Type inputAttr = typeof(AInput);
		static Type outputAttr = typeof(AOutput);
		static Type configAttr = typeof(AConfig);

		static BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic |
		                            BindingFlags.Static | BindingFlags.Instance |
		                            BindingFlags.DeclaredOnly;

		public static IEnumerable<FieldInfo> GetAllFields (Type type)
		{
			if (type == null)
				return Enumerable.Empty<FieldInfo> ();
			return type.GetFields (flags).Concat (GetAllFields (type.BaseType));
		}

		public static void UseAvatarType (Type type)
		{
			var fields = GetAllFields (type);

			InheritedClassData data = new InheritedClassData ();
			int ioCount = 0;
			foreach (var field in fields)
			{
				
				if (field.IsDefined (inputAttr, true))
				{
					data.inputs.Add (new FieldData (field, ((AInput)Attribute.GetCustomAttribute (field, inputAttr)).Name));
					ioCount++;
				}
				if (field.IsDefined (outputAttr, true))
				{
					data.outputs.Add (new FieldData (field, ((AOutput)Attribute.GetCustomAttribute (field, outputAttr)).Name));
					ioCount++;
				}
				if (field.IsDefined (configAttr, true))
				{
					data.configs.Add (new FieldData (field, ((AConfig)Attribute.GetCustomAttribute (field, configAttr)).Name));
					ioCount++;
				}
			}
			StringBuilder builder = new StringBuilder (100);
			builder.Append ("IN: ");
			foreach (var inp in data.inputs)
			{
				builder.Append (inp.ID);
				builder.Append (' ');
			}
			builder.Append ("OUT: ");
			foreach (var inp in data.outputs)
			{
				builder.Append (inp.ID);
				builder.Append (' ');
			}
			builder.Append ("CFG: ");
			foreach (var inp in data.configs)
			{
				builder.Append (inp.ID);
				builder.Append (' ');
			}
			Debug.LogFormat ("{0} : {1} : {2}", type, ioCount, builder.ToString ());
			usedAvatars.Add (type, data);
		}

		int finishedInputs = 0;

		void CountFinishedInputs ()
		{
			finishedInputs++;
			if (finishedInputs >= Inputs.Count)
			{
				Debug.LogFormat ("[AVATARS WORKFLOW] {0} started working", this.Name);
				Work ();
			}
                
		}

		public void SetupIO ()
		{
			var data = usedAvatars [this.GetType ()];
			foreach (var con in data.configs)
			{
				AvatarConfig config = new AvatarConfig (con.ID, con.Field, this);
				Configs.Add (config);
			}
			foreach (var inp in data.inputs)
			{
				AvatarInput input = new AvatarInput (inp.ID, inp.Field, this);
				input.OnFinish (CountFinishedInputs);
				Inputs.Add (input);
			}

			foreach (var outp in data.outputs)
			{
				AvatarOutput output = new AvatarOutput (outp.ID, outp.Field, this);
				Outputs.Add ((string)outp.ID, output);
			}
		}
	}

}