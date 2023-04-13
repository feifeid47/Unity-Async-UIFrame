using System.Threading.Tasks;
using UnityEngine.Networking;
using UnityEngine;

namespace Feif
{
    public static class NetworkResources
    {
        public static Task<TextAsset> LoadTextAssetAsync(string uri)
        {
            var completionSource = new TaskCompletionSource<TextAsset>();
            if (string.IsNullOrEmpty(uri))
            {
                completionSource.SetResult(null);
                return completionSource.Task;
            }
            var request = UnityWebRequest.Get(uri);
            request.SendWebRequest().completed += _ =>
            {
                if (request.responseCode != 200)
                {
                    completionSource.SetResult(null);
                    request.Dispose();
                    return;
                }
                if (request.downloadHandler.text == null)
                {
                    completionSource.SetResult(null);
                    request.Dispose();
                    return;
                }
                completionSource.SetResult(new TextAsset(request.downloadHandler.text));
                request.Dispose();
            };
            return completionSource.Task;
        }

        public static Task<Texture2D> LoadTexture2DAsync(string uri)
        {
            var completionSource = new TaskCompletionSource<Texture2D>();
            if (string.IsNullOrEmpty(uri))
            {
                completionSource.SetResult(null);
                return completionSource.Task;
            }
            var request = UnityWebRequestTexture.GetTexture(uri);
            request.SendWebRequest().completed += _ =>
            {
                if (request.responseCode != 200)
                {
                    completionSource.SetResult(null);
                    request.Dispose();
                    return;
                }
                var texture = (request.downloadHandler as DownloadHandlerTexture).texture;
                if (texture == null)
                {
                    completionSource.SetResult(null);
                    request.Dispose();
                    return;
                }
                completionSource.SetResult(texture);
                request.Dispose();
            };
            return completionSource.Task;
        }

        public static Task<Sprite> LoadSpriteAsync(string uri)
        {
            var completionSource = new TaskCompletionSource<Sprite>();
            if (string.IsNullOrEmpty(uri))
            {
                completionSource.SetResult(null);
                return completionSource.Task;
            }
            var request = UnityWebRequestTexture.GetTexture(uri);
            request.SendWebRequest().completed += _ =>
            {
                if (request.responseCode != 200)
                {
                    completionSource.SetResult(null);
                    request.Dispose();
                    return;
                }
                var texture = (request.downloadHandler as DownloadHandlerTexture).texture;
                if (texture == null)
                {
                    completionSource.SetResult(null);
                    request.Dispose();
                    return;
                }
                var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                completionSource.SetResult(sprite);
                request.Dispose();
            };
            return completionSource.Task;
        }

        public static Task<AudioClip> LoadAudioClipAsync(string uri, AudioType audioType)
        {
            var completionSource = new TaskCompletionSource<AudioClip>();
            if (string.IsNullOrEmpty(uri))
            {
                completionSource.SetResult(null);
                return completionSource.Task;
            }
            var request = UnityWebRequestMultimedia.GetAudioClip(uri, audioType);
            request.SendWebRequest().completed += _ =>
            {
                if (request.responseCode != 200)
                {
                    completionSource.SetResult(null);
                    request.Dispose();
                    return;
                }
                var audioClip = (request.downloadHandler as DownloadHandlerAudioClip).audioClip;
                if (audioClip == null)
                {
                    completionSource.SetResult(null);
                    request.Dispose();
                    return;
                }
                completionSource.SetResult(audioClip);
                request.Dispose();
            };
            return completionSource.Task;
        }

        public static Task<AssetBundle> LoadAssetBundleAsync(string uri)
        {
            var completionSource = new TaskCompletionSource<AssetBundle>();
            if (string.IsNullOrEmpty(uri))
            {
                completionSource.SetResult(null);
                return completionSource.Task;
            }
            var request = UnityWebRequestAssetBundle.GetAssetBundle(uri);
            request.SendWebRequest().completed += _ =>
            {
                if (request.responseCode != 200)
                {
                    completionSource.SetResult(null);
                    request.Dispose();
                    return;
                }
                var assetBundle = (request.downloadHandler as DownloadHandlerAssetBundle).assetBundle;
                if (assetBundle == null)
                {
                    completionSource.SetResult(null);
                    request.Dispose();
                    return;
                }
                completionSource.SetResult(assetBundle);
                request.Dispose();
            };
            return completionSource.Task;
        }
    }
}