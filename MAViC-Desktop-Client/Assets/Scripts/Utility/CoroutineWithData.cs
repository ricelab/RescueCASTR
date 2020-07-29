/**
 * Credit for this code goes to Ted Bigham: https://answers.unity.com/users/40491/ted-bigham.html
 * https://answers.unity.com/questions/24640/how-do-i-return-a-value-from-a-coroutine.html
 */

using System.Collections;
using UnityEngine;

public class CoroutineWithData
{
    public Coroutine coroutine { get; private set; }
    public object result;
    private IEnumerator target;
    public CoroutineWithData(MonoBehaviour owner, IEnumerator target)
    {
        this.target = target;
        this.coroutine = owner.StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        while (target.MoveNext())
        {
            result = target.Current;
            yield return result;
        }
    }
}
