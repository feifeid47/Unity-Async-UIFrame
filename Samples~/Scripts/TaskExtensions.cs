using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Feif.Extensions
{
    public static class TaskExtensions
    {
#if !USING_UNITASK
        public static TaskAwaiter<UnityWebRequest> GetAwaiter(this UnityWebRequestAsyncOperation opt)
        {
            var completionSource = new TaskCompletionSource<UnityWebRequest>();
            opt.completed += _ => completionSource.SetResult(opt.webRequest);
            return completionSource.Task.GetAwaiter();
        }

        public static TaskAwaiter GetAwaiter(this TimeSpan time)
        {
            return Task.Delay(time).GetAwaiter();
        }

        public static TaskAwaiter<UnityEngine.Object> GetAwaiter(this ResourceRequest request)
        {
            var completionSource = new TaskCompletionSource<UnityEngine.Object>();
            request.completed += _ =>
            {
                completionSource.SetResult(request.asset);
            };
            return completionSource.Task.GetAwaiter();
        }
#endif
    }
}