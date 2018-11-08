using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
#if UNITY_EDITOR_WIN
using System.Drawing;
#endif
namespace GifConvertEditor
{
    public class GifConvertWindow : EditorWindow
    {

        private string OutputFolder = "Assets/GifConvert/GIF";
        private string IntputFolder = "Assets/GifConvert/UGIF";

        public static string OuputTextureSearchFolder = "GIF";
        private string SplitExtension = "_";

       

        public static int CompressionQuality = 100;
        public static bool MipmapEnabled = false;
        public static TextureImporterFormat selectFormatEnum = TextureImporterFormat.ETC2_RGBA8;
        public static SpriteImportMode spriteImportMode = SpriteImportMode.Single;

        public static string Platform = "Android";
        private string[] m_platform = new string[] { "Standalone", "Android"};
        private int platformSelect = 0;

        public static bool OverrideTexture = false;
        public static bool CompressTexture = false;

        public static int TextureSize = 1024;
        public string[] m_textureSize = new string[] { "32", "64", "128", "256", "512", "1024", "2048", "4096", "8192" };
        public int[] i_textureSize = new int[] { 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192 };


        [MenuItem("Window/GifConvert")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            //ConvertWindow convertSettings = EditorWindow.CreateInstance<ConvertWindow>();
            //convertSettings.Init();

            GifConvertWindow window = (GifConvertWindow)EditorWindow.GetWindow(typeof(GifConvertWindow));
            window.Show();

        }

        void OnGUI()
        {
            string sourcePath = Application.dataPath + IntputFolder;

            GUILayout.Space(15);

            GUILayout.Label("Settings", EditorStyles.boldLabel);

            GUILayout.Space(15);

            #region OutputFolder Selection

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Input GIF Path");
            GUIStyle outPtBtn = new GUIStyle(GUI.skin.button);
            outPtBtn.alignment = TextAnchor.MiddleLeft;
            if (GUILayout.Button(IntputFolder.Length == 0 ? "Click To Select Path" : IntputFolder, outPtBtn))
            {
                string returnPath = EditorUtility.OpenFolderPanel("Select Input Directory", IntputFolder.Length == 0 ? Application.dataPath : Application.dataPath, "");
                Debug.Log("Return pATH : " + returnPath);
                if (returnPath.Length > 0)
                {
                    if (returnPath.Contains(Application.dataPath))
                    {
                        returnPath = returnPath.Substring(Application.dataPath.Length);
                        returnPath = "Assets" + returnPath;
                    }
                    IntputFolder = returnPath;
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Output Folder Path");
            GUIStyle alignLeft = new GUIStyle(GUI.skin.button);
            alignLeft.alignment = TextAnchor.MiddleLeft;
            if (GUILayout.Button(OutputFolder.Length == 0 ? "Click To Select Path" : OutputFolder, alignLeft))
            {
                string returnPath = EditorUtility.OpenFolderPanel("Select Output Folder", OutputFolder.Length == 0 ? Application.dataPath : Application.dataPath, "");
               
                if (returnPath.Length > 0)
                {
                    if (returnPath.Contains(Application.dataPath))
                    {
                        returnPath = returnPath.Substring(Application.dataPath.Length);
                        returnPath = "Assets" + returnPath;
                    }
                    OutputFolder = returnPath;
                }

                //Set Compress texture search folder
                var allStr = OutputFolder.Split('/');
                OuputTextureSearchFolder = allStr[allStr.Length-1];
            }
            GUILayout.EndHorizontal();



            #endregion

            SplitExtension = EditorGUILayout.TextField("SplitExtension", SplitExtension);
            
            CompressTexture = EditorGUILayout.Toggle("CompressTexture" ,CompressTexture);
            if (CompressTexture == true)
            {
                OverrideTexture = EditorGUILayout.Toggle("Override for Platfrom", OverrideTexture);
                platformSelect = EditorGUILayout.Popup("Platform", platformSelect, m_platform);
                Platform = m_platform[platformSelect];
                spriteImportMode = (SpriteImportMode)EditorGUILayout.EnumPopup("Sprite Mode", spriteImportMode);
                selectFormatEnum = (TextureImporterFormat)EditorGUILayout.EnumPopup("Compression mode", selectFormatEnum);
                CompressionQuality = EditorGUILayout.IntSlider("CompressionQuality", CompressionQuality, 0, 100);
                TextureSize = EditorGUILayout.IntPopup("Texture Size", TextureSize, m_textureSize, i_textureSize);
                MipmapEnabled = EditorGUILayout.Toggle("MipmapEnabled", MipmapEnabled);
            }
            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Cancel"))
            {

                Close();
            }
            else if (GUILayout.Button("Convert"))
            {

#if UNITY_EDITOR_WIN

                //path  
                string fullPath = Application.dataPath + IntputFolder.Substring("Assets".Length);

                if (Directory.Exists(fullPath))
                {

                    DirectoryInfo direction = new DirectoryInfo(fullPath);
                    FileInfo[] files = direction.GetFiles("*", SearchOption.AllDirectories);

                    Debug.Log("InputFolder :" + IntputFolder + " Convert Count : " + files.Length / 2); //cause meta file
                    Debug.Log("OutpuFolder :" + OutputFolder);

                    for (int i = 0; i < files.Length; i++)
                    {
                        if (files[i].Name.EndsWith(".gif"))
                        {

                            sourcePath = fullPath + "/" + files[i].Name;


                            var outputFullFolderPath = Application.dataPath + OutputFolder.Substring("Assets".Length) + "/" + files[i].Name.Substring(0, files[i].Name.Length - 4);

                            if (!Directory.Exists(outputFullFolderPath))
                            {
                                AssetDatabase.CreateFolder(OutputFolder, files[i].Name.Substring(0, files[i].Name.Length - 4));
                            }

                            using (Image gifImg = Image.FromFile(sourcePath))
                            {
                                System.Drawing.Imaging.FrameDimension dimension = new System.Drawing.Imaging.FrameDimension(gifImg.FrameDimensionsList.First());


                                for (int k = 0; k < gifImg.GetFrameCount(dimension); k++)
                                {

                                    int finalWidth = gifImg.Width;
                                    int finalHeight = gifImg.Height;

                                    using (Bitmap finalSprite = new Bitmap(finalWidth, finalHeight))
                                    {

                                        using (System.Drawing.Graphics canvas = System.Drawing.Graphics.FromImage(finalSprite))
                                        {

                                            canvas.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                                            canvas.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                                            gifImg.SelectActiveFrame(dimension, k);
                                            using (Bitmap frame = new Bitmap(gifImg, finalWidth, finalHeight))
                                            {

                                                canvas.DrawImage(frame, 0, 0);
                                            }

                                            canvas.Save();

                                            var tmpPath = outputFullFolderPath + "/" + files[i].Name;
                                            string targetPath = tmpPath.Substring(0, tmpPath.Length - 4) + SplitExtension + k + ".png";


                                            finalSprite.Save(targetPath, System.Drawing.Imaging.ImageFormat.Png);


                                        }
                                    }
                                }

                            }

                        }

                    }

                    AssetDatabase.Refresh();

                }
#endif
                Close();
            }
            GUILayout.EndHorizontal();

        }
    }

    public class GifTextureCompress : AssetPostprocessor
    {
        
        void OnPreprocessTexture()
        {
            if (!GifConvertWindow.CompressTexture)
                return;

            string _path = assetPath;

          
            if (GifConvertWindow.OuputTextureSearchFolder.Length <= 0)
                return;
            

            if (!_path.Contains(GifConvertWindow.OuputTextureSearchFolder))
                return;


            if (_path.EndsWith(".png"))
            {
                

                TextureImporter textureImporter = (TextureImporter)assetImporter;
                UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath(textureImporter.assetPath, typeof(Texture2D));

                if (!asset)
                {
                    textureImporter.textureType = TextureImporterType.Sprite;
                    textureImporter.spriteImportMode = SpriteImportMode.Single;
                    textureImporter.mipmapEnabled = GifConvertWindow.MipmapEnabled;

                    TextureImporterPlatformSettings t = new TextureImporterPlatformSettings
                    {
                        format = GifConvertWindow.selectFormatEnum,
                        overridden = GifConvertWindow.OverrideTexture,
                        name = GifConvertWindow.Platform,
                        maxTextureSize = GifConvertWindow.TextureSize,
                        compressionQuality = (int)GifConvertWindow.CompressionQuality
                    };

                    textureImporter.SetPlatformTextureSettings(t);

                }
                else
                {
                    
                }
                
            }
        }


    }


}