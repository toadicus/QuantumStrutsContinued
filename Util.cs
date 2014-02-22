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
using System.Threading.Tasks;
using KSP.IO;
using UnityEngine;

namespace QuantumStrut
{
    public static class Util
    {
        static System.Random r = new System.Random();

        public static float Random(float min, float max)
        {
            return (float)((r.NextDouble() * (max - min)) + min);
        }

        public static float Random()
        {
            return (float)r.NextDouble();
        }

        public static Part getTank(Part parent)
        {
            if (parent.Resources.Count != 0)
                return parent;

            if (isValid(parent.parent) && parent.fuelCrossFeed)
                return getTank(parent.parent);

            return null;
        }

        public static Vessel Kerbal
        {
            get
            {
                if (isValid(FlightGlobals.fetch.activeVessel) && FlightGlobals.fetch.activeVessel.isEVA)
                    return FlightGlobals.fetch.activeVessel;
                return null;
            }
        }

        public static bool isValid(UnityEngine.Object obj)
        {
            return (obj && obj != null);
        }

        public static void printObj(object obj)
        {
            using (KSP.IO.TextWriter writer = TextWriter.CreateForType<string>("obj.cs"))
            {
                string str = printProperties(obj, TypeDescriptor.GetProperties(obj), 0);
                writer.Write(str);
                MonoBehaviour.print(str);
            }
        }

        static string printProperties(object obj, PropertyDescriptorCollection collection, int I)
        {
            if (I > 2) return "";

            string prefix = "";
            string str = "";
            for (int T = 0; T < I; T++)
            {
                prefix += "    ";
            }

            foreach (PropertyDescriptor d in collection)
            {
                str += "\n" + prefix + d.Name + " = " + d.GetValue(obj);
            }
            return str;
        }

        public static Part partFromGameObject(GameObject ob)
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

        public static Part partFromRaycast(RaycastHit hit)
        {
            return partFromGameObject(hit.collider.gameObject);
        }

        public static Part partFromId(Vessel vessel, long id)
        {
            Console.WriteLine("Vessel Parts: " + vessel.Parts.Count);
            foreach (Part p in vessel.Parts)
            {
                MonoBehaviour.print(p.uid + " ?= " + id + ": " + (p.uid == id));
                if (p.uid == id)
                    return p;
            }
            return null;
        }

        public static float GetEnergy(Vessel vessel)
        {
            double energy = 0;
            foreach (Part p in vessel.parts)
            {
                foreach (PartResource r in p.Resources)
                {
                    if (r.resourceName == "ElectricCharge")
                        energy += r.amount;
                }
            }
            return (float)energy;
        }
    }
}
