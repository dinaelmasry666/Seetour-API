﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SeetourAPI.BL.TourAnswerManager;
using SeetourAPI.BL.TourManger;
using SeetourAPI.Data.Models;

namespace SeetourAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TourAnswerController : ControllerBase
    {
        private readonly ITourAnswerManager ITourAnswerManager;
        public TourAnswerController(ITourAnswerManager ITourAnswerManager)
        {
            this.ITourAnswerManager = ITourAnswerManager;

        }
        [HttpGet("GetAll")]
        public ActionResult GetAll()
        {
            var ans  = ITourAnswerManager.GetAll();
            if(ans == null)
            {
                return NotFound();
            }
            return Ok(ITourAnswerManager.GetAll());
        }

        [HttpGet("id")]
        public IActionResult GetById(int id)
        {
            var ans = ITourAnswerManager.GetAnswerById(id);
            if (ans == null)
            {
                return BadRequest();
            }
            return Ok(ans);
        }


        [HttpPost]
        public IActionResult createTourAnswer(TourAnswer tourAnswer)
        {
            ITourAnswerManager.AddAnswer(tourAnswer);
            return Created("Created Successfully", tourAnswer);
        }


        [HttpPut]
        public IActionResult EditAnswer(int id, TourAnswer tourAnswer)
        {
            if (tourAnswer.Id != id)
            {
                return BadRequest();
            }
            return Ok();
        }


        [HttpDelete]
        public IActionResult DeleteTourAnswer(int id)
        {
            ITourAnswerManager.DeleteAnswer(id);
            return NoContent();
        }
    }
}
