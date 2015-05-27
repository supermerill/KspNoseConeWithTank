using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NoseConeWithTank
{
	public class ModuleNoseConeWithTank : PartModule, IPartMassModifier
	{
		[KSPField]
		public float maxMonoPropellant;

		[KSPField]
		public float massMonoPropellant = -1;

		[KSPField]
		public float maxLiquidFuel;

		[KSPField]
		public float massLiquidFuel = -1;

		[KSPField]
		public float maxOxidizerPer11;

		[KSPField]
		public float massOxidizerAndLiquidFuel = -1;

		[KSPField]
		public float massAddEmpty = 0;

		[KSPField(isPersistant = true, guiActiveEditor = true, guiActive = false, guiName = "tankType")]
		public string tankType = "none";

		[KSPField(isPersistant = true)]
		public float addedMass = 0;

		public override void OnStart(PartModule.StartState state)
		{
			base.OnStart(state);
		}

		[KSPEvent(guiActiveEditor = true, guiActive = false, guiName = "Switch Tank")]
		public void switchTank()
		{
			try
			{
				//print("[NoseConeWithTank]SWITCH TANK " + initialMass + " => " + part.mass);
				//print("[NoseConeWithTank]SWITCH TANK " + tankType + " => " + maxMonoPropellant + ", " + maxOxidizerPer11);
				switch (tankType)
				{
					case "none":
						tankType = "Rcs";
						if (maxMonoPropellant > 0)
						{
							removeActiveResources();
							double fuelMass = addResource("MonoPropellant", maxMonoPropellant);
							if (massMonoPropellant < 0)
							{
								Debug.Log("[MNCWT] getet rcs for rcs quantity of : " + maxMonoPropellant);
								//Get the mass via the least big rcs tank fuel fraction or something like that
								addedMass = (float)(fuelMass * getRcsMassFraction(maxMonoPropellant));
							}
							else addedMass = (float)(massMonoPropellant);
							//do the same for all symparts
							foreach (Part symPart in part.symmetryCounterparts)
							{
								((ModuleNoseConeWithTank)symPart.Modules["ModuleNoseConeWithTank"]).tankType = "Rcs";
								((ModuleNoseConeWithTank)symPart.Modules["ModuleNoseConeWithTank"]).removeActiveResources();
								((ModuleNoseConeWithTank)symPart.Modules["ModuleNoseConeWithTank"]).addResource("MonoPropellant", maxMonoPropellant);
								((ModuleNoseConeWithTank)symPart.Modules["ModuleNoseConeWithTank"]).addedMass = addedMass;
							}
						}
						else
						{
							switchTank();
						}
						break;
					case "Rcs":
						tankType = "Fuel";
						if (maxLiquidFuel > 0)
						{
							removeActiveResources();
							double fuelMass = addResource("LiquidFuel", maxLiquidFuel);
							if (massLiquidFuel < 0)
							{
								//get the MK1Fuselage part to compute default mass
								double coeff = 0.2;
								foreach (AvailablePart ap in PartLoader.LoadedPartsList)
								{
									if (ap.name.Equals("MK1Fuselage"))
									{
										//get base mass
										double baseMass = ap.partPrefab.mass;
										//get resourcemass
										double resourceMass = 0;
										foreach (PartResource res in ap.partPrefab.Resources)
										{
											resourceMass += res.maxAmount * res.info.density;
										}
										coeff = baseMass / resourceMass;
										break;
									}
								}
								addedMass = (float)(fuelMass * coeff);
							}
							else addedMass = (float)(massLiquidFuel);
							//do the same for all symparts
							foreach (Part symPart in part.symmetryCounterparts)
							{
								((ModuleNoseConeWithTank)symPart.Modules["ModuleNoseConeWithTank"]).tankType = "Fuel";
								((ModuleNoseConeWithTank)symPart.Modules["ModuleNoseConeWithTank"]).removeActiveResources();
								((ModuleNoseConeWithTank)symPart.Modules["ModuleNoseConeWithTank"]).addResource("LiquidFuel", maxLiquidFuel);
								((ModuleNoseConeWithTank)symPart.Modules["ModuleNoseConeWithTank"]).addedMass = addedMass;
							}
						}
						else
						{
							switchTank();
						}
						break;
					case "Fuel":
						tankType = "Fuel/Oxi";
						//40 maxOxidizerPer11 => 0.5t ==> 0.0125t / maxOxidizerPer11
						if (maxOxidizerPer11 > 0)
						{
							removeActiveResources();
							double fuelMass = 0;
							fuelMass += addResource("LiquidFuel", maxOxidizerPer11 * 9);
							fuelMass += addResource("Oxidizer", maxOxidizerPer11 * 11);
							if (massOxidizerAndLiquidFuel < 0)
							{
								//get the t100 part to compute default mass
								double coeff = 0.125;
								foreach (AvailablePart ap in PartLoader.LoadedPartsList)
								{
									if (ap.name.Equals("fuelTankSmallFlat"))
									{
										//get base mass
										double baseMass = ap.partPrefab.mass;
										//get resourcemass
										double resourceMass = 0;
										foreach (PartResource res in ap.partPrefab.Resources)
										{
											resourceMass += res.maxAmount * res.info.density;
										}
										coeff = baseMass / resourceMass;
										break;
									}
								}
								addedMass = (float)(fuelMass * coeff);
							}
							else addedMass = (float)(massOxidizerAndLiquidFuel);
							//do the same for all symparts
							foreach (Part symPart in part.symmetryCounterparts)
							{
								((ModuleNoseConeWithTank)symPart.Modules["ModuleNoseConeWithTank"]).tankType = "Fuel/Oxi";
								((ModuleNoseConeWithTank)symPart.Modules["ModuleNoseConeWithTank"]).removeActiveResources();
								((ModuleNoseConeWithTank)symPart.Modules["ModuleNoseConeWithTank"]).addResource("LiquidFuel", maxOxidizerPer11 * 9);
								((ModuleNoseConeWithTank)symPart.Modules["ModuleNoseConeWithTank"]).addResource("Oxidizer", maxOxidizerPer11 * 11);
								((ModuleNoseConeWithTank)symPart.Modules["ModuleNoseConeWithTank"]).addedMass = addedMass;
							}
						}
						else
						{
							switchTank();
						}
						break;
					default:
						tankType = "none";
						removeActiveResources();
						//part.mass = (float)(initialMass);
						addedMass = massAddEmpty;
						print("[NCWT] EMPTY massbefore:" + part.mass + ", addedmass" + addedMass + " => totalmass=" + (part.mass + addedMass));
						//do the same for all symparts
						foreach (Part symPart in part.symmetryCounterparts)
						{
							((ModuleNoseConeWithTank)symPart.Modules["ModuleNoseConeWithTank"]).tankType = "none";
							((ModuleNoseConeWithTank)symPart.Modules["ModuleNoseConeWithTank"]).removeActiveResources();
							((ModuleNoseConeWithTank)symPart.Modules["ModuleNoseConeWithTank"]).addedMass = addedMass;
						}
						break;
				}
			}
			catch (Exception e)
			{
				Debug.LogError("[NoseConeWithTank]ERROR " + e);
			}
			GameEvents.onEditorShipModified.Fire(EditorLogic.fetch.ship);
		}

		public float GetModuleMass(float defaultMass)
		{
			return addedMass;
		}

		public void removeActiveResources()
		{
			//foreach (PartResource res in activeResources)
			List<PartResource> allResource = new List<PartResource>();
			allResource.AddRange(part.Resources.list);
			foreach (PartResource res in allResource)
			{
				part.Resources.list.Remove(res);
				Destroy(res);
			}
			//activeResources.Clear();
		}

		// return maxfuelmass
		public double addResource(string name, float amount)
		{
			ConfigNode newPartResource = new ConfigNode("RESOURCE");
			newPartResource.AddValue("name", name);
			newPartResource.AddValue("amount", amount);
			newPartResource.AddValue("maxAmount", amount);
			//activeResources.Add(part.Resources.Add(newPartResource));
			PartResource res = part.AddResource(newPartResource);
			double mass = 0;
			if (res != null)
			{
				mass = res.maxAmount * res.info.density;
			}
			return mass;
		}


		public static double getRcsMassFraction(double storedAmount)
		{
			double coeff = 0.25;
			foreach (AvailablePart ap in PartLoader.LoadedPartsList)
			{
				//Debug.Log("[MNCWT] get rcs coeff: for " + storedAmount);
				//try
				//{
				//	Debug.Log("[MNCWT]see ap " + ap);
				//	Debug.Log("[MNCWT]see appartPrefab " + ap.partPrefab);
				//	Debug.Log("[MNCWT]see appname " + ap.name);
				//	Debug.Log("[MNCWT]see apResources " + ap.partPrefab.Resources);
				//	Debug.Log("[MNCWT]see apResources count " + ap.partPrefab.Resources.Count);
				//	Debug.Log("[MNCWT]see apResource name " + ap.partPrefab.Resources[0].resourceName);
				//	Debug.Log("[MNCWT]see apResource amount " + ap.partPrefab.Resources[0].amount);
					
				//}
				//catch (Exception e) { }
				//only get rcs tank with less/equal fuel than me.
				if (ap != null && ap.partPrefab != null && ap.partPrefab.Resources!=null)
					if (ap.partPrefab.Resources.Count == 1
						&& ap.partPrefab.Resources[0].resourceName == "MonoPropellant"
						&& ap.partPrefab.Resources[0].amount <= storedAmount)
					{
						Debug.Log("[MNCWT]see appname " + ap.name);
						Debug.Log("[MNCWT] set rcs coeff: find prefab " + ap.partPrefab.partName);
						//get base mass
						double baseMass = ap.partPrefab.mass;
						//get resourcemass
						double resourceMass = 0;
						foreach (PartResource res in ap.partPrefab.Resources)
						{
							resourceMass += res.maxAmount * res.info.density;
						}
						Debug.Log("[MNCWT] set rcs coeff: find prefab coeff? " +coeff+" >? "+ (baseMass / resourceMass));
						//update coeff if found better
						if (coeff > baseMass / resourceMass)
							coeff = baseMass / resourceMass;
					}
			}
			Debug.Log("[MNCWT] set rcs coeff: " + coeff);
			return coeff;
		}

		public override void OnLoad(ConfigNode node)
		{
			base.OnLoad(node);
			//load added mass (at pre-prelaunch : in flight but without vessel)
			if (vessel == null && HighLogic.LoadedSceneIsFlight)
				part.mass += addedMass;
		}
	}
}
