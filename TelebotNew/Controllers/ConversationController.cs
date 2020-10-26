using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TelebotNew.Implementations;
using TelebotNew.Implementations.RestaurantReservation;

namespace TelebotNew.Controllers
{
    public class ConversationController : Controller
    {
        [HttpGet]
        [Route("api/[controller]/create")]
        public async Task<IActionResult> CreateAsync()
        {
            RestaurantReservationConversationManager conversationManager = new RestaurantReservationConversationManager();
            conversationManager.Entities[0].UpdateValue("Harry", conversationManager.Entities);
            return Ok();
        }

        // Think more carefully about how to create the API. Do I have a separate "event" url?
    }
}
