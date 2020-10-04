using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerPersist : MonoBehaviour
{
    /* Script ensures that persistence of the timer when completing a level does not produce duplicate game objects. If another instance of a LevelCanvas (containing the timer) is detected,
     * the instance is destroyed.
     */
    private static TimerPersist timerInstance;

    void Start()
    {

        if (timerInstance == null)
        {
            timerInstance = this;
        }
        else if (timerInstance != null)
        {
            Destroy(this.gameObject);
        }

    }



}
