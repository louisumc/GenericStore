using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenericStore.WebApi.Controllers
{
    [Authorize]
    public class BaseController : Controller
    {
      
    }
}
