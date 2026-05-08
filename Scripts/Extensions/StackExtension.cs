using System.Collections.Generic;

namespace Local.Scripts.Extensions
{
    public static class StackExtension
    {
        public static void RemoveElement<T>(this Stack<T> stack, T elementToRemove)
        {
            Stack<T> tempStack = new Stack<T>();

            while (stack.Count > 0)
            {
                T topElement = stack.Pop();

                if (ReferenceEquals(topElement, elementToRemove))
                {
                    // Element found and removed, now put the rest back
                    while (tempStack.Count > 0)
                    {
                        stack.Push(tempStack.Pop());
                    }
                    return;
                }
                else
                {
                    tempStack.Push(topElement);
                }
            }

            // If we reach here, the element was not found. Put everything back.
            while (tempStack.Count > 0)
            {
                stack.Push(tempStack.Pop());
            }

            // Optionally, handle the case where the element was not found
        }
    }
}