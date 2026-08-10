using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireFly;

/// <summary>Maintains the maximum of a FIFO window under the default comparer with amortized O(1) updates. Removed prefixes remain allocated, so space is O(the total number of Add calls).</summary>
public class SlidingWindowMax<T>
{
    private List<T> _payload;
    private List<int> _queue;
    private int _pLeft, _qLeft;

    /// <summary>Creates an empty window.</summary>
    public SlidingWindowMax() {
        _payload = new();
        _queue = new();
        _pLeft = _qLeft = 0;
    }

    /// <summary>Adds an item to the back of the window in amortized O(1) time.</summary>
    /// <param name="item">The item to add.</param>
    public void Add(T item) {
        while (_qLeft < _queue.Count && Comparer<T>.Default.Compare(_payload[_queue[^1]], item) <= 0) {
            _queue.RemoveAt(_queue.Count - 1);
        }
        _queue.Add(_payload.Count);
        _payload.Add(item);
    }

    /// <summary>Removes the oldest item in O(1) time, or does nothing when the window is empty.</summary>
    public void Remove() {
        if (_pLeft == _payload.Count) return;
        if (_queue[_qLeft] == _pLeft) {
            ++_qLeft;
        }
        ++_pLeft;
    }

    /// <summary>Gets the maximum item of the nonempty window in O(1) time.</summary>
    public T Max {
        get {
            if (_qLeft >= _queue.Count) throw new InvalidOperationException();
            return _payload[_queue[_qLeft]];
        }
    }
}
