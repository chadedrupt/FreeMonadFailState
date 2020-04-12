using System;
using LanguageExt;
using static LanguageExt.Prelude;

namespace FreeMonadFailState
{
    public static partial class Free
    {
        public static Free<A> Return<A>(A value) => new Free<A>.Return(value);
        public static Free<A> Fail<A>(Exception error) => new Free<A>.Fail(error);
        public static Free<Unit> SomeAsyncThing(string message) => new Free<Unit>.SomeAsyncThing(message, Return, Fail<Unit>);
    }

    public abstract partial class Free<A>
    {
        internal class Return : Free<A>
        {
            public readonly A Value;
            public Return(A value) => Value = value;
        }

        internal class Fail : Free<A>
        {
            public readonly Exception Exception;
            public Fail(Exception exception) => Exception = exception;
        }

        internal class SomeAsyncThing : Free<A>
        {
            public readonly string Message;
            public readonly Func<Unit, Free<A>> Next;
            public readonly Func<Exception, Free<A>> FailNext;
            public SomeAsyncThing(
                string message,
                Func<Unit, Free<A>> next,
                Func<Exception, Free<A>> failNext
            )
            {
                Message = message;
                Next = next;
                FailNext = failNext;
            }
        }

    }

    public static partial class FreeExtensions
    {

        public static Free<B> Bind<A, B>(this Free<A> ma, Func<A, Free<B>> f) => ma switch
        {
            Free<A>.Return rt => f(rt.Value),
            Free<A>.Fail fa => new Free<B>.Fail(fa.Exception),
            Free<A>.SomeAsyncThing a => new Free<B>.SomeAsyncThing(a.Message, n => a.Next(n).Map(f).Flatten(), e => a.FailNext(e).Map(f).Flatten()),
            _ => default
        };

        public static Free<B> BiBind<A, B>(this Free<A> ma, Func<A, Free<B>> Succ, Func<Exception, Free<B>> Fail) => ma switch
        {
            Free<A>.Return rt => Succ(rt.Value),
            Free<A>.Fail fa => Fail(fa.Exception),
            Free<A>.SomeAsyncThing a => new Free<B>.SomeAsyncThing(a.Message, n => a.Next(n).BiMap(Succ, Fail).Flatten(), e => a.FailNext(e).BiMap(Succ, Fail).Flatten()),
            _ => default
        };

        public static Free<B> Map<A, B>(this Free<A> ma, Func<A, B> f) => ma switch
        {
            Free<A>.Return rt => new Free<B>.Return(f(rt.Value)),
            Free<A>.Fail fa => new Free<B>.Fail(fa.Exception),
            Free<A>.SomeAsyncThing a => new Free<B>.SomeAsyncThing(a.Message, n => a.Next(n).Map(f), e => a.FailNext(e).Map(f)),
            _ => default
        };

        public static Free<B> BiMap<A, B>(this Free<A> ma, Func<A, B> Succ, Func<Exception, B> Fail) => ma switch
        {
            Free<A>.Return rt => new Free<B>.Return(Succ(rt.Value)),
            Free<A>.Fail fa => new Free<B>.Return(Fail(fa.Exception)),
            Free<A>.SomeAsyncThing a => new Free<B>.SomeAsyncThing(a.Message, n => a.Next(n).BiMap(Succ, Fail), e => a.FailNext(e).BiMap(Succ, Fail)),
            _ => default
        };

        public static Free<A> Flatten<A>(this Free<Free<A>> ma) =>
            ma.Bind(identity);

        public static Free<B> Select<A, B>(this Free<A> ma, Func<A, B> f) =>
            ma.Map(f);

        public static Free<C> SelectMany<A, B, C>(this Free<A> ma, Func<A, Free<B>> bind, Func<A, B, C> project) =>
            ma.Bind(a => bind(a).Select(b => project(a, b)));

    }

}
