using System;
using FreeMonadFailState;
using NUnit.Framework;
using LanguageExt;
using static LanguageExt.Prelude;
using System.Threading.Tasks;

namespace FreeMonadFailStateTests
{
    public class Tests
    {
        [Test]
        public async Task TestBindStillShortCutsOnFailure()
        {
            var count = 0;
            var props = new InterpreterProps(
                DoSomeAsyncThing: _ => {
                    count++;
                    return TryAsync<Unit>(new Exception());
                }
            );

            var tree =
                from _ in Free.SomeAsyncThing("This will fail")
                from __ in Free.SomeAsyncThing("So this shouldn't be called")
                select __;

            var result = await tree
                .Interpret(props)
                .Try();

            Assert.That(result.IsFaulted);
            Assert.AreEqual(count, 1);
        }

        [Test]
        public async Task TestRecoveringFromFailureWithBiBind()
        {
            var count = 0;

            var props = new InterpreterProps(
                DoSomeAsyncThing: _ => {
                    count++;
                    return TryAsync<Unit>(new Exception());
                }
            );

            var valueToRecoverWith = 4;

            var tree = Free.SomeAsyncThing("This will fail")
                .Bind(_ => Free.SomeAsyncThing("So this shouldn't be called"))
                .Bind(_ => Free.Return(1))
                .BiBind(
                    Succ: Free.Return,
                    Fail: _ => Free.Return(valueToRecoverWith)
                );

            var result = await tree
                .Interpret(props)
                .IfFailThrow();

            Assert.AreEqual(result, valueToRecoverWith);
            Assert.AreEqual(count, 1);
        }

        [Test]
        public async Task TestRecoveringFromDirectFailureWithBiBind()
        {
            var count = 0;

            var props = new InterpreterProps(
                DoSomeAsyncThing: _ => {
                    count++;
                    return TryAsync(unit);
                }
            );

            var valueToRecoverWith = 3;

            var tree = Free.SomeAsyncThing("This should succeed")
                .Bind(_ => Free.Fail<Unit>(new Exception("Some direct error will occur here")))
                .Bind(_ => Free.SomeAsyncThing("So this shouldn't be called"))
                .BiBind(
                    Succ: _ => Free.Return(1),
                    Fail: _ => Free.Return(valueToRecoverWith)
                );

            var result = await tree
                .Interpret(props)
                .IfFailThrow();

            Assert.AreEqual(result, valueToRecoverWith);
            Assert.AreEqual(count, 1);
        }

        [Test]
        public async Task TestNormalBind()
        {
            var props = new InterpreterProps(
                DoSomeAsyncThing: _ => TryAsync(unit)
            );

            var value = 4;

            var tree =
                from _ in Free.SomeAsyncThing("This should succeed")
                from after in Free.Return(value)
                select after;

            var result = await tree
                .Interpret(props)
                .IfFailThrow();

            Assert.AreEqual(result, value);
        }

    }
}
