using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Utilities.UTask
{
    public readonly struct UTask
    {
        private readonly Task _task;
        internal UTask(Task task) { _task = task ?? Task.CompletedTask; }
        public TaskAwaiter GetAwaiter() => _task.GetAwaiter();
        public static UTask Completed => new UTask(Task.CompletedTask);
        public Task AsTask() => _task;
    }

    public readonly struct UTask<T>
    {
        private readonly Task<T> _task;
        internal UTask(Task<T> task) { _task = task ?? Task.FromResult(default(T)); }
        public TaskAwaiter<T> GetAwaiter() => _task.GetAwaiter();
        public Task<T> AsTask() => _task;
    }

    public static class UTaskEx
    {
        // -------- basics --------
        public static UTask NextFrame() => NextFrame(CancellationToken.None);
        public static UTask NextFrame(CancellationToken token)
        {
            var tcs = new TaskCompletionSource<bool>();
            UTaskRunner.AddOp(new UTaskRunner.CompleteNextFrameOp(tcs, token));
            return new UTask(tcs.Task);
        }

        public static UTask Delay(float seconds, bool unscaled = false) =>
            Delay(seconds, unscaled, CancellationToken.None);

        public static UTask Delay(float seconds, bool unscaled, CancellationToken token)
        {
            if (seconds <= 0f) return UTask.Completed;
            var tcs = new TaskCompletionSource<bool>();
            UTaskRunner.AddOp(new UTaskRunner.DelaySecondsOp(tcs, seconds, unscaled, token));
            return new UTask(tcs.Task);
        }

        public static UTask DelayFrames(int frames = 1) =>
            DelayFrames(frames, CancellationToken.None);

        public static UTask DelayFrames(int frames, CancellationToken token)
        {
            var tcs = new TaskCompletionSource<bool>();
            UTaskRunner.AddOp(new UTaskRunner.DelayFramesOp(tcs, frames, token));
            return new UTask(tcs.Task);
        }

        public static UTask WaitUntil(Func<bool> predicate) =>
            WaitUntil(predicate, CancellationToken.None);

        public static UTask WaitUntil(Func<bool> predicate, CancellationToken token)
        {
            var tcs = new TaskCompletionSource<bool>();
            UTaskRunner.AddOp(new UTaskRunner.WaitPredicateOp(tcs, predicate, negate: false, token));
            return new UTask(tcs.Task);
        }

        public static UTask WaitWhile(Func<bool> predicate) =>
            WaitWhile(predicate, CancellationToken.None);

        public static UTask WaitWhile(Func<bool> predicate, CancellationToken token)
        {
            var tcs = new TaskCompletionSource<bool>();
            UTaskRunner.AddOp(new UTaskRunner.WaitPredicateOp(tcs, predicate, negate: true, token));
            return new UTask(tcs.Task);
        }

        // -------- threading helpers --------
        public static UTask SwitchToMainThread()
        {
            UTaskRunner.Ensure();
            if (System.Threading.Thread.CurrentThread.ManagedThreadId == UTaskRunner.MainThreadId)
                return UTask.Completed;

            var tcs = new TaskCompletionSource<bool>();
            UTaskRunner.MainThreadContext.Post(_ => tcs.TrySetResult(true), null);
            return new UTask(tcs.Task);
        }

        public static UTask<T> RunOnThreadPool<T>(Func<T> func) => new UTask<T>(Task.Run(func));
        public static UTask RunOnThreadPool(Action action) => new UTask(Task.Run(action));

        // -------- combinators / timeouts --------
        public static UTask WhenAll(params UTask[] tasks)
        {
            var arr = new Task[tasks.Length];
            for (int i = 0; i < tasks.Length; i++) arr[i] = tasks[i].AsTask();
            return new UTask(Task.WhenAll(arr));
        }

        public static UTask<int> WhenAny(params UTask[] tasks)
        {
            var arr = new Task[tasks.Length];
            for (int i = 0; i < tasks.Length; i++) arr[i] = tasks[i].AsTask();
            var idxTask = Task.WhenAny(arr).ContinueWith(t =>
            {
                var completed = t.Result;
                for (int i = 0; i < arr.Length; i++)
                    if (arr[i] == completed) return i;
                return -1;
            });
            return new UTask<int>(idxTask);
        }

        public static UTask WithTimeout(this UTask task, float seconds)
        {
            if (seconds <= 0f) return task;
            var timeout = Delay(seconds).AsTask();
            var wrapped = Task.WhenAny(task.AsTask(), timeout)
                .ContinueWith(async t =>
                {
                    if (t.Result == timeout) throw new TimeoutException("UTask timeout.");
                    await task.AsTask();
                }).Unwrap();
            return new UTask(wrapped);
        }
    }
}
