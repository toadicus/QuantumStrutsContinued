using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace QuantumStrut
{
    public class QuantumStrut : PartModule
    {
        public static Material LaserMaterial;

        Strut strut = null;
        GameObject lineObj;
        LineRenderer lr;
        bool Editor = false;
        int I = 0;

        #region Fields
        [KSPField(isPersistant = true)]
        public bool IsEnabled = true;

        [KSPField(isPersistant = false)]
        public float PowerConsumption = 0;


        [KSPField(isPersistant = false)]
        public string TransformName = "";

        [KSPField(isPersistant = false)]
        public Vector3 Start = new Vector3(0,0,0);

        [KSPField(isPersistant = false)]
        public Vector3 Dir = new Vector3(0, 1, 0);


        [KSPField(isPersistant = false)]
        public string Material = "Particles/Additive";

        [KSPField(isPersistant = false)]
        public Vector3 StartColor = Vector3.zero;

        [KSPField(isPersistant = false)]
        public Vector3 EndColor = Vector3.zero;


        [KSPField(isPersistant = false)]
        public float StartSize = 0.03f;

        [KSPField(isPersistant = false)]
        public float EndSize = 0.0075f;
        #endregion

        #region Actions
        [KSPAction("Toggle")]
        public void ToggleStrut(KSPActionParam param)
        {
            IsEnabled = !IsEnabled;
            CheckHit();
        }

        [KSPAction("Activate")]
        public void ActivateStrut(KSPActionParam param)
        {
            IsEnabled = true;
            CheckHit();
        }

        [KSPAction("Deactivate")]
        public void DeactivateStrut(KSPActionParam param)
        {
            IsEnabled = false;
            CheckHit();
        }
        #endregion

        #region Events
        [KSPEvent(guiActive = true, guiName = "Activate", active = true)]
        public void ActivateStrut()
        {
            IsEnabled = true;
            CheckHit();
        }

        [KSPEvent(guiActive = true, guiName = "Deactivate", active = false)]
        public void DeactivateStrut()
        {
            IsEnabled = false;
            CheckHit();
        }
        #endregion

        public Material material = null;
        public Color startColor = Color.white;
        public Color endColor = Color.white;

        public Color Vector3toColor(Vector3 vec)
        {
            return new Color(vec.x/255, vec.y/255, vec.z/255);
        }

        Transform getTransform()
        {
            if (TransformName == "")
                return part.transform;
            else
                return part.FindModelTransform(TransformName);
        }

        public void print(object body, params object[] args)
        {
            string final = body.ToString();
            for (int I = 0; I < args.Length; I++)
            {
                final = final.Replace("{" + I + "}", args[I].ToString());
            }
            MonoBehaviour.print("[AutoStrut] " + final);
        }

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
        }

        public override void OnActive()
        {
            InitLaser();

            base.OnActive();
        }

        public override void OnInactive()
        {
            DestroyLaser();

            base.OnInactive();
        }

        public override string GetInfo()
        {
            return "Requires:\n- ElectricCharge (" + PowerConsumption + "/s.)\n\n Costs 5 to create strut.";
            return base.GetInfo();
        }

        public override void OnStart(PartModule.StartState state)
        {
            try
            {
                print("Material: {0}", Material);
                material = new Material(Shader.Find(Material.Trim()));
            }
            catch
            {
                material = null;
            }

            startColor = Vector3toColor(StartColor);
            endColor = Vector3toColor(EndColor);

            if (!Util.isValid(LaserMaterial))
                LaserMaterial = new Material(Shader.Find("Particles/Additive"));

            if (state == StartState.Docked)
                CheckHit();

            if (state == StartState.Editor)
            {
                Editor = true;
                RenderingManager.AddToPostDrawQueue(0, DrawBuildOverlay);
                InitLaser();
            }
            else
            {
                Editor = false;
                RenderingManager.RemoveFromPostDrawQueue(0, DrawBuildOverlay);
                DestroyLaser();
            }

            base.OnStart(state);
        }

        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
        }


        public override void OnUpdate()
        {
            Events["ActivateStrut"].active = !IsEnabled;
            Events["DeactivateStrut"].active = IsEnabled;

            if (IsEnabled)
            {
                I = I + 1 % 255;

                if (strut != null && !strut.isDestroyed)
                {
                    if (PowerConsumption == 0 || (Util.GetEnergy(part.vessel) > PowerConsumption * TimeWarp.fixedDeltaTime && part.RequestResource("ElectricCharge", PowerConsumption * TimeWarp.fixedDeltaTime) > 0))
                        strut.Update();
                    else
                        strut.Destroy();
                }
                else
                {
                    if ((I % 10)==0)
                    {
                        CheckHit();
                    }
                }
            }
            else
            {
                if (strut != null)
                {
                    strut.Destroy();
                    strut = null;
                }
            }

            base.OnUpdate();
        }

        void CheckHit()
        {
            if (!isEnabled)
            {
                strut.Destroy();
                strut = null;
                return;
            }

            if (strut == null || strut.isDestroyed)
            {
                Vector3 dir = getTransform().TransformDirection(Dir);
                Vector3 start = getTransform().TransformPoint(Start);

                UnityEngine.RaycastHit info = new RaycastHit();
                bool hit = Physics.Raycast(new UnityEngine.Ray(start + (dir * 0.05f), dir), out info, 10);
                if (hit)
                {
                    Part targetPart = Util.partFromRaycast(info);

                    if (targetPart && vessel.parts.Contains(targetPart) && Util.GetEnergy(part.vessel) > 5 * TimeWarp.fixedDeltaTime)
                    {
                        float energy = part.RequestResource("ElectricCharge", 5 * TimeWarp.fixedDeltaTime);

                        strut = new Strut(part, targetPart, targetPart.transform.InverseTransformPoint(info.point), getTransform());
                        strut.Material = material;
                        strut.StartColor = startColor;
                        strut.EndColor = endColor;
                        strut.StartSize = StartSize;
                        strut.EndSize = EndSize;
                    }
                }
            }
        }

        void InitLaser()
        {
            if (!Util.isValid(lr))
            {
                lineObj = new GameObject();

                lr = lineObj.AddComponent<LineRenderer>();
                lr.useWorldSpace = true;

                lr.material = material;
                lr.SetColors(startColor, endColor);
                lr.SetWidth(StartSize, EndSize);

                lr.SetVertexCount(2);
                lr.SetPosition(0, Vector3.zero);
                lr.SetPosition(1, Vector3.zero);
                lr.castShadows = false;
                lr.receiveShadows = true;
            }
        }

        void DestroyLaser()
        {
            if (Util.isValid(lr))
                LineRenderer.DestroyImmediate(lr);

            if (Util.isValid(lineObj))
                GameObject.DestroyImmediate(lineObj);
        }

        public void DrawBuildOverlay()
        {
            if (Util.isValid(part))
            {
                if (!Editor) return;

                if (Util.isValid(lr))
                {
                    Vector3 dir = getTransform().TransformDirection(Dir);
                    Vector3 start = getTransform().TransformPoint(Start);

                    UnityEngine.RaycastHit info = new RaycastHit();
                    bool hit = Physics.Raycast(new UnityEngine.Ray(start + (dir * 0.05f), dir), out info, 10);
                    if (hit)
                    {
                        if (Util.isValid(material))
                            lr.material = material;

                        lr.SetColors(startColor, endColor);
                        lr.SetWidth(StartSize, EndSize);

                        lr.SetPosition(0, start);
                        lr.SetPosition(1, info.point);
                    }
                    else
                    {
                        lr.material = LaserMaterial;
                        lr.SetColors(Color.red, Color.red);
                        lr.SetWidth(0.01f, 0.01f);

                        lr.SetPosition(0, start);
                        lr.SetPosition(1, start + (dir * 10));
                    }
                }
            }
            else
            {
                DestroyLaser();
                RenderingManager.RemoveFromPostDrawQueue(0, DrawBuildOverlay);
            }
        }
    }
}