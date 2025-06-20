public interface IPausableTick
{
    public bool Registered { get; set; }
    void Tick();
}
