#if UNITY_RECORDER
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Mapbox.BaseModule.Data.Vector2d;
using Mapbox.BaseModule.Map;
using Mapbox.BaseModule.Utilities;
using Mapbox.Example.Scripts.Map;
using Mapbox.Example.Scripts.MapInput;
using Mapbox.MapDebug.Scripts.Logging;
using Mapbox.UnityMapService;
using Mapbox.UnityMapService.TileProviders;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.Recorder;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Debug = UnityEngine.Debug;

namespace Mapbox.BaseModuleTests.PlayModeTests
{
    internal class SceneTests
    {
        private string TestScreenshotsFolder = "TestScreenshots";
        private int _sceneIndex = -1;

        private Dictionary<string, LatitudeLongitude> _latitudeLongitudes = new Dictionary<string, LatitudeLongitude>()
        {
            { "Chicago", new LatitudeLongitude(41.8336152, -87.896769) },
            { "Helsinki", new LatitudeLongitude(60.1711822, 24.9422687) },
            { "Berlin", new LatitudeLongitude(52.5165309, 13.3777779) },
            { "Paris", new LatitudeLongitude(48.8584642, 2.3097613) },
            { "SF", new LatitudeLongitude(37.7873314, -122.4080894) },
            { "Mountain", new LatitudeLongitude(46.85264466, -121.75710321) },
        };

        private static IEnumerable LatLngTestSource
        {
            get
            {
                yield return new TestCaseData("WorldMapScene", 41.8336152, -87.896769).Returns(null);
                yield return new TestCaseData("WorldMapScene", 60.1711822, 24.9422687).Returns(null);
                yield return new TestCaseData("WorldMapScene", 52.5165309, 13.3777779).Returns(null);
                yield return new TestCaseData("WorldMapScene", 48.8584642, 2.3097613).Returns(null);
                yield return new TestCaseData("WorldMapScene", 37.7873314, -122.4080894).Returns(null);
                yield return new TestCaseData("WorldMapScene", 46.85264466, -121.75710321).Returns(null);
                yield return new TestCaseData("TerrainScene", 41.8336152, -87.896769).Returns(null);
                yield return new TestCaseData("TerrainScene", 60.1711822, 24.9422687).Returns(null);
                yield return new TestCaseData("TerrainScene", 52.5165309, 13.3777779).Returns(null);
                yield return new TestCaseData("TerrainScene", 48.8584642, 2.3097613).Returns(null);
                yield return new TestCaseData("TerrainScene", 37.7873314, -122.4080894).Returns(null);
                yield return new TestCaseData("TerrainScene", 46.85264466, -121.75710321).Returns(null);
            }
        }

        [UnityTest]
        [TestCaseSource(nameof(LatLngTestSource))]
        public IEnumerator GroupTest(string sceneName, double lat, double lon)
        {
            yield return TestScene(sceneName, lat, lon);
        }
        
        //[UnityTest]
        // public IEnumerator TestAllScenes()
        // {
        //     string[] files = Directory.GetFiles(Path.Combine(Application.dataPath + "/Mapbox/BaseModuleTests/PlayModeTests/Scenes/"), "*.unity");
        //     foreach(string file in files)
        //     {
        //         var name = Path.GetFileNameWithoutExtension(file);
        //         foreach (var location in _latitudeLongitudes)
        //         {
        //             yield return TestScene(name, location.Key);
        //         }
        //     }
        // }

        public IEnumerator TestScene(string sceneName, double lat, double lon)
        {
            yield return SceneManager.LoadSceneAsync(sceneName);
            yield return ProcessScene(sceneName, lat, lon);
            yield return SceneManager.UnloadSceneAsync(sceneName);
        }
        
        private IEnumerator ProcessScene(string sceneName, double lat, double lon)
        {
            var camera = Camera.main;
            var mapCore = GameObject.FindObjectOfType<MapboxMapBehaviour>();
            MapboxMap _map = null;
            if (mapCore.InitializationStatus == InitializationStatus.WaitingForInitialization)
            {
                mapCore.Initialized += map =>
                {
                    _map = map;
                };
                mapCore.MapInformation.SetLatitudeLongitude(new LatitudeLongitude(lat, lon));
                mapCore.Initialize();
            }
            else
            {
                _map = mapCore.MapboxMap;
            }
        
            while(mapCore.InitializationStatus < InitializationStatus.ReadyForUpdates) yield return null;

            yield return new WaitForSeconds(2);
        
            Assert.IsNotEmpty(_map.TileCover.Tiles);
            Assert.NotZero(_map.TileCover.Tiles.Count);

            var path = TakeScreenshot(string.Format("{0}", string.Format("{0}_{1}_{2}", sceneName, lat.ToString("F"), lon.ToString("F"))), camera);
            Assert.IsNotEmpty(path);
        }


        [UnityTest]
        public IEnumerator ZoomInWithCache()
        {
            yield return ZoomInTest("DebugWorldMapScene");
        }
        
        [UnityTest]
        public IEnumerator ZoomOutWithCache()
        {
            yield return ZoomOutTest("DebugWorldMapScene");
        }
        
        [UnityTest]
        public IEnumerator ZoomInNoCache()
        {
            yield return ZoomInTest("DebugWorldMapSceneNoCache");
        }
        
        [UnityTest]
        public IEnumerator ZoomOutNoCache()
        {
            yield return ZoomOutTest("DebugWorldMapSceneNoCache");
        }
        
        private IEnumerator ZoomOutTest(string sceneName)
        {
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            
            var mapCore = GameObject.FindObjectOfType<LoggingMapBehaviour>();
            MapboxMap _map = null;
            var cameraController = GameObject.FindObjectOfType<Moving3dCameraBehaviour>();
            if (mapCore.InitializationStatus == InitializationStatus.WaitingForInitialization)
            {
                mapCore.Initialized += map => { _map = map; };
                mapCore.MapInformation.SetInformation(null, 16);
                mapCore.Initialize();
            }
            else
            {
                _map = mapCore.MapboxMap;
            }
            while(mapCore.InitializationStatus < InitializationStatus.ReadyForUpdates) yield return null;

            yield return new WaitForSeconds(1);

            RecorderController _testRecorderController;
            var controllerSettings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
            _testRecorderController = new RecorderController(controllerSettings);
            var videoRecorder = ScriptableObject.CreateInstance<MovieRecorderSettings>();
            videoRecorder.name = "MapboxDebugVideoRecorder";
            videoRecorder.Enabled = true;
            videoRecorder.VideoBitRateMode = VideoBitrateMode.High;
            videoRecorder.AudioInputSettings.PreserveAudio = false;
            videoRecorder.OutputFile = Path.Combine(GetFolderName(TestContext.CurrentContext.Test.MethodName), "zoomVideo_" + DateTime.Now.ToString("ddMMyyy_hhmm"));
            
            controllerSettings.AddRecorderSettings(videoRecorder);
            controllerSettings.FrameRate = 60;

            RecorderOptions.VerboseMode = false;
            _testRecorderController.PrepareRecording();
            _testRecorderController.StartRecording();
            
            while (mapCore.MapInformation.Zoom > 6)
            {
                cameraController.Zoom(mapCore.MapInformation, -1 * Time.deltaTime);
                yield return null;
            }
            
            _testRecorderController?.StopRecording();
            yield return SceneManager.UnloadSceneAsync(sceneName);
        }
        
        private IEnumerator ZoomInTest(string sceneName)
        {
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            
            var mapCore = GameObject.FindObjectOfType<LoggingMapBehaviour>();
            MapboxMap _map = null;
            var cameraController = GameObject.FindObjectOfType<Moving3dCameraBehaviour>();
            if (mapCore.InitializationStatus == InitializationStatus.WaitingForInitialization)
            {
                mapCore.Initialized += map => { _map = map; };
                mapCore.MapInformation.SetInformation(null, 8);
                mapCore.Initialize();
            }
            else
            {
                _map = mapCore.MapboxMap;
            }
            while(mapCore.InitializationStatus < InitializationStatus.ReadyForUpdates) yield return null;

            yield return new WaitForSeconds(1);

            RecorderController _testRecorderController;
            var controllerSettings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
            _testRecorderController = new RecorderController(controllerSettings);
            var videoRecorder = ScriptableObject.CreateInstance<MovieRecorderSettings>();
            videoRecorder.name = "MapboxDebugVideoRecorder";
            videoRecorder.Enabled = true;
            videoRecorder.VideoBitRateMode = VideoBitrateMode.High;
            videoRecorder.AudioInputSettings.PreserveAudio = false;
            videoRecorder.OutputFile = Path.Combine(GetFolderName(TestContext.CurrentContext.Test.MethodName), "zoomVideo_" + DateTime.Now.ToString("ddMMyyy_hhmm"));
            
            controllerSettings.AddRecorderSettings(videoRecorder);
            controllerSettings.FrameRate = 60;

            RecorderOptions.VerboseMode = false;
            _testRecorderController.PrepareRecording();
            _testRecorderController.StartRecording();
            
            while (mapCore.MapInformation.Zoom < 16.9)
            {
                cameraController.Zoom(mapCore.MapInformation, 1 * Time.deltaTime);
                yield return null;
            }

            yield return new WaitForSeconds(5);
            
            _testRecorderController?.StopRecording();
            yield return SceneManager.UnloadSceneAsync(sceneName);
        }
        
        private string TakeScreenshot(string sceneName, Camera camera)
        {
            var rt = new RenderTexture(1920, 1080, 24);
            camera.targetTexture = rt;
            var screenShot = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
            camera.Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
            camera.targetTexture = null;
            RenderTexture.active = null;
            GameObject.Destroy(rt);
            byte[] bytes = screenShot.EncodeToJPG();
            
            var path = SaveFile(bytes, sceneName);
            return path;
        }

        private string GetFolderName(string currentMethodName) => Path.Combine(Application.persistentDataPath, TestScreenshotsFolder, currentMethodName);
        
        private string SaveFile(byte[] bytes, string sceneName)
        {
            var currentMethodName = TestContext.CurrentContext.Test.MethodName;
            var dirPath = GetFolderName(currentMethodName);
            if(!Directory.Exists(dirPath)) {
                Directory.CreateDirectory(dirPath);
            }

            var fileName = string.Format("{0}.jpg", sceneName);
            var fullPath = Path.Combine(dirPath, fileName);
            File.WriteAllBytes(fullPath, bytes);
            return fullPath;
        }
    }
}
#endif

