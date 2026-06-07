using UnityEngine;

[System.Serializable]
public abstract class Shape{}

[System.Serializable]
public class Square : Shape
{
    [SerializeField]
    public DMVector size;
}

[System.Serializable]
public class Circle : Shape
{
    [SerializeField]
    public DM64 radius;
}