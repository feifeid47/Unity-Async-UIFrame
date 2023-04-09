using System.Threading.Tasks;
using UnityEngine;

namespace Feif.UI
{
    public class UIWindowEffect : MonoBehaviour
    {
        private bool isPlayingOpen = false;
        private bool isPlayingClose = false;
        private float duration = 0.15f;
        private float currentPosition = 0;
        private TaskCompletionSource<object> completionSource;

        // 播放打开Window动效
        public void PlayOpen()
        {
            transform.localScale = Vector3.zero;
            isPlayingOpen = true;
            currentPosition = 0;
        }

        // 播放关闭Window动效
        public Task PlayClose()
        {
            transform.localScale = Vector3.one;
            isPlayingClose = true;
            currentPosition = 0;
            completionSource = new TaskCompletionSource<object>();
            return completionSource.Task;
        }

        private void OnPlayingOpen(float position)
        {
            transform.localScale = Vector3.one * position / duration;
        }

        private void OnPlayingClose(float position)
        {
            transform.localScale = Vector3.one * (1 - (position / duration));
        }

        private void Update()
        {
            if (isPlayingOpen)
            {
                currentPosition += Time.deltaTime;
                OnPlayingOpen(Mathf.Clamp(currentPosition, 0, duration));
            }
            if (isPlayingClose)
            {
                currentPosition += Time.deltaTime;
                OnPlayingClose(Mathf.Clamp(currentPosition, 0, duration));
            }
            if (currentPosition >= duration)
            {
                currentPosition = 0;
                isPlayingOpen = isPlayingClose = false;
                completionSource?.TrySetResult(null);
            }
        }
    }
}