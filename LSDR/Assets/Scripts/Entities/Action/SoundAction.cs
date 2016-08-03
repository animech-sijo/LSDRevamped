﻿using UnityEngine;
using System.Collections;
using Entities.Dream;
using Types;
using UnityEngine.Audio;
using Util;

namespace Entities.Action
{
	public class SoundAction : BaseAction
	{
		public string PathToAudioClip;

		private AudioSource _source;

		private static AudioMixer _masterMixer = Resources.Load<AudioMixer>("Mixers/MasterMixer");

		public static GameObject Instantiate(ENTITY e)
		{
			GameObject instantiated = new GameObject(e.Classname);
			SoundAction actionScript = instantiated.AddComponent<SoundAction>();
			actionScript.Name = e.GetPropertyValue("Sequence name");
			actionScript.SequencePosition = EntityUtil.TryParseInt("Sequence position", e);
			actionScript._source = instantiated.AddComponent<AudioSource>();
			actionScript._source.outputAudioMixerGroup = _masterMixer.FindMatchingGroups("SFX")[0];

			actionScript.PathToAudioClip = e.GetPropertyValue("Audio clip");

			DreamDirector.OnLevelFinishChange += actionScript.PostLoad;

			EntityUtil.SetInstantiatedObjectTransform(e, ref instantiated);

			return instantiated;
		}

		private void PostLoad()
		{
			ReferencedSequence = ActionSequence.FindSequence(Name);
			AddSelf();

			StartCoroutine(IOUtil.LoadOGGIntoSource(IOUtil.PathCombine("sfx", PathToAudioClip), _source));
		}

		public override IEnumerator DoAction()
		{
			ReferencedSequence.ReferencedGameObject.GetComponent<AudioSource>().PlayOneShot(_source.clip);
			ReferencedSequence.DoNextAction();
			yield return null;
		}
	}
}