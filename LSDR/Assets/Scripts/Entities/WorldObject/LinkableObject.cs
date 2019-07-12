﻿using UnityEngine;
using System.Collections;

namespace Entities.WorldObject
{
	// TODO: LinkableObject is now obsolete
	public class LinkableObject : MonoBehaviour
	{
		public string LinkedLevel;
		public Color FadeColor;
		public bool ForceFadeColor;
		public bool LinkToSpecificLevel;
		public bool DisableLinking;
		public bool IsSolid;
	}
}