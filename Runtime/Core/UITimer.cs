
using System;
using UnityEngine;

namespace Feif.UIFramework
{
    public class UITimer
    {
        private float progress = 0;
        private Action callback = null;

        public float Delay { get; private set; }
        public bool IsCancel { get; private set; }
        public bool IsLoop { get; private set; }

        public UITimer(float delay, Action callback, bool isLoop = false)
        {
            Delay = delay;
            IsLoop = isLoop;
            this.callback = callback;
        }

        public void Update()
        {
            progress += Time.deltaTime;
            if (!IsCancel && progress >= Delay)
            {
                callback?.Invoke();
                progress = 0;
                if (!IsLoop) Cancel();
            }
        }

        public void Cancel()
        {
            IsCancel = true;
        }
    }
}