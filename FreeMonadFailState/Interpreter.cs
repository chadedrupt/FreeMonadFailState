using LanguageExt;
using static LanguageExt.Prelude;
using static FreeMonadFailState.Prelude;

namespace FreeMonadFailState
{
    public static class Interpreter
    {

        public static TryAsync<A> Interpret<A>(this Free<A> ma, InterpreterProps props) =>
            TryAsync(async () =>
            {
                var current = Node(ma);
                while (!current.IsLeaf)
                {
                    current = await InternalInterpret(current.Next, props)
                        .IfFailThrow();
                }
                return current.Value;
            });

        static TryAsync<Node<A>> InternalInterpret<A>(this Free<A> action, InterpreterProps props) =>
            action switch
            {
                Free<A>.Return a => Return(a),
                Free<A>.Fail a => Fail(a),
                Free<A>.SomeAsyncThing a => SomeAsyncThing(a, props),
                _ => default
            };

        static TryAsync<Node<A>> Return<A>(Free<A>.Return action) =>
            TryAsync(Node(action.Value));

        static TryAsync<Node<A>> Fail<A>(Free<A>.Fail action) =>
            TryAsync<Node<A>>(action.Exception);

        static TryAsync<Node<A>> SomeAsyncThing<A>(Free<A>.SomeAsyncThing action, InterpreterProps props) =>
            props.DoSomeAsyncThing(action.Message)
                .BiMap(
                    Succ: _ => Node(action.Next(unit)),
                    Fail: e => Node(action.FailNext(e))
                );

    }
}
