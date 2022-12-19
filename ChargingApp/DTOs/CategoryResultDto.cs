using System.Collections;

namespace ChargingApp.DTOs;

public class CategoryResultDto 
{
    public CategoryWithProductsDto? Category { get; set; }
    public IEnumerator GetEnumerator()
    {
        throw new NotImplementedException();
    }
}