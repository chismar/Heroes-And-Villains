using UnityEngine;
using System.Collections;
using Demiurg;


namespace CoreMod
{
    public partial class ContinuousChunksModule : Demiurg.Core.Avatar
    {
        #region enums

        enum TilesConnection
        {
            Four,
            Eight
        }

        enum EnvConnection
        {
            None,
            Cylinder,
            Sphere
        }

        enum Direction
        {
            Left,
            Right,
            Top,
            Down,
            TopLeft,
            TopRight,
            DownLeft,
            DownRight
        }

        #endregion
    }
}



