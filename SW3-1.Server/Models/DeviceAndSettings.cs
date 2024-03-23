namespace SW3_1.Server.Models
{
  public class DeviceAndSettingsWithAnalyzedData : DeviceAndSettings
  {
    public DeviceAndSettingsWithAnalyzedData(DeviceAndSettings deviceAndSettings)
    {
      Id = deviceAndSettings.Id;
      Name = deviceAndSettings.Name;
      DataAnnotations = deviceAndSettings.DataAnnotations;
      LatestData = deviceAndSettings.LatestData;
    }

    /// <summary>
    /// correlates by index to the latest data 
    /// </summary>
    public IEnumerable<AnalyzedData> AnalyzedData { get; set; } = new List<AnalyzedData>();
  }

  public class DeviceAndSettings
  {
    public Guid Id { get; set; }
    public string Name { get; set; }
    public IEnumerable<DataAnnotations> DataAnnotations { get; set; }
    public IEnumerable<DataWithTime> LatestData { get; set; } = new List<DataWithTime>();
  }

  public class DataAnnotations
  {
    public string Name { get; set; }
    public string Unit { get; set; }
  }
}