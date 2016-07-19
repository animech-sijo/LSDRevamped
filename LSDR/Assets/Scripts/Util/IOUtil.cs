﻿using UnityEngine;
using System;
using System.Collections;
using System.IO;
using System.Net;
using Entities;
using Entities.WorldObject;
using Game;
using IO;
using SimpleJSON;
using Types;
using UnityEditor.Animations;

namespace Util
{
	public static class IOUtil
	{
		/// <summary>
		/// Writes the contents of a JSON file to disk. Note that this does not use prettyprint.
		/// </summary>
		/// <param name="json">The JSONClass instance to write to disk</param>
		/// <param name="path">The path to write to</param>
		/// <param name="fileName">The name of the json file</param>
		public static void WriteJSONToDisk(JSONClass json, string path)
		{
			// we don't want to be able to create files without a name
			if (string.IsNullOrEmpty(path))
			{
				Debug.LogError("Error writing JSON file to disk: fileName was null or empty");
				return;
			}

			try
			{
				File.WriteAllText(path, json.ToString());
			}
			catch (IOException e)
			{
				Debug.LogError("Error writing JSON file to disk: " + path);
				Debug.LogException(e);
			}
		}

		/// <summary>
		/// Reads JSON from disk into JSONClass.
		/// </summary>
		/// <param name="path">Path to the json file.</param>
		/// <returns>Null if error reading.</returns>
		public static JSONClass ReadJSONFromDisk(string path)
		{
			try
			{
				JSONClass json = JSON.Parse(File.ReadAllText(path)).AsObject;
				return json;
			}
			catch (IOException e)
			{
				Debug.LogError("Error reading JSON file from disk: " + path);
				Debug.LogException(e);
			}
			return null;
		}

		/// <summary>
		/// Create a Texture2D from a PNG file.
		/// </summary>
		/// <param name="filePath">The path to the file.</param>
		public static Texture2D LoadPNG(string filePath)
		{
			Texture2D tex = null;

			string fullFilePath = filePath;
			if (!Path.GetExtension(fullFilePath).Equals(".png"))
			{
				fullFilePath += ".png";
			}

			if (File.Exists(fullFilePath))
			{
				byte[] fileData = File.ReadAllBytes(fullFilePath);
				tex = new Texture2D(2, 2, TextureFormat.ARGB32, false); // (2, 2) is temporary power of 2 size; next line will resize automatically
				tex.LoadImage(fileData);
				tex.filterMode = FilterMode.Point;
				tex.mipMapBias = 0F;
			}
			else
			{
				Debug.LogError("ERROR: Could not find texture at: " + fullFilePath);
			}
			return tex;
		}

		/// <summary>
		/// Loads an OGG file into the specified AudioSource
		/// </summary>
		public static IEnumerator LoadOGGIntoSource(string filePath, AudioSource source)
		{
			// TODO: handle missing files
		
			Debug.Log("Starting download");
			WWW www = new WWW("file:///" + PathCombine(Application.dataPath, filePath));
			while (!www.isDone)
			{
				Debug.Log("not done...");
				yield return null;
			}
			Debug.Log(www.GetAudioClip(true).name);
			source.clip = www.GetAudioClip(true);
			source.Play();
		}

		/// <summary>
		/// Loads a Torii object (3D model with animations) and returns a gameobject
		/// </summary>
		public static GameObject LoadObject(string filePath, bool collisionMesh)
		{
			// TODO: handle missing files
		
			TOBJ t = new TOBJ();
			ToriiObjectReader.Read(PathCombine(Application.dataPath, filePath), ref t); // load model

			GameObject g = OBJReader.ReadOBJString(t.ObjectFile); // create mesh
			Renderer[] renderers = g.GetComponentsInChildren<Renderer>();
			foreach (Renderer r in renderers)
			{
				Material m = r.material;

				// load texture
				if (!string.IsNullOrEmpty(t.ObjectTexture))
				{
					if (Path.GetFileNameWithoutExtension(t.ObjectTexture).Contains("["))
					{
						// part of a texture set
						m.shader = Shader.Find(GameSettings.UseClassicShaders ? "LSD/PSX/TransparentSet" : "LSD/TransparentSet");

						string texNameWithoutExtension = Path.GetFileNameWithoutExtension(t.ObjectTexture);
						string baseTexName = texNameWithoutExtension.Substring(1);

						string pathToTextureDir = Path.GetDirectoryName(t.ObjectTexture);

						m.SetTexture("_MainTexA", LoadPNG(PathCombine(Application.dataPath, pathToTextureDir, "A" + baseTexName) + ".png"));
						m.SetTexture("_MainTexB", LoadPNG(PathCombine(Application.dataPath, pathToTextureDir, "B" + baseTexName) + ".png"));
						m.SetTexture("_MainTexC", LoadPNG(PathCombine(Application.dataPath, pathToTextureDir, "C" + baseTexName) + ".png"));
						m.SetTexture("_MainTexD", LoadPNG(PathCombine(Application.dataPath, pathToTextureDir, "D" + baseTexName) + ".png"));
					}
					else
					{
						m.shader = Shader.Find(GameSettings.UseClassicShaders ? "LSD/PSX/Transparent" : "Transparent/Diffuse");
						m.SetTexture("_MainTex", LoadPNG(PathCombine(Application.dataPath, t.ObjectTexture)));
					}
				}
			}

			// load animations
			if (t.NumberOfAnimations > 0)
			{
				ToriiObjectAnimator animator = g.AddComponent<ToriiObjectAnimator>();
				foreach (Transform child in g.transform)
				{
					animator.Objects.Add(child.gameObject);
				}
				animator.ToriiObject = t;
			}

			if (collisionMesh)
			{
				foreach (Transform child in g.transform)
				{
					child.gameObject.AddComponent<MeshCollider>();
				}
			}

			g.transform.localScale = new Vector3(-g.transform.localScale.x, g.transform.localScale.y, g.transform.localScale.z);

			return g;
		}

		/// <summary>
		/// Loads MAP file geometry into a mesh and returns a gameobject
		/// </summary>
		public static GameObject LoadMap(string filePath, bool collisionMesh)
		{
			// TODO: handle missing files

			MapReader.MapScaleFactor = 1F;
			
			GameObject g = MapReader.LoadMap(PathCombine(Application.dataPath, filePath),
				PathCombine(Application.dataPath, "textures", "wad"),
				Shader.Find("LSD/DiffuseSet"),
				Shader.Find("LSD/TransparentSet"), collisionMesh);

			return g;
		}

		/// <summary>
		/// Loads a Torii map and returns a gameobject with entities in the torii map as child elements
		/// </summary>
		public static GameObject LoadToriiMap(string filePath, out TMAP tmap)
		{
			// TODO: handle missing files
			
			tmap = ToriiMapReader.ReadFromFile(filePath);

			GameObject tmapObject = new GameObject(tmap.Header.Name);

			foreach (ENTITY e in tmap.Content.Entities)
			{
				GameObject entityObject = EntityInstantiator.Instantiate(e);
				entityObject.transform.SetParent(tmapObject.transform);
			}

			return tmapObject;
		}

		/// <summary>
		/// Combines A and B to form a full path.
		/// </summary>
		public static string PathCombine(string a, string b)
		{
			if (b.StartsWith("\\") || b.StartsWith("/"))
			{
				b = b.Substring(1);
			}
			return Path.Combine(a, b);
		}
		/// <summary>
		/// Combines any number of elements to form a full path.
		/// </summary>
		public static string PathCombine(params string[] componentStrings)
		{
			string path = componentStrings[0];
			for (int i = 1; i < componentStrings.Length; i++)
			{
				path = PathCombine(path, componentStrings[i]);
			}
			return path;
		}
	}
}