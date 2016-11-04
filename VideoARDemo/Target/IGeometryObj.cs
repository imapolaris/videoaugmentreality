namespace VideoARDemo.Target
{
    public interface IGeometryObj
    {
        double TransformHeading { set; }
        double OpacityInfo { get; set; }
        bool IsFill { get; set; }
    }
}