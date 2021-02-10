using System;

namespace SqlInsertComparison
{
    /// <summary>
    /// A disposable implementation that takes an action and runs it when Dispose is called
    /// </summary>
    public class ActionDisposable : IDisposable
    {
        private readonly Action _action;

        public ActionDisposable(Action action)
        {
            _action = action;
        }

        public void Dispose()
        {
            _action?.Invoke();
        }
    }
}