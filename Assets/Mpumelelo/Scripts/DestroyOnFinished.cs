using UnityEngine;

public class DestroyOnFinished : MonoBehaviour
{
    public void Finished()
    {
        if(Application.isPlaying)
        {
            Destroy(gameObject);
        }
    }
    
}
