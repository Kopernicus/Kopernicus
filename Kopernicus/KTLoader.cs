//KittopiaTech plugin Config Loader.
//DWTFYW liscense, I guess.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Reflection;
using KSP.IO;

namespace Kopernicus
{
	public class KittopiaTechLoader
	{
		public static void LoadCB( CelestialBody cbBody, string path, string BodyName )
		{
			ConfigNode root = ConfigNode.Load( path );
			ConfigNode CBNode = root.GetNode( "CelestialBody" );
			//Load CB related stuff
			System.Object cbobj = cbBody;
			foreach( FieldInfo key in cbobj.GetType().GetFields() )
			{
				if( CBNode.HasValue( key.Name ) )
				{
					string val = CBNode.GetValue( key.Name );
					System.Object castedval = val;
					Type t = key.GetValue( cbobj ).GetType();
					
					if ( t == typeof(UnityEngine.Vector3) )
					{
						val = val.Replace( "(" , "" );
						val = val.Replace( ")" , "" );
						
						key.SetValue( cbobj, ConfigNode.ParseVector3( val ) );
					}
					else if ( t == typeof(UnityEngine.Color) )
					{
						val = val.Replace( "RGBA(" , "" );
						val = val.Replace( ")" , "" );
						key.SetValue( cbobj, ConfigNode.ParseColor( val ) );
					}
					else
					{
						key.SetValue( cbobj, Convert.ChangeType( castedval, t ) );
					}
				}
				
				CelestialBody CBody = (CelestialBody)cbobj;
				CBody.CBUpdate();
			}
			Debug.Log("KTLoader: Loaded CB of " +BodyName+ "\n" );
		}
		
		public static PQSMod AddPQSMod( PQS mainSphere, Type ofType )
		{
			//HackHack
			var newGObj = new GameObject();
            var newComponent = (PQSMod)newGObj.AddComponent(ofType);
			newGObj.name = (""+ofType);
			newGObj.name.Replace( "PQSMod_", "_");
			newGObj.transform.parent = mainSphere.gameObject.transform;
            newComponent.sphere = mainSphere;
			
			return newComponent;
		}
	}
}

