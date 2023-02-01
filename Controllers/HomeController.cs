using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using mvc_dotnet.Models;
using System.Data.SqlClient;

namespace mvc_dotnet.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        ViewData["hieu"] = "asc";
        string connString = "Server=tcp:lvhoan.database.windows.net,1433;Initial Catalog=lvhoan;Persist Security Info=False;User ID=lvhoan;Password=Leviethoan2001;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        try
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select * from test", conn); 

                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Console.WriteLine(reader["name"].ToString());
                } 
                conn.Close();
            }
        }
        catch (Exception ex)
        {
            //display error message
            Console.WriteLine("Exception: " + ex.Message);
        }
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
