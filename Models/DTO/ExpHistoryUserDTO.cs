namespace Diplom.Models.DTO;


public class ExpHistoryUserDTO
{
    public List<DataExpHistoryDTO> Data { get; set; }
}

public class DataExpHistoryDTO{
    public int Value { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Discription { get; set; }
}