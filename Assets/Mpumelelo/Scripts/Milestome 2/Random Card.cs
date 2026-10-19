using System.Collections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic; 

public class Card : MonoBehaviour
{
    [SerializeField] GameObject[] cardPrefab;
    [SerializeField] float secondSpawn = 0.5f;
    [SerializeField] float minTras;
    [SerializeField] float maxTras;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(CardSpawn());
    }

    IEnumerator CardSpawn()
    {
        while(true)
        {
            var wanted =  Random.Range(minTras, maxTras);
            var position = new Vector3(wanted, transform.position.y);
            GameObject gameObject = Instantiate(cardPrefab[Random.Range(0 , cardPrefab.Length)],position, Quaternion.identity );
            yield return new WaitForSeconds(secondSpawn);
            Destroy(gameObject, 5f);
        }
    }
}
