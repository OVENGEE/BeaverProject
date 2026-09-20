using UnityEngine;

public class DestroyOnFinished : MonoBehaviour
{
    // Called by an animation or effect finish event to remove this object during play mode.
    public void Finished()
    {
        if(Application.isPlaying)
        {
            Destroy(gameObject);
        }
    }
    
}
