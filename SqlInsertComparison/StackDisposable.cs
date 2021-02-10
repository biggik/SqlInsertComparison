using System;
using System.Collections.Generic;

namespace SqlInsertComparison
{
    /// <summary>
    /// A stacked-disposable pattern. Pushed disposables are pop-ed off the stack and disposed
    /// when the StackDisposable is disposed
    /// </summary>
    public class StackDisposable : IDisposable
    {
        private Stack<IDisposable> stack = new Stack<IDisposable>();
        public StackDisposable()
        {
            
        }

        public void Push(IDisposable disposable)
        {
            if (disposable != null)
            {
                stack.Push(disposable);
            }
        }
        
        public void Dispose()
        {
            while (stack.TryPop(out IDisposable disposable))
            {
                disposable.Dispose();
            }
        }
    }
}