using System;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    
    /*
    This basically handles calling animations, 
    regardless of whether this is a sprite based or model based character
    */

    public enum AnimationType
    {
       Sprite,
       Model 
    }
    public AnimationType animationType = AnimationType.Sprite;
    

    public void PlayAnimation()
    {
        
    }



}
