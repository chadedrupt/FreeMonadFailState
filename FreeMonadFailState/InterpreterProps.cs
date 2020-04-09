using System;
using LanguageExt;

namespace FreeMonadFailState
{
    [Record]
    public partial struct InterpreterProps
    {
        public readonly Func<string, TryAsync<Unit>> DoSomeAsyncThing;
    }
}
