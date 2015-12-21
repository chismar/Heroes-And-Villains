using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Demiurg.Core.Extensions;
using System;
using MoonSharp.Interpreter;

namespace Demiurg.Essentials
{
    public class ListLoader : IConfigLoader
    {
        static Type listType = typeof(IList);

        public bool IsSpecific ()
        {
            return false;
        }

        public bool Check (Type targetType)
        {
            return listType.IsAssignableFrom (targetType);
        }

        public object Load (object fromObject, Type targetType, Demiurg.Core.ConfigLoaders loaders)
        {
            IList objects = Activator.CreateInstance (targetType) as IList;
            ITable table = fromObject as ITable;
            Type containedType = targetType.GetGenericArguments () [0];
            IConfigLoader loader = loaders.FindLoader (containedType);
            var keys = table.GetKeys ();
            foreach (var key in keys)
            {
                objects.Add (loader.Load (table.Get (key), containedType, loaders));
            }
            return objects;

        }
        
    }
}

