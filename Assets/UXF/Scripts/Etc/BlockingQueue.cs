using System.Threading;
using System.Collections.Generic;
using System.Collections;
using System;


/// <summary>
/// Queue class used to manage multithreaded file IO.
/// </summary>
/// <typeparam name="T"></typeparam>
class BlockingQueue<T> : IEnumerable<T>
{
    private int _count = 0;

    private Queue<T> _queue = new Queue<T>();


    public T Dequeue()
    {
        lock (_queue)
        {
            // If we have items remaining in the queue, skip over this. 
            while (_count <= 0)
            {
                // Release the lock and block on this line until someone
                // adds something to the queue, resuming once they 
                // release the lock again.
                Monitor.Wait(_queue);
            }

            _count--;

            return _queue.Dequeue();
        }
    }

    public void Enqueue(T data)
    {
        if (data == null) throw new ArgumentNullException("data");

        lock (_queue)
        {
            _queue.Enqueue(data);

            _count++;

            // If the consumer thread is waiting for an item
            // to be added to the queue, this will move it
            // to a waiting list, to resume execution
            // once we release our lock.
            Monitor.Pulse(_queue);
        }
    }

    // Lets the consumer thread consume the queue with a foreach loop.
    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        while (true) yield return Dequeue();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable<T>)this).GetEnumerator();
    }

    public int NumItems()
    {
        return _count;
    }

}
