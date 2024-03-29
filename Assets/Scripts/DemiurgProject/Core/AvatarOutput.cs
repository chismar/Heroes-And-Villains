using UnityEngine;
using System.Collections;
using System.Reflection;
using System;


namespace Demiurg.Core
{
    public class AvatarOutput : AvatarIO
    {
        public AvatarOutput (object name, FieldInfo field, Avatar avatar) : base (name, field, avatar)
        {
        }

        public Type GetAvatarType ()
        {
            return Avatar.GetType ();
        }

        public void Finish ()
        {
            base.Finish ();
        }
        
    }

}