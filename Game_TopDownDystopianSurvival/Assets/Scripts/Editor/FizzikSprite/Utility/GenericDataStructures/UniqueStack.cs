using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fizzik {
    /*
     * @author - Maxim Tiourin 
     *
     * A unique stack maintains a stack of elements T such that anytime an element is pushed onto the stack, 
     * if it already exists in the stack (determined by .Equals() comparison), it removes itself from its previous position while still being pushed.
     *
     * EX:
     *     3 -> 2 -> 1 -> 0
     *     Push(2)
     *     3 -> 1 -> 0 -> 2
     *     Pop() = 2
     *     3 -> 1 -> 0
     */
    public class UniqueStack<T> {
        const int DEFAULT_SIZE = 10;
        const int DEFAULT_ALLOCATION = 10;

        private T[] stack;
        private int lra; //lra == (e + 1) where e is the index of the least recently added element, lra == 0 when array is empty, lra == 1 when array has one element
        private int initialSize; //The initial size of the stack array, MIN = DEFAULT_SIZE
        private int allocationSize; //The size increment the stack array should grow when an allocation is needed, MIN = DEFAULT_ALLOCATION

        public UniqueStack(int initialSize = DEFAULT_SIZE, int allocationSize = DEFAULT_ALLOCATION) {
            this.initialSize = Mathf.Min(DEFAULT_SIZE, initialSize);
            this.allocationSize = Mathf.Min(DEFAULT_ALLOCATION, allocationSize);

            clear();
        }

        /*
         * Pushes the element onto the stack, purging any previous occurence in the stack if it existed.
         */
        public void push(T obj) {
            //Easy case, element is already at top of the stack
            if (lra > 0 && stack[0].Equals(obj)) {
                return;
            }

            int i = indexOf(obj);
            if (i > 0) {
                //Swap element
                swap(i);
            }
            else if (i < 0) {
                //Push
                for (int n = lra; n > 0; n--) {
                    stack[n] = stack[n - 1];
                }

                stack[0] = obj;

                lra++;
            }

            if (lra >= stack.Length) {
                allocate();
            }
        }

        /*
         * Pops the most recent element off of the stack and returns it, returns default(T) if there are no elements to pop, so be sure to check stack size ahead of this call.
         */
        public T pop() {
            if (lra > 0) {
                T obj = stack[0];

                for (int i = 0; i < lra; i++) {
                    stack[i] = stack[i + 1];
                }

                lra--;

                return obj;
            }
            else {
                return default(T);
            }
        }

        /*
         * Returns the element at the top of the stack, without removing it, returns default(T) if there are no elements to peek, so be sure to check stack size ahead of this call
         */
        public T peek() {
            if (lra > 0) {
                return stack[0];
            }
            else {
                return default(T);
            }
        }

        /*
         * Returns the element at the bottom of the stack, without removing it, returns default(T) if there are no elements to peek, so be sure to check stack size ahead of this call
         */
        public T reversePeek() {
            if (lra > 0) {
                return stack[lra - 1];
            }
            else {
                return default(T);
            }
        }

        /*
         * Returns an array representation of the stack of size = size(), or of size = index if index >= 0, returning the subarray [0, index - 1]
         */
        public T[] toArray(int index = -1) {
            int size = lra;

            if (index > 0) {
                size = index;
            }
            else if (index == 0) {
                return null;
            }

            T[] arr = new T[size];

            for (int i = 0; i < size; i++) {
                arr[i] = stack[i];
            }

            return arr;
        }

        private void swap(int swapIndex) {
            T obj = stack[swapIndex];

            for (int i = swapIndex; i > 0; i--) {
                stack[i] = stack[i - 1];
            }

            stack[0] = obj;
        }

        /*
         * Returns the amount of elements in the stack
         */
        public int size() {
            return lra;
        }

        /*
         * Clears the stack and resets the internal array size to 'initialSize'
         */
        public void clear() {
            stack = new T[initialSize];
            lra = 0;
        }

        /*
         * Returns true if the stack contains the element
         */
        public bool contains(T obj) {
            if (indexOf(obj) >= 0) {
                return true;
            }
            else {
                return false;
            }
        }

        private int indexOf(T obj) {
            for (int i = 0; i < lra; i++) {
                if (stack[i].Equals(obj)) {
                    return i;
                }
            }

            return -1;
        }

        private void allocate() {
            T[] newstack = new T[stack.Length + allocationSize];
            
            for (int i = 0; i < stack.Length; i++) {
                newstack[i] = stack[i];
            }

            stack = newstack;
        }
    }
}
