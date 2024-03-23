namespace SW3_shared.Models
{
  public class IncomingData
  {
    public Guid DeviceId { get; set; }
    public IEnumerable<float> Data { get; set; } = new List<float>();
    public DateTime CreatedAt = DateTime.UtcNow;

  }
}
