using UnityEngine;
public class SfxSmokeTest : MonoBehaviour
{
    public bool testShoot, testHurt, testPickup, testGameOver;

    void Update()
    {
        if (testShoot)  { testShoot = false;  SfxManager.Instance?.PlayShoot(); }
        if (testHurt)   { testHurt  = false;  SfxManager.Instance?.PlayHurt(); }
        if (testPickup) { testPickup= false;  SfxManager.Instance?.PlayPickup(); }
        if (testGameOver){testGameOver=false; SfxManager.Instance?.PlayGameOver(); }
    }
}
