﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Demiurg;

public class FrameworkRoots : MonoBehaviour
{
    void Awake ()
    {
        Find.Register<PlanetsGenerator> ();
        Find.Register<ModsManager> ();
        Find.Register<LuaContext> ();
        Find.Register<Sprites> ();
    }
    
}

