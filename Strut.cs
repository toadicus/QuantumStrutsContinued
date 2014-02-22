// QuantumStrutsContinued © 2014 toadicus
//
// This work is licensed under the Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License. To view a
// copy of this license, visit http://creativecommons.org/licenses/by-nc-sa/3.0/
//
// Continued from QuantumStrut, © 2013 BoJaN.  Used with permission.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//using System.Threading.Tasks;
using UnityEngine;

namespace QuantumStrut
{
	class Strut
	{
		public bool isDestroyed = false;
		Material _material = null;

		public Material Material
		{
			set
			{
				_material = value;
				if (Material != null)
					lr.material = Material;
			}
			get
			{
				return _material;
			}
		}

		Color _startColor = Color.white;

		public Color StartColor
		{
			set
			{
				_startColor = value;
				lr.SetColors(StartColor, EndColor);
			}
			get
			{
				return _startColor;
			}
		}

		Color _endColor = Color.white;

		public Color EndColor
		{
			set
			{
				_endColor = value;
				lr.SetColors(StartColor, EndColor);
			}
			get
			{
				return _endColor;
			}
		}

		float _startSize = 0;

		public float StartSize
		{
			set
			{
				_startSize = value;
				lr.SetWidth(StartSize, EndSize);
			}
			get
			{
				return _startSize;
			}
		}

		float _endSize = 0;

		public float EndSize
		{
			set
			{
				_endSize = value;
				lr.SetWidth(StartSize, EndSize);
			}
			get
			{
				return _endSize;
			}
		}

		ConfigurableJoint joint;
		Transform parentTransform = null;
		Part parent = null;
		Part target = null;
		Vector3 targetOffset = Vector3.zero;
		GameObject LineObj = null;
		LineRenderer lr = null;

		public void print(object body, params object[] args)
		{
			string final = body.ToString();
			for (int I = 0; I < args.Length; I++)
			{
				final = final.Replace("{" + I + "}", args[I].ToString());
			}
			MonoBehaviour.print("[AutoStrut] " + final);
		}

		void DrawLine(Vector3 origin, Vector3 end)
		{
			if (Util.isValid(lr))
			{
				lr.SetPosition(0, origin);
				lr.SetPosition(1, end);
			}
		}

		public Strut(Part parent, Part target, Vector3 targetOffset, Transform parentTransform)
		{
			this.parent = parent;
			this.target = target;
			this.targetOffset = targetOffset;
			this.parentTransform = parentTransform;

			if (parent.vessel.parts.Contains(target))
			{
				joint = parent.parent.gameObject.AddComponent<ConfigurableJoint>();
				joint.connectedBody = target.rigidbody;

				joint.anchor = new Vector3(
					0,
					0,
					Vector3.Distance(
						parentTransform.position,
						target.transform.TransformPoint(targetOffset)
					) / 2
				);
				joint.axis = new Vector3(0, 0, 1);
				joint.xMotion = ConfigurableJointMotion.Locked;
				joint.yMotion = ConfigurableJointMotion.Locked;
				joint.zMotion = ConfigurableJointMotion.Locked;
				joint.angularXMotion = ConfigurableJointMotion.Locked;
				joint.angularYMotion = ConfigurableJointMotion.Locked;
				joint.angularZMotion = ConfigurableJointMotion.Locked;

				LineObj = new GameObject();
				LineObj.name = "quantumstrut";

				lr = LineObj.AddComponent<LineRenderer>();
				lr.useWorldSpace = true;

				Material = QuantumStrut.LaserMaterial;
				StartColor = Color.white;
				EndColor = Color.white;
				StartSize = 0.03f;
				EndSize = 0.0075f;

				lr.SetVertexCount(2);
				lr.SetPosition(0, Vector3.zero);
				lr.SetPosition(1, Vector3.zero);
			}
			else
			{
				Destroy();
			}
		}

		public void Update()
		{
			if (Util.isValid(parent) && Util.isValid(target) && Util.isValid(parent.vessel) && parent.vessel.parts.Contains(target))
			{
				Vector3 start = parentTransform.position;
				Vector3 end = target.transform.TransformPoint(targetOffset);
				DrawLine(start, end);
			}
			else
			{
				DrawLine(Vector3.zero, Vector3.zero);
				Destroy();
			}
		}

		public void Destroy()
		{
			DrawLine(Vector3.zero, Vector3.zero);
			if (Util.isValid(joint))
				GameObject.DestroyImmediate(joint);

			if (Util.isValid(lr))
				GameObject.DestroyImmediate(lr);

			if (Util.isValid(LineObj))
				GameObject.DestroyImmediate(LineObj);

			joint = null;
			LineObj = null;
			lr = null;

			parentTransform = null;
			parent = null;
			target = null;
			targetOffset = Vector3.zero;
			isDestroyed = true;
		}
	}
}