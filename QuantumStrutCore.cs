// QuantumStrutsContinued © 2014 toadicus
//
// This work is licensed under the Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License. To view a
// copy of this license, visit http://creativecommons.org/licenses/by-nc-sa/3.0/
//
// Continued from QuantumStrut, © 2013 BoJaN.  Used with permission.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using UnityEngine;
using KSP.IO;


namespace QuantumStrut
{
    class QuantumStrutCore : PartModule
    {
        GameObject lineObj = null;
        LineRenderer lr = null;
        ConfigNode nodeToLoad = null;

        bool Placing = false;
        bool Deleting = false;
        List<CoreStrut> struts = new List<CoreStrut>();

        Part startPart = null;
        Vector3 startOffset = Vector3.zero;
        Part endPart = null;
        Vector3 endOffset = Vector3.zero;

        public Material material = null;
        public Color startColor = Color.white;
        public Color endColor = Color.white;

        #region Fields
        [KSPField(isPersistant = true)]
        public bool IsEnabled = true;

        [KSPField(isPersistant = false)]
        public float PowerConsumption = 0;

        [KSPField(isPersistant = false)]
        public int MaxStruts = 8;

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
        public void ToggleCore(KSPActionParam param)
        {
            IsEnabled = !IsEnabled;
        }

        [KSPAction("Activate")]
        public void ActivateCore(KSPActionParam param)
        {
            IsEnabled = true;
        }

        [KSPAction("Deactivate")]
        public void DeactivateCore(KSPActionParam param)
        {
            IsEnabled = false;
        }
        #endregion

        #region Events
        [KSPEvent(guiActive = true, guiName = "Place Strut", active = false, externalToEVAOnly=true, guiActiveUnfocused=true, unfocusedRange = 5)]
        public void BeginPlace()
        {
            startPart = null;
            startOffset = Vector3.zero;
            endPart = null;
            endOffset = Vector3.zero;
            Placing = true;
        }

        [KSPEvent(guiActive = true, guiName = "Remove Strut", active = false, externalToEVAOnly = true, guiActiveUnfocused = true, unfocusedRange = 2500)]
        public void BeginDelete()
        {
            Deleting = true;
        }

        [KSPEvent(guiActive = true, guiName = "Activate", active = true)]
        public void ActivateCore()
        {
            IsEnabled = true;
        }

        [KSPEvent(guiActive = true, guiName = "Deactivate", active = false)]
        public void DeactivateCore()
        {
            IsEnabled = false;
        }
        #endregion

        public Vector3 StringToVector3(string str)
        {
            string[] vals = str.Split(',');
            float x = (float)Convert.ToDecimal(vals[0]);
            float y = (float)Convert.ToDecimal(vals[1]);
            float z = (float)Convert.ToDecimal(vals[2]);
            return new Vector3(x, y, z);
        }

        public Color Vector3toColor(Vector3 vec)
        {
            return new Color(vec.x / 255, vec.y / 255, vec.z / 255);
        }

        Part partFromGameObject(GameObject ob)
        {
            GameObject o = ob;

            while (o)
            {
                Part p = Part.FromGO(o);
                if (p && p != null)
                {
                    return p;
                }

                if (o.transform.parent)
                    o = o.transform.parent.gameObject;
                else
                    return null;
            }
            return null;
        }

        Part partFromRaycast(RaycastHit hit)
        {
            return partFromGameObject(hit.collider.gameObject);
        }

        Part partFromId(long id)
        {
            Console.WriteLine("Vessel Parts: " + part.vessel.Parts.Count);
            foreach (Part p in part.vessel.Parts)
            {
                print(p.uid + " ?= " + id + ": " + (p.uid == id));
                if (p.uid == id)
                    return p;
            }
            return null;
        }

        bool isValid(UnityEngine.Object obj)
        {
            return (obj && obj != null);
        }

        void InitLaser()
        {
            if (!isValid(lr))
            {
                lineObj = new GameObject();

                lr = lineObj.AddComponent<LineRenderer>();
                lr.useWorldSpace = true;

                lr.material = new Material(Shader.Find("Particles/Additive"));
                lr.SetColors(Color.white, Color.white);
                lr.SetWidth(0.03f, 0.03f);

                lr.SetVertexCount(2);
                lr.SetPosition(0, Vector3.zero);
                lr.SetPosition(1, Vector3.zero);
                lr.castShadows = false;
                lr.receiveShadows = true;
            }
        }

        void DestroyLaser()
        {
            if (isValid(lr))
                LineRenderer.DestroyImmediate(lr);
            if (isValid(lineObj))
                GameObject.DestroyImmediate(lineObj);
        }

        float GetEnergy()
        {
            double energy = 0;
            foreach (Part p in part.vessel.parts)
            {
                foreach (PartResource r in p.Resources)
                {
                    if (r.resourceName == "ElectricCharge")
                        energy += r.amount;
                }
            }
            return (float)energy;
        }

        void AddStrut(Part a, Vector3 aO, Part b, Vector3 bO)
        {
            CoreStrut s = new CoreStrut(a, aO, b, bO);
            s.Material = material;
            s.StartColor = startColor;
            s.EndColor = endColor;
            s.StartSize = StartSize;
            s.EndSize = EndSize;
            struts.Add(s);
        }

        public override string GetInfo()
        {
            return "Max Struts: " + MaxStruts + "\nRequires:\n- ElectricCharge (" + PowerConsumption + "/s.)\n\n Energy cost is per-strut.";
        }

        public override void OnStart(PartModule.StartState state)
        {
            try
            {
                material = new Material(Shader.Find(Material.Trim()));
            }
            catch
            {
                material = null;
            }

            startColor = Vector3toColor(StartColor);
            endColor = Vector3toColor(EndColor);
            base.OnStart(state);
        }

        public override void OnSave(ConfigNode node)
        {
            foreach (CoreStrut strut in struts)
            {
                if (!strut.isDestroyed)
                {
                    ConfigNode n = node.AddNode("QuantumStrut");
                    n.AddValue("parent", part.vessel.parts.IndexOf(strut.parent));
                    n.AddValue("parentOffset", strut.parentOffset.x + "," + strut.parentOffset.y + "," + strut.parentOffset.z);
                    n.AddValue("target", part.vessel.parts.IndexOf(strut.target));
                    n.AddValue("targetOffset", strut.targetOffset.x + "," + strut.targetOffset.y + "," + strut.targetOffset.z);
                }
            }

            base.OnSave(node);
        }

        public override void OnLoad(ConfigNode node)
        {
            if (nodeToLoad == null)
                nodeToLoad = node;

            base.OnLoad(node);
        }

        void printObj(object obj)
        {
            using (KSP.IO.TextWriter writer = TextWriter.CreateForType<string>("obj.cs"))
            {
                string str = printProperties(obj, TypeDescriptor.GetProperties(obj), 0);
                writer.Write(str);
                print(str);
            }
        }

        string printProperties(object obj, PropertyDescriptorCollection collection, int I)
        {
            if (I > 2) return "";

            string prefix = "";
            string str = "";
            for (int T = 0; T < I; T++)
            {
                prefix += "    ";
            }

            str += "\n" + prefix + "#region";
            foreach (PropertyDescriptor d in collection)
            {
                str += "\n" + prefix + d.Name + " = " + d.GetValue(obj);
                str += printProperties(obj, d.GetChildProperties(obj), I+1);
            }
            str += "\n" + prefix + "#endregion";
            return str;
        }

        public override void OnUpdate()
        {
            bool eva = isValid(Util.Kerbal);
            Events["ActivateCore"].active = !IsEnabled;
            Events["DeactivateCore"].active = IsEnabled;
            Events["BeginPlace"].active = !Placing && !Deleting && struts.Count < MaxStruts && eva;
            Events["BeginDelete"].active = !Deleting && !Placing && struts.Count > 0 && eva;

            if (nodeToLoad != null)
            {
                foreach (ConfigNode strut in nodeToLoad.GetNodes("QuantumStrut"))
                {
                    Part parent = part.vessel.parts[Convert.ToInt32(strut.GetValue("parent"))];
                    Vector3 parentOffset = StringToVector3(strut.GetValue("parentOffset"));
                    Part target = part.vessel.parts[Convert.ToInt32(strut.GetValue("target"))];
                    Vector3 targetOffset = StringToVector3(strut.GetValue("targetOffset"));

                    if (isValid(parent) && isValid(target))
                        AddStrut(parent, parentOffset, target, targetOffset);
                }
                nodeToLoad = null;
            }

            if (eva)
            {
                InitLaser();
                if (Placing)
                {
                    RaycastHit info = new RaycastHit();
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    bool hit = Physics.Raycast(ray, out info, Mathf.Infinity);
                    if (isValid(startPart))
                    {
                        Vector3 pos = startPart.transform.TransformPoint(startOffset);
                        ray = new Ray(pos, (info.point - pos).normalized);
                        hit = Physics.Raycast(ray, out info, 10);
                    }

                    if (Input.GetMouseButtonUp(0))
                    {
                        if (hit)
                        {
                            if (!isValid(startPart))
                            {
                                startPart = partFromRaycast(info);
                                if (isValid(startPart))
                                    startOffset = startPart.transform.InverseTransformPoint(info.point);
                            }
                            else if (!isValid(endPart))
                            {
                                Part p = partFromRaycast(info);
                                if (isValid(p))
                                {
                                    if (p != startPart && startPart.vessel.parts.Contains(p))
                                    {
                                        endPart = p;
                                        endOffset = endPart.transform.InverseTransformPoint(info.point);

                                        AddStrut(startPart, startOffset, endPart, endOffset);
                                    }
                                    Placing = false;
                                }
                            }
                        }
                    }

                    if(isValid(startPart))
                    {
                        if (hit)
                        {
                            Part p = partFromRaycast(info);
                            Color c = Color.red;

                            if (isValid(p))
                            {
                                if (startPart.vessel.parts.Contains(p) && p != startPart)
                                    c = Color.green;
                            }

                            lr.SetColors(c, c);
                            lr.SetPosition(0, startPart.transform.TransformPoint(startOffset));
                            lr.SetPosition(1, info.point);
                        }
                        else
                        {
                            lr.SetColors(Color.red, Color.red);
                            lr.SetPosition(0, startPart.transform.TransformPoint(startOffset));
                            lr.SetPosition(1, ray.GetPoint(10));
                        }
                    }
                }
                else
                {
                    lr.SetPosition(0, Vector3.zero);
                    lr.SetPosition(1, Vector3.zero);
                }

                if (Deleting)
                {
                    RaycastHit info = new RaycastHit();
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    bool hit = Physics.Raycast(ray, out info, Mathf.Infinity);

                    CoreStrut closest = null;
                    float closestDist = Mathf.Infinity;
                    foreach (CoreStrut strut in struts)
                    {
                        strut.Selected = false;
                        if (hit)
                        {
                            Vector3 start = strut.parent.transform.TransformPoint(strut.parentOffset);
                            float d = Vector3.Distance(start, info.point);

                            if (d < 0.05 && d < closestDist)
                            {
                                closest = strut;
                                closestDist = d;
                            }
                        }
                    }

                    if(closest != null)
                        closest.Selected = true;

                    if (Input.GetMouseButtonUp(0))
                    {
                        if (closest != null)
                        {
                            closest.Destroy();
                            struts.Remove(closest);
                        }
                        Deleting = false;
                    }
                }
            }
            else
            {
                DestroyLaser();
            }

            foreach (CoreStrut s in struts.ToArray())
            {
                if (IsEnabled)
                {
                    if (s.Active)
                    {
                        if (PowerConsumption == 0 || (GetEnergy() > PowerConsumption * TimeWarp.fixedDeltaTime && part.RequestResource("ElectricCharge", PowerConsumption * TimeWarp.fixedDeltaTime) > 0))
                        {
                            s.Active = true;
                        }
                        else
                        {
                            s.Active = false;
                        }
                    }
                    else
                    {
                        if (GetEnergy() > 5 * TimeWarp.fixedDeltaTime && part.RequestResource("ElectricCharge", 5 * TimeWarp.fixedDeltaTime) > 0)
                        {
                            s.Active = true;
                        }
                    }
                }
                else
                {
                    s.Active = false;
                }

                s.Update();
                if (s.isDestroyed)
                    struts.Remove(s);
            }
            base.OnUpdate();
        }
    }
}
