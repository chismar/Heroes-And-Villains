﻿using UnityEngine;
using System.Collections;
using MoonSharp.Interpreter;
using UIO;

namespace UIOBinding
{
	public class BindingFunction : ICallback
	{
		public Closure Closure { get; internal set; }

		public object ID { get; internal set; }

		public BindingFunction (Closure closure, object id)
		{
			this.Closure = closure;
			this.ID = id;
		}

		public object Call (params object[] args)
		{
			for (int i = 0; i < args.Length; i++)
			{
				BindingTable table = args [i] as BindingTable;
				if (table != null)
					args [i] = table.Table;
				else
				{
					BindingFunction func = args [i] as BindingFunction;
					if (func != null)
						args [i] = func.Closure;
				}
			}
			DynValue value = Closure.Call (args);
			switch (value.Type)
			{
			case DataType.Boolean:
				return value.CastToBool ();
			case DataType.Nil:
				return null;
			case DataType.Function:
				return new BindingFunction (value.Function, null);
			case DataType.Number:
				return value.CastToNumber ();
			case DataType.String:
				return value.CastToString ();
			case DataType.Table:
				return new BindingTable (value.Table, null);
			default:
				return null;
			}
		}



	}

}