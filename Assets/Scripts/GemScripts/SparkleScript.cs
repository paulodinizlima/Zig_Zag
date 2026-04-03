using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SparkleScript : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(DeactiveAfterTime());
    }

    IEnumerator DeactiveAfterTime()
	{
        yield return new WaitForSeconds(1.5f);
        gameObject.SetActive(false);
	}
}
