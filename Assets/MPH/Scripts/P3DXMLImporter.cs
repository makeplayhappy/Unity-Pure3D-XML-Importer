using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Xml;


[CreateAssetMenu(fileName = "new P3DXMLImporter", menuName = "DH/P3DXMLImporter", order = 55)]
public class P3DXMLImporter : ScriptableObject{
//public class P3DXMLImporter : MonoBehaviour{
  /*  
    #if UNITY_EDITOR


    #endif
*/
    public enum runType{    All,    Materials,    Meshes,    Textures    }
    public string importFolderPath = "ImportFolder"+Path.DirectorySeparatorChar;
    public string outputFolderPath = "RoadRageAssets"+Path.DirectorySeparatorChar;

    public runType importTypes = runType.All;

    private string basepath;
    private string currentBasename;
    private string textureFolder = "Textures";
    private string materialFolder = "Materials";
    //public Material testMaterial;

    //private Pure3D.File XML_File;

    public void RunImporter(){

        basepath = Application.dataPath;

        

        string search_path = basepath + Path.DirectorySeparatorChar + importFolderPath;

        Debug.Log( search_path );

        if( Directory.Exists(search_path) ){

            string[] files_found = Directory.GetFiles(search_path,"*.p3dxml");

            Debug.Log("Found " + files_found.Length + " p3dxml files");

            if( files_found.Length > 0 ){
                 foreach (string p3dxml_filename in files_found){

                    string m_filename = Path.GetFileName(p3dxml_filename);
                    
                    currentBasename = Path.GetFileNameWithoutExtension(p3dxml_filename);

                    DebugManager.Log("Found <b>" + currentBasename + "</b> at <i>" + p3dxml_filename + "</i>", 9999);

                    try{

                        XmlDocument xmlDoc = new XmlDocument();
		                xmlDoc.Load(p3dxml_filename);
                        //DebugManager.Log(xmlDoc.InnerXml, 1);
//                      PrintHierarchy(xmlDoc, 0);

                        GenerateAssets(xmlDoc);

                    }

                    catch(System.Exception e){
                        DebugManager.Log("<color=red>Caught error: " + e.Message + "</color> \nTrace: " +  e.StackTrace + "", 1);
                    }


                }

            }

        }
        
    }


    public void GenerateAssets( XmlDocument document ){
        //get top level nodes
        XmlNodeList chunks = document.SelectNodes("/Pure3DFile/Chunk");

        if( chunks.Count > 0 ){

            for (int p = 0; p < chunks.Count; p++) {

                XmlNode chunk = chunks[p];
                string cType = chunk.Attributes["Type"].Value;

                switch( cType ){
                    case "0x7000": //header history comment
                    break;
                    case "0x19000": //sprite or texture
                    // !TODO check if texture exists in texture folder

                    break;

                    case "0x11000": //shader - convert to material...
                        if( importTypes == runType.All || importTypes == runType.Materials){

                            DebugManager.Log("== Gen Material == " +  cType.ToString() , 5 );
                            GenerateMaterial(chunk);

                        }
                        


                    break;

                    case "0x4500": //skeleton
                        if( importTypes == runType.All || importTypes == runType.Meshes){
                            DebugManager.Log("== Gen Skeleton == " +  cType.ToString() , 5 );
                            GenerateSkeleton(chunk);
                        }
                    break;

                    
                    case "0x10001": // Skin mesh - made up of meshes and colliders
                        if( importTypes == runType.All || importTypes == runType.Meshes){
                            DebugManager.Log("== Gen Skin Mesh == " +  cType.ToString() , 5 );
                        }
                    break;

                    case "0x4512": //composite drawable (the full prefab)
                        if( importTypes == runType.All || importTypes == runType.Meshes){
                            DebugManager.Log("== Gen Prefab composite drawable == " +  cType.ToString() , 5 );
                        }
                    break;

                    case "0x4290":
                        DebugManager.Log("== Skinned Animation == " +  cType.ToString() , 5 );
                    break;
                    case "0x4520":
                        DebugManager.Log("== Simple Animation == " +  cType.ToString() , 5 );
                    break;
                    default:
                        DebugManager.Log("== <b>UNKNOWN IN SWITCH GenerateAssets</b> " +  cType.ToString() , 5 );
                    break;    

                }
               
            }


        }

    }



    //positional data is held in a 4x4 matrix
    public void GenerateSkeleton( XmlNode rootChunk ){
        //Get the value chunks
        //name and version
        XmlNode RootNameElement = rootChunk.SelectSingleNode("Value[@Name='Name']");
        string rootName = RootNameElement.Attributes["Value"].Value;   

        XmlNodeList chunkChunks = rootChunk.SelectNodes("Chunk");
        DebugManager.Log("Skeleton " + rootName  , 50 );

        List<Transform> mSkeleton = new List<Transform>();
        
        if( chunkChunks.Count > 0 ){

            for (int i = 0; i < chunkChunks.Count; i++) {

                XmlNode chunk = chunkChunks[i];
                string cType = chunk.Attributes["Type"].Value;

                switch( cType ){
                    case "0x4501": //Skeleton Joint
                        XmlNodeList jointValues = chunk.SelectNodes("Value");
                      
                        if( jointValues.Count > 0 ){

                            Matrix4x4 _restPoseMatrix = new Matrix4x4();
                            string _jointName = "";
                            int _jointParent = 0;
                            int _jointIndx = i; //should get this from child 0x4503 - MappedJointIndex

                            Vector3 t_position = Vector3.zero;
                            Quaternion t_rotation = Quaternion.identity;
                            Vector3 t_scale = Vector3.one;

                            for (int j = 0; j < jointValues.Count; j++) {

                                XmlNode jointElement = jointValues[j];
                                string vName = jointElement.Attributes["Name"].Value;

                                switch( vName ){
                                    case "Name":
                                        _jointName = jointElement.Attributes["Value"].Value;
                                    break;
                                    case "Parent":
                                        _jointParent = int.Parse( jointElement.Attributes["Value"].Value );
                                    break;
                                    case "RestPose":
                                //    M11="1" M12="0" M13="-0" M14="0" M21="0" M22="1" M23="-0" M24="0" M31="-0" M32="-0" M33="1" M34="-0" M41="0" M42="0" M43="0" M44="1" />
                                        for(int x = 0;x <= 3; x++){
                                            for(int y = 0;y <= 3; y++){
                                                string Mattribute = "M" + (x+1).ToString() + (y+1).ToString();
                                                _restPoseMatrix[x,y] = Convert.ToSingle( jointElement.Attributes[Mattribute].Value );
                                            }
                                        }
                                        DebugManager.Log(i + " " + _jointName + " Matrix \n" + _restPoseMatrix.ToString("e2")  , 50 );
                                        //DebugManager.Log("Is valid Matrix? " + _restPoseMatrix.ValidTRS()  , 50 );
                                        t_position = _restPoseMatrix.MultiplyPoint3x4( Vector3.one );
                                        DebugManager.Log(i + " " + _jointName + " Position " + t_position.ToString("F2")  , 50 );

                                        t_rotation = _restPoseMatrix.rotation;
                                        DebugManager.Log(i + " " + _jointName + " Rotation " + t_rotation.eulerAngles.ToString("F2")  , 50 );

                                        t_scale = _restPoseMatrix.lossyScale;
                                        DebugManager.Log(i + " " + _jointName + " Scale " + t_scale.ToString("F2")  , 50 );

                                    break;
                                }

                                
                            } //end for through value nodes

                            XmlNodeList jointChunks = chunk.SelectNodes("Chunk"); //child chunks 
                            // - Skeleton Joint Mirror Map
                            // - Skeleton Joint Bone Preserve

                            //create the empty
                            GameObject go = new GameObject();
                            go.name = _jointName;
                            Transform tr = go.transform;
                            if( _jointParent == 0){

                                go.AddComponent<BoneDebug>();



                            }else if( _jointParent > 0 && mSkeleton.Count > _jointParent){
                                tr.parent = mSkeleton[_jointParent];
                            }
                            tr.localPosition = t_position;
                            tr.localRotation = t_rotation;
                            tr.localScale = t_scale;

                            mSkeleton.Add(tr);
                        }


                        

                        
 


                    break;
                    default:
                       DebugManager.Log("Found type " + cType + " in Skeleton " + rootName  , 5 ); 
                    break;
                }

                
            }
        } 
        
    }

/*
    public static void Decompose(ref SharpDX.Matrix Matrix, out Vector3 Scale)
		{
			Scale = new Vector3();
			Scale.X = (float)Math.Sqrt((double)(Matrix.M11 * Matrix.M11 + Matrix.M12 * Matrix.M12 + Matrix.M13 * Matrix.M13));
			Scale.Y = (float)Math.Sqrt((double)(Matrix.M21 * Matrix.M21 + Matrix.M22 * Matrix.M22 + Matrix.M23 * Matrix.M23));
			Scale.Z = (float)Math.Sqrt((double)(Matrix.M31 * Matrix.M31 + Matrix.M32 * Matrix.M32 + Matrix.M33 * Matrix.M33));
		}

		public static void Decompose(ref SharpDX.Matrix Matrix, out Vector3 Scale, out Vector3 Translation)
		{
			Translation = new Vector3();
			Translation.X = Matrix.M41;
			Translation.Y = Matrix.M42;
			Translation.Z = Matrix.M43;
			Utility.Decompose(ref Matrix, out Scale);
		}

		public static bool Decompose(ref SharpDX.Matrix Matrix, out Vector3 Scale, out Matrix3x3 Rotation, out Vector3 Translation)
		{
			Utility.Decompose(ref Matrix, out Scale, out Translation);
			if (MathUtil.IsZero(Scale.X) || MathUtil.IsZero(Scale.Y) || MathUtil.IsZero(Scale.Z))
			{
				Rotation = Matrix3x3.Identity;
				return false;
			}
			Matrix3x3 matrix3x3 = new Matrix3x3()
			{
				M11 = Matrix.M11 / Scale.X,
				M12 = Matrix.M12 / Scale.X,
				M13 = Matrix.M13 / Scale.X,
				M21 = Matrix.M21 / Scale.Y,
				M22 = Matrix.M22 / Scale.Y,
				M23 = Matrix.M23 / Scale.Y,
				M31 = Matrix.M31 / Scale.Z,
				M32 = Matrix.M32 / Scale.Z,
				M33 = Matrix.M33 / Scale.Z
			};
			Rotation = matrix3x3;
			return true;
		}
*/

    //Shader = 0x11000,        
    // Handle the shader setting - import as Standard Material
    public void GenerateMaterial(XmlNode rootChunk){
        string _extension = ".mat";
        string _chunkName = "";

        bool _isTranslucent = false;
        bool _hasLighting = false;
        bool _alphaTested = false;
        bool _twoSided = false;

        bool _isEmissive = false;

        string _blendMode = ""; //None,	Alpha,	Additive,	Subractive,
        string _texture = "";
        string _textureFilter = "";
        string _UVRepeatMode = ""; //repeat or clamp to edge


        Color32 _spec = new Color32(0, 0, 0, 255);
        Color32 _ambi = new Color32(255, 255, 255, 255);
        Color32 _diff = new Color32(255, 255, 255, 255);
        Color32 _emis = new Color32(0, 0, 0, 255);

        DebugManager.Log(rootChunk.InnerXml , 50 );


        //Get the value chunks
        XmlNodeList valueChunks = rootChunk.SelectNodes("Value");
        DebugManager.Log("Value Chunks " + valueChunks.Count  , 50 );
        if( valueChunks.Count > 0 ){

            for (int i = 0; i < valueChunks.Count; i++) {

                XmlNode valueElement = valueChunks[i];
                string vName = valueElement.Attributes["Name"].Value;
                string vVal = valueElement.Attributes["Value"].Value;
                switch( vName ){
                    case "Name":
                        _chunkName = vVal;
                    break;
                    case "HasTranslucency":
                        _isTranslucent = vVal == "1";
                    break;
                    case "Version":
                    case "PddiShaderName":
                    case "VertexNeeds":
                    case "VertexMask":
                    break;
                }

                
            }

            //get chunk data
            XmlNodeList chunkChunks = rootChunk.SelectNodes("Chunk");

            DebugManager.Log("GenerateMaterial " +  _chunkName + " is translucent " + _isTranslucent + " chunks " + chunkChunks.Count , 50 );

            if( chunkChunks.Count > 0 ){
                XmlNode shaderValueNode;
                string shaderValue;
                XmlNode shaderParamNode;
                string shaderParam;

                for (int i = 0; i < chunkChunks.Count; i++) {
                    
                    XmlNode chunk = chunkChunks[i];
                    string cType = chunk.Attributes["Type"].Value;
                    //ShaderTextureParam = 0x11002,        ShaderIntParam = 0x11003,        ShaderFloatParam = 0x11004,        ShaderColorParam = 0x11005,
                    switch(cType){
                        case "0x11002":
                        //texture
                        // <Chunk Type="0x11002"> <Value Name="Value" Value="nameoftexture" /> <Value Name="Param" Value="TEX" /> </Chunk>
                            shaderValueNode = chunk.SelectSingleNode("Value[@Name='Value']");
                            shaderValue = shaderValueNode.Attributes["Value"].Value;
                            if( !String.IsNullOrEmpty(shaderValue) ){
                                _texture = shaderValue;
                                //DebugManager.Log("Texture = " + _texture, 50 );

                            }
                        break;
                        case "0x11003": //integer

                            shaderParamNode = chunk.SelectSingleNode("Value[@Name='Param']");
                            shaderParam = shaderParamNode.Attributes["Value"].Value;

                            shaderValueNode = chunk.SelectSingleNode("Value[@Name='Value']");
                            shaderValue = shaderValueNode.Attributes["Value"].Value;

                            switch(shaderParam){
                                case "LIT":
                                    _hasLighting = shaderValue == "1";
                                break;
                                case "BLMD":
                                    _blendMode = shaderValue;
                                break;
                                case "UVMD":
                                    _UVRepeatMode = shaderValue;
                                break;
                                case "ATST":
                                    _alphaTested = shaderValue == "1";
                                break;
                                case "2SID":
                                    _twoSided = shaderValue == "1";
                                break;
                                
                                
                                case "SHMD":
                                case "FIMD":
                                break;
                            }

                        break;
                        case "0x11004": //floats
                            //"SHIN" ??
                        break;
                        case "0x11005": // RGB values 0 - 255
                            shaderParamNode = chunk.SelectSingleNode("Value[@Name='Param']");
                            shaderParam = shaderParamNode.Attributes["Value"].Value;

                            shaderValueNode = chunk.SelectSingleNode("Value[@Name='Value']");

                            int shaderR = int.Parse( shaderValueNode.Attributes["Red"].Value );
                            int shaderG = int.Parse( shaderValueNode.Attributes["Green"].Value);
                            int shaderB = int.Parse( shaderValueNode.Attributes["Blue"].Value);


                            Color32 shaderColor = new Color32( (byte)shaderR , (byte)shaderG, (byte)shaderB, 255);

                            switch(shaderParam){
                                case "SPEC":
                                    if( shaderR > 0 || shaderG > 0 || shaderB > 0){
                                        _spec = shaderColor;
                                    }
                                break;
                                case "DIFF":
                                    if( shaderR < 255 || shaderG < 255 || shaderB < 255){
                                        _diff = shaderColor;
                                    }
                                break;
                                case "AMBI":
                                    if( shaderR < 255 || shaderG < 255 || shaderB < 255){
                                        _ambi = shaderColor;
                                    }
                                break;
                                case "EMIS":
                                    if( shaderR > 0 || shaderG > 0 || shaderB > 0){
                                        _emis = shaderColor;
                                        _isEmissive = true;
                                    }
                                break;


                            }


                        break;

                    }
                    
                }
            }

        }

        string shaderName = "Standard";
        //get root data
        if( _alphaTested || _blendMode == "1" ){
            //shaderName = "Particles/Standard Surface";
            shaderName = "Mobile/Particles/Alpha Blended";
            
        }


       // if( !_hasLighting ){
       //     shaderName = "Particles/Standard Surface";
       // }

        Material material = new Material(Shader.Find(shaderName));
        material.name = currentBasename + "_" + _chunkName;

        if( !String.IsNullOrEmpty(_texture) ){

            string texturePath = outputFolderPath + textureFolder + Path.DirectorySeparatorChar + _texture + ".png";
            string fullTexPath = basepath + Path.DirectorySeparatorChar + texturePath;
            string assetDBTexPath = "Assets" + Path.DirectorySeparatorChar + texturePath;

            DebugManager.Log("<b>Searching for Texture</b> = " + _texture + " in " + fullTexPath, 50 );

            if( System.IO.File.Exists(fullTexPath)){
                DebugManager.Log("Found Texture = " + _texture + " in " + fullTexPath , 50 );
                Texture2D tex = (Texture2D)AssetDatabase.LoadAssetAtPath(assetDBTexPath, typeof(Texture2D));
                material.mainTexture = tex;
            }

        }

        if( _twoSided ){
            material.SetFloat("_Cull", 0f);
        }              
        
        material.color = _diff;
        if( _isEmissive ){
            material.SetColor ("_EmissionColor", _emis);
        }

        




        string savefilepath = outputFolderPath + materialFolder + Path.DirectorySeparatorChar + currentBasename +"_" + _chunkName + _extension; 
        string fullfilepath = basepath + Path.DirectorySeparatorChar + savefilepath;
        string assetDBpath = "Assets" + Path.DirectorySeparatorChar + savefilepath;

        if( System.IO.File.Exists(fullfilepath)){
            AssetDatabase.DeleteAsset( assetDBpath );
        }

        //System.IO.File.WriteAllBytes(fullfilepath, _bytes);

        AssetDatabase.CreateAsset(material, assetDBpath);
        AssetDatabase.SaveAssets();

        //AssetDatabase.CreateAsset(destTex, assetDBpath);
        //AssetDatabase.SaveAssets();

        DebugManager.Log($"Saved to :{assetDBpath}",5);
        //Selection.activeObject = destTex;
/* */
    }



    static void PrintHierarchy(XmlDocument document, int indent){
        string tabs = new System.String('\t', indent);

        XmlNodeList chunks = document.SelectNodes("/Pure3DFile/Chunk");
        DebugManager.Log("Found " +  chunks.Count.ToString() + " Top Level Chunks" , 5 );
        if( chunks.Count > 0 ){

            for (int p = 0; p < chunks.Count; p++) {

                XmlNode chunk = chunks[p];
                string cType = chunk.Attributes["Type"].Value;
                switch( cType ){
                    case "0x7000": //header history comment
                    case "0x19000": //sprite or texture
                    case "0x11000": //shader
                    
                    break;
                    default:
                        DebugManager.Log(tabs + "== " +  cType.ToString() , 5 );
                    break;    

                }
               
            }


        }

    }




/*


    public void generateMesh( Pure3D.Chunks.Mesh _chunk ){
        if( _chunk.Type == 65536 && _chunk.Children.Count > 0 ){
            DebugManager.Log($"Generate MESH {_chunk.Name}" , 5);

            //Could have a range of diffent children
            // bounding box / bounding sphere /  prim group > vertex shader | position list | uv list | color list | index list
            // prm group > prm group im vertex description / vertex data / indices 
            // create a parent empty and add them all in! -- try and add in a method to the chunk that will generate the unity component

            //sceene add empty
            //gameObject parent

            foreach (var _chunk_level_1 in _chunk.Children){
                
               // _chunk.generateUnityComponents(gameObject parent);


            }




        }

    }



    public void generateTexture(Pure3D.Chunks.Texture _chunk ){

        //check children, textures have image chunk > image data
        //we at the parent level 102400 => Texture, should have child  102401 => Image that has child 102402 => ImageData
        if( _chunk.Type == 102400 && _chunk.Children.Count > 0 ){
            DebugManager.Log($"Generate Texture {_chunk.Width}x{_chunk.Height} TextureType {_chunk.TextureType} Mips: {_chunk.NumMipMaps} BitsPerPixel: {_chunk.Bpp} AlphaDepth: {_chunk.AlphaDepth}" , 5);

            foreach (var _chunk_level_1 in _chunk.Children){
                if( _chunk_level_1.Type == 102401 && _chunk_level_1.Children.Count > 0 ){
                    Pure3D.Chunks.Image _chunk_image = (Pure3D.Chunks.Image)_chunk_level_1;
                    DebugManager.Log($"Image Chunk Level1: {_chunk_image.Name} ({_chunk_image.Format}) ({_chunk_image.Width}x{_chunk_image.Height})" , 5);
                    foreach (var _chunk_level_2 in _chunk_image.Children){
                        
                        if( _chunk_level_2.Type == 102402 ){

                            Pure3D.Chunks.ImageData _chunk_image_data = (Pure3D.Chunks.ImageData)_chunk_level_2;



                            if(_chunk_image_data.Data.Length > 0){
                                



 
//Attempt using Unity Engine DXT handling -- gives broken images with junk at the lower part of image       
       


                                DebugManager.Log($"Image Data size: {_chunk_image_data.Data.Length}" , 5);

                                // Texture2D(int width, int height, TextureFormat format, bool mipmap);
                                ImageUtils.PixelFormat pFormat = ImageUtils.PixelFormat.DXT1;
                                bool hasAlpha = true;

                               // byte[] decompressed = ImageUtils.LoadDXT(_chunk_image_data.Data, (uint)_chunk.Width, (uint)_chunk.Height, pFormat,out hasAlpha);

                                uint dWidth;
                                uint dHeight;
                                ImageUtils.PixelFormat dPixelFormat;
                                bool dHasAlpha;

                                byte[] decompressed = ImageUtils.LoadDDS(_chunk_image_data.Data,  out dWidth, out dHeight, out dPixelFormat, out dHasAlpha);
                                byte[] flipped = ImageUtils.flip_byte_image_vertically(decompressed , (int)dWidth, (int)dHeight, 4);

                                //flip the image
                                //byte[] flipped = new byte[decompressed.Length];
                                //flip in 4 byte chunks
                                
                                //int imgbyteWidth = 4;
                                //this isn't quite right -- still needs flipping after this per row
                                //this does a total vertical and horizontal flip 

                                //need to just flip in rows (bottom row to top)





                                //Array.Reverse( decompressed );

                                Texture2D texture = new Texture2D( (int)_chunk.Width, (int)_chunk.Height, TextureFormat.RGBA32, false); // end false is the mip maps 
                                texture.LoadRawTextureData(flipped);
                                texture.Apply();

                                byte[] _bytes = texture.EncodeToPNG();

//                                byte[] _bytes = destTex.EncodeToPNG();



                                //ImageUtils.LoadDXT(byte[] ImageData, uint Width, uint Height, ImageUtils.PixelFormat PixelFormat, out bool Alpha);
/*
                                Texture2D texture = new Texture2D( (int)_chunk.Width, (int)_chunk.Height, TextureFormat.DXT1, false);

                                texture.LoadRawTextureData( _chunk_image_data.Data );
                                texture.Apply();
*/                                
/* set it as the texture * /
                                testMaterial.mainTexture = destTex;
/* */

/* use another texture to convert
                                //decompress via another texture
                                Color32[] pixels = texture.GetPixels32();

                                Texture2D destTex = new Texture2D(texture.width, texture.height);
                                destTex.SetPixels32(pixels);
                                destTex.Apply();
                                byte[] _bytes = destTex.EncodeToPNG();
*/

                                //byte[] _bytes = texture.EncodeToPNG();

/* use a render texture to convert
                                RenderTexture renderTex = RenderTexture.GetTemporary(
                                            source.width,
                                            source.height,
                                            0,
                                            RenderTextureFormat.Default,
                                            RenderTextureReadWrite.Linear);

                                Graphics.Blit(source, renderTex);
                                RenderTexture previous = RenderTexture.active;
                                RenderTexture.active = renderTex;
                                Texture2D readableText = new Texture2D(source.width, source.height);
                                readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
                                readableText.Apply();
                                RenderTexture.active = previous;
                                RenderTexture.ReleaseTemporary(renderTex);
                                

                                Texture2D decopmpresseTex = texture.DeCompress();
                                //Object.Destroy(texture);

                                byte[] _bytes = decopmpresseTex.EncodeToPNG();
                                //Object.Destroy(decopmpresseTex);


// Save file 


                                string datap = Application.dataPath; // full path to assets  /User/Documents/Unity/ProjectName/Assets
                                string savefilepath = "RoadRageExport" + Path.DirectorySeparatorChar + "Textures" + Path.DirectorySeparatorChar + _chunk_image.Name + ".png"; 
                                string fullfilepath = datap + Path.DirectorySeparatorChar + savefilepath;
                                string assetDBpath = "Assets" + Path.DirectorySeparatorChar + savefilepath;

                                if( System.IO.File.Exists(fullfilepath)){
                                    AssetDatabase.DeleteAsset( assetDBpath );
                                }

                                System.IO.File.WriteAllBytes(fullfilepath, _bytes);
                                
                                //DebugManager.Log($"Saved to :{fullfilepath} dp: {datap}",5);
                                //AssetDatabase.CreateAsset(destTex, assetDBpath);
                                //AssetDatabase.SaveAssets();
                                //Selection.activeObject = destTex;



                            }

                            
                            

                        }

                    }

                }
            }
        }
    }

/*
        if( true ){ //_chunk.Format == Formats.DXT1 ){
            
            DebugManager.Log("DXT1 " + _chunk.Width.ToString() + "x" + _chunk.Height.ToString() + " TextureType " + _chunk.TextureType.ToString() , 5);
            Texture2D texture = new Texture2D( (int)_chunk.Width, (int)_chunk.Height, TextureFormat.DXT1, true);
            texture.LoadRawTextureData(www.bytes);
            texture.Apply();

            string savefilepath = "Assets" + Path.DirectorySeparatorChar + "RoadRageExport" + Path.DirectorySeparatorChar + "Textures" + Path.DirectorySeparatorChar + "MyTexture.dxt";
            if( File.Exists(savefilepath)){
                AssetDatabase.DeleteAsset( savefilepath )
            }
            AssetDatabase.CreateAsset(texture, savefilepath);
            AssetDatabase.SaveAssets();
            Selection.activeObject = texture;
       
        }else{
            DebugManager.Log("Not DXT1" + _chunk.Width.ToString() + "x" + _chunk.Height.ToString(), 5);
        }
        /*
         // texture Texture2D(int width, int height, TextureFormat textureFormat = TextureFormat.RGBA32, bool mipChain = true, bool linear = false);
        //texture.LoadRawTextureData(www.bytes);
        // AssetDatabase.CreateAsset(texture, "Assets/Textures/Mytexture.dxt");

    }

/*
    public void CreateAsset(){
        AssetDatabase.CreateAsset(block, savefilepath);
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = block;
    }
*/

}
