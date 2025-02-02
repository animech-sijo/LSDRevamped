﻿using Torii.Event;
using UnityEditor;
using UnityEngine;

namespace DefaultNamespace
{
    [CustomEditor(typeof(ToriiEvent))]
    public class ToriiEventEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUI.enabled = Application.isPlaying;

            ToriiEvent e = target as ToriiEvent;
            if (GUILayout.Button("Raise"))
                e.Raise();
        }
    }
}