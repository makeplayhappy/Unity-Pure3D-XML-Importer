using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(P3DXMLImporter))]
public class P3DXMLImporterInspector : Editor{

    


    public override void OnInspectorGUI () {
        DrawDefaultInspector();

        P3DXMLImporter importer = (P3DXMLImporter)target;
        if(GUILayout.Button("Import Files")){
           importer.RunImporter();
        }

	}



}
