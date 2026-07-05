using UnityEngine;

[System.Serializable]
public abstract class Shape{
    public abstract DMVector GetReach();
}

[System.Serializable]
public class Square : Shape
{
    [SerializeField]
    public DMVector size;

    public override DMVector GetReach()
    {
        return size /2;
    }
}

[System.Serializable]
public class Circle : Shape
{
    [SerializeField]
    public DM64 radius;

    public override DMVector GetReach()
    {
        return new DMVector( radius, radius);
    }
}