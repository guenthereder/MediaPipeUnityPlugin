// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Mediapipe.Unity
{
  public class StartSceneController : MonoBehaviour
  {
    private const string _TAG = nameof(Bootstrap);

    [SerializeField] private Image _screen;
    [SerializeField] private GameObject _consolePrefab;

    private IEnumerator Start()
    {
      LoadLibrary();

      var _ = Instantiate(_consolePrefab, _screen.transform);

      var bootstrap = GetComponent<Bootstrap>();

      yield return new WaitUntil(() => bootstrap.isFinished);

      DontDestroyOnLoad(gameObject);

      Logger.LogInfo(_TAG, "Loading the first scene...");
      var sceneLoadReq = SceneManager.LoadSceneAsync(1);
      yield return new WaitUntil(() => sceneLoadReq.isDone);
    }

    public void LoadLibrary()
    {
#if UNITY_EDITOR_LINUX
    var path = Path.Combine("Packages", "com.github.homuler.mediapipe", "Runtime", "Plugins", "libmediapipe_c.so");
#elif UNITY_STANDALONE_LINUX
    var path = Path.Combine(Application.dataPath, "Plugins", "libmediapipe_c.so");
#elif UNITY_EDITOR_OSX
      var path = Path.Combine("Packages", "com.github.homuler.mediapipe", "Runtime", "Plugins", "libmediapipe_c.dylib");
#elif UNITY_STANDALONE_OSX || UNITY_IOS
    var path = Path.Combine(Application.dataPath, "Plugins", "libmediapipe_c.dylib");
#endif

      var handle = dlopen(path, 2);

      if (handle != IntPtr.Zero)
      {
        // Success
        var result = dlclose(handle);

        if (result != 0)
        {
          Debug.LogError($"Failed to unload {path}");
        }
      }
      else
      {
        Debug.LogError($"Failed to load {path}: {Marshal.GetLastWin32Error()}");
        var error = Marshal.PtrToStringAnsi(dlerror());
        // TODO: release memory

        if (error != null)
        {
          Debug.LogError(error);
        }
      }
    }

    [DllImport("dl", SetLastError = true, ExactSpelling = true)]
    private static extern IntPtr dlopen(string name, int flags);

    [DllImport("dl", ExactSpelling = true)]
    private static extern IntPtr dlerror();

    [DllImport("dl", ExactSpelling = true)]
    private static extern int dlclose(IntPtr handle);

  }
}
