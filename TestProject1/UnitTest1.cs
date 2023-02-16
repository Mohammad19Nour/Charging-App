using ChargingApp.Controllers;
using ChargingApp.Data;
using ChargingApp.DTOs;
using ChargingApp.Interfaces;
using Moq;

namespace TestProject1;
public class Tests
{
    [SetUp]
    public void Setup()
    {
        // _mock = new Mock<IUnitOfWork>();
       /* _mock.Setup(uow=>uow.CategoryRepository.GetAllCategoriesAsync())
            .ReturnsAsync(new List<CategoryDto>
            {
                new CategoryDto
                {
                    Id = 1,
                    ArabicName = "rfr",
                    EnglishName = "frfr",
                    Available = true
                },
                new CategoryDto
                {
                    Id = 2,
                    ArabicName = "rfr",
                    EnglishName = "frfr",
                    Available = true
                },
            });
*/
    }

    [Test]
    public async Task Test1()
    {
        var mock = new Mock<IUnitOfWork>();
        var controler = new HomeController(mock.Object);
        var res = await controler.GetHomePage();
        Assert.AreEqual(2,res.Value.Categories.Count);
    }
    
}