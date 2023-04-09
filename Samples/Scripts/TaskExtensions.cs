using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace Feif
{
    public static class TaskExtensions
    {
        public static TaskAwaiter<UnityWebRequest> GetAwaiter(this UnityWebRequestAsyncOperation opt)
        {
            var completionSource = new TaskCompletionSource<UnityWebRequest>();
            opt.completed += _ => completionSource.SetResult(opt.webRequest);
            return completionSource.Task.GetAwaiter();
        }
    }
}
