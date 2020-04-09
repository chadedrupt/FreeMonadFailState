
namespace FreeMonadFailState
{
    internal class Node<A>
    {

        public enum StateType
        {
            Leaf,
            Inner
        }

        public readonly StateType State;
        public readonly A Value;
        public readonly Free<A> Next;
        public bool IsLeaf => State == StateType.Leaf;
        public bool IsInner => State == StateType.Inner;

        public Node(A value)
        {
            State = StateType.Leaf;
            Value = value;
            Next = default;
        }

        public Node(Free<A> next)
        {
            State = StateType.Inner;
            Value = default;
            Next = next;
        }

    }

    internal static class Prelude
    {
        public static Node<A> Node<A>(A value) => new Node<A>(value);
        public static Node<A> Node<A>(Free<A> next) => new Node<A>(next);
    }
}
