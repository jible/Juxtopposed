using System;
using UnityEngine;

public class AnimationManager : ITickable
{

    //     /*
    //     This basically handles calling animations, 
    //     regardless of whether this is a sprite based or model based character
    //     */

    //     public enum AnimationType
    //     {
    //        Sprite,
    //        Model 
    //     }
    //     public AnimationType animationType = AnimationType.Sprite;


    //     public void PlayAnimation()
    //     {

    //     }

    //     public void Tick(){
    //         // If you did not start an anim this tick, step forward a tick of the anim.
    //     }


    // }


    // public interface ICustomAnimTrack{
    //     // Stores array of keys 
    //     public list<int> keys; 
    // }

    // public class PropertyAnimTrack<PropertyType>: CustomAnimTrack{
    //     // Stores array of keys
    //     dictionary<int, PropertyType> keys = new Dictionary<int, PropertyType>(); 

    //     public void CallKey(int key){
    //         keys.tryGetValue(key, out PropertyType value);
    //         if (value == null){
    //             return;
    //         }


    //     }
    public void Tick()
    {
        return;
    }
}