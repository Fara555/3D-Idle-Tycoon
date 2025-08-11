using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Utilities.UTask
{
    [DefaultExecutionOrder(-10000)]
    [DisallowMultipleComponent]
    public class UTaskRunner : MonoBehaviour
    {
        private static UTaskRunner _instance;
        public static SynchronizationContext MainThreadContext { get; private set; }
        public static int MainThreadId { get; private set; }

        private readonly List<IDelayedOp> _ops = new List<IDelayedOp>(256);
        private readonly List<IDelayedOp> _opsAddBuffer = new List<IDelayedOp>(64);

        public static void Ensure()
        {
            if (_instance != null) return;
            var go = new GameObject("[UTaskRunner]");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<UTaskRunner>();
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            MainThreadContext = SynchronizationContext.Current ?? new SynchronizationContext();
            MainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
        }

        private void Update()
        {
            if (_opsAddBuffer.Count > 0)
            {
                _ops.AddRange(_opsAddBuffer);
                _opsAddBuffer.Clear();
            }

            float dt = Time.deltaTime;
            float udt = Time.unscaledDeltaTime;

            for (int i = _ops.Count - 1; i >= 0; i--)
            {
                var op = _ops[i];
                if (op.Tick(dt, udt))
                {
                    _ops.RemoveAt(i);
                }
            }
        }

        internal static void AddOp(IDelayedOp op)
        {
            Ensure();
            _instance._opsAddBuffer.Add(op);
        }

        // -------- internal operation interfaces/impls --------
        internal interface IDelayedOp
        {
            /// <summary>Return true when finished.</summary>
            bool Tick(float dt, float unscaledDt);
        }

        internal abstract class CancellableOpBase : IDelayedOp
        {
            protected readonly TaskCompletionSource<bool> Tcs;
            protected readonly CancellationToken Token;

            protected CancellableOpBase(TaskCompletionSource<bool> tcs, CancellationToken token)
            {
                Tcs = tcs;
                Token = token;
            }

            public bool Tick(float dt, float udt)
            {
                if (Token.IsCancellationRequested)
                {
                    Tcs.TrySetCanceled(Token);
                    return true;
                }
                return TickInternal(dt, udt);
            }

            protected abstract bool TickInternal(float dt, float udt);
        }

        internal sealed class DelaySecondsOp : CancellableOpBase
        {
            private float _remain;
            private readonly bool _unscaled;

            public DelaySecondsOp(TaskCompletionSource<bool> tcs, float seconds, bool unscaled, CancellationToken token)
                : base(tcs, token)
            {
                _remain = Mathf.Max(0f, seconds);
                _unscaled = unscaled;
            }

            protected override bool TickInternal(float dt, float udt)
            {
                _remain -= _unscaled ? udt : dt;
                if (_remain <= 0f)
                {
                    Tcs.TrySetResult(true);
                    return true;
                }
                return false;
            }
        }

        internal sealed class DelayFramesOp : CancellableOpBase
        {
            private int _frames;

            public DelayFramesOp(TaskCompletionSource<bool> tcs, int frames, CancellationToken token)
                : base(tcs, token)
            {
                _frames = Mathf.Max(1, frames);
            }

            protected override bool TickInternal(float dt, float udt)
            {
                if (--_frames <= 0)
                {
                    Tcs.TrySetResult(true);
                    return true;
                }
                return false;
            }
        }

        internal sealed class WaitPredicateOp : CancellableOpBase
        {
            private readonly Func<bool> _predicate;
            private readonly bool _negate;

            public WaitPredicateOp(TaskCompletionSource<bool> tcs, Func<bool> predicate, bool negate, CancellationToken token)
                : base(tcs, token)
            {
                _predicate = predicate;
                _negate = negate;
            }

            protected override bool TickInternal(float dt, float udt)
            {
                bool ok = false;
                try { ok = _predicate?.Invoke() ?? false; }
                catch (Exception e) { Tcs.TrySetException(e); return true; }

                if (_negate) ok = !ok;
                if (ok)
                {
                    Tcs.TrySetResult(true);
                    return true;
                }
                return false;
            }
        }

        // завершается на следующем кадре (один реальный frame)
        internal sealed class CompleteNextFrameOp : CancellableOpBase
        {
            public CompleteNextFrameOp(TaskCompletionSource<bool> tcs, CancellationToken token)
                : base(tcs, token) { }

            protected override bool TickInternal(float dt, float udt)
            {
                Tcs.TrySetResult(true);
                return true;
            }
        }
    }
}
