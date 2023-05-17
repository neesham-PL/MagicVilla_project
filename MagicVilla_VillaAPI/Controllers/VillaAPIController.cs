using AutoMapper;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System.Net;
//using MagicVilla_VillaAPI.Logging;

namespace MagicVilla_VillaAPI.Controllers

{
	[Route("api/v{version:apiVersion}/VillaAPI")]     //[Route("api/[controller]")]
	[ApiController]
	[ApiVersion("1.0")]   //[ApiNeutral]
	public class VillaAPIController : ControllerBase
	{
		//private readonly ILogging _logger;

		//public VillaAPIController(ILogging logger) 
		//{
		//    _logger = logger;
		//}
		protected APIResponse _response;
		private readonly IVillaRepository _dbVilla;
		private readonly IMapper _mapper;
		public VillaAPIController(IVillaRepository dbVilla, IMapper mapper)
		{
			_dbVilla = dbVilla;
			_mapper = mapper;
			_response = new();
		}
		[HttpGet]
		[ResponseCache(CacheProfileName = "Default30")]
		//[Authorize]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		public async Task<ActionResult<APIResponse>> GetVillas([FromQuery(Name = "filterOccupancy")] int? occupancy,
			[FromQuery] string? search, int pageSize = 0, int pageNumber = 1)
		{
			try
			{
				IEnumerable<Villa> villaList;
				if (occupancy > 0)
				{
					villaList = await _dbVilla.GetAllAsync(u => u.Occupancy == occupancy, pageSize: pageSize, pageNumber: pageNumber);
				}
				else
				{
					villaList = await _dbVilla.GetAllAsync(pageSize: pageSize,
						pageNumber: pageNumber);
				}
				if (!string.IsNullOrEmpty(search))
				{
					villaList = villaList.Where(u => u.Name.ToLower().Contains(search));
				}
				_response.Result = _mapper.Map<List<VillaDTO>>(villaList);
				_response.StatusCode = HttpStatusCode.OK;
				//Console.WriteLine("gh");
				return Ok(_response);

			}
			catch (Exception ex)
			{
				_response.IsSuccess = false;
				_response.ErrorMessages = new List<string>() { ex.ToString() };
			}
			return _response;
		}


		[HttpGet("id", Name = "GetVilla")]            //[HttpGet({"id = int"}] 
		[ProducesResponseType(StatusCodes.Status200OK)] //[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(VillaDTO))]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ResponseCache(Duration = 30)] //(Location=ResponseCacheLocation.None (=none means won't store), NoStore=true(=cache storing disabled)
		public async Task<ActionResult<APIResponse>> GetVilla(int id)
		{
			try
			{
				if (id == 0)
				{
					//  _logger.Log("Get Villa Error with Id" + id, "error");
					return BadRequest();
				}
				var villa = await _dbVilla.GetAsync(u => u.Id == id);
				if (villa == null)
				{
					return NotFound();
				}
				_response.Result = _mapper.Map<VillaDTO>(villa);
				_response.StatusCode = HttpStatusCode.OK;
				return Ok(_response);
			}
			catch (Exception ex)
			{
				_response.IsSuccess = false;
				_response.ErrorMessages = new List<string>() { ex.ToString() };
			}
			return _response;
		}


		[HttpPost]
		[Authorize(Roles = "admin")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<APIResponse>> CreateVilla([FromBody] VillaCreateDTO createDTO)
		{
			//if(!ModelState.IsValid)
			//{
			//    return BadRequest(ModelState);
			//}
			try
			{

				if (await _dbVilla.GetAsync(u => u.Name.ToLower() == createDTO.Name.ToLower()) != null)
				{
					ModelState.AddModelError("ErrorMessages", "Villa already Exits");
					return BadRequest(ModelState);
				}

				if (createDTO == null)
				{
					return BadRequest(createDTO);
				}
				//if (villaDTO.Id > 0)
				//{
				//    return BadRequest();
				//}
				Villa villa = _mapper.Map<Villa>(createDTO);  //mapper enabled us to auto map to model ratherthan -below
															  //Villa model = new()
															  //{
															  //    Name = createDTO.Name,
															  //    Amenity = createDTO.Amenity,
															  //    Details = createDTO.Details,
															  //    ImageUrl = createDTO.ImageUrl,
															  //    Occupancy = createDTO.Occupancy,
															  //    Rate = createDTO.Rate,
															  //    Sqft = createDTO.Sqft
															  //};
				await _dbVilla.CreateAsync(villa);


				// VillaStore.villaList.Add(villaDTO);
				_response.Result = _mapper.Map<VillaDTO>(villa);
				_response.StatusCode = HttpStatusCode.Created;
				return CreatedAtRoute("GetVilla", new { id = villa.Id }, _response);
			}
			catch (Exception ex)
			{
				_response.IsSuccess = false;
				_response.ErrorMessages = new List<string>() { ex.ToString() };
			}
			return _response;
		}

		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[HttpDelete("{id = int}", Name = "DeleteName")]
		[Authorize(Roles = "admin")]
		public async Task<ActionResult<APIResponse>> DeleteVilla(int id)
		{
			try
			{
				if (id == 0)
				{
					return BadRequest();
				}
				var villa = await _dbVilla.GetAsync(u => u.Id == id);
				if (villa == null)
				{
					return NotFound();
				}
				await _dbVilla.RemoveAsync(villa);
				_response.StatusCode = HttpStatusCode.NoContent;
				_response.IsSuccess = true;
				return Ok(_response);
			}
			catch (Exception ex)
			{
				_response.IsSuccess = false;
				_response.ErrorMessages = new List<string>() { ex.ToString() };
			}
			return _response;
		}

		[HttpPut("id={int}", Name = "UpdateVilla")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[Authorize(Roles = "admin")]
		public async Task<ActionResult<APIResponse>> UpdateVilla(int id, [FromBody] VillaUpdateDTO updateDTO)
		{
			try
			{
				if (updateDTO == null || id != updateDTO.Id)
				{
					return BadRequest();
				}
				Villa model = _mapper.Map<Villa>(updateDTO);
				//Villa model = new()
				//{
				//    Amenity = updateDTO.Amenity,
				//    Details = updateDTO.Details,
				//    Id = updateDTO.Id,
				//    ImageUrl = updateDTO.ImageUrl,
				//    Occupancy = updateDTO.Occupancy,
				//    Rate = updateDTO.Rate,
				//    Sqft = updateDTO.Sqft
				//};
				await _dbVilla.UpdateAsync(model);
				_response.StatusCode = HttpStatusCode.NoContent;
				_response.IsSuccess = true;
				return Ok(_response);

				//return NoContent();
			}
			catch (Exception ex)
			{
				_response.IsSuccess = false;
				_response.ErrorMessages = new List<string>() { ex.ToString() };
			}
			return _response;
		}

		[HttpPatch("{id:int}", Name = "UpdatePartialVilla")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]

		public async Task<ActionResult<APIResponse>> UpdatePartialVilla(int id, JsonPatchDocument<VillaUpdateDTO> patchDTO)
		{
			try
			{
				if (patchDTO == null || id == 0)
				{
					return BadRequest();
				}
				var villa = await _dbVilla.GetAsync(u => u.Id == id, tracked: false);

				VillaUpdateDTO villaDTO = _mapper.Map<VillaUpdateDTO>(villa);  //<destinatn>(source )                                            
				if (villa == null)
				{
					return BadRequest(id);
				}
				patchDTO.ApplyTo(villaDTO, ModelState);
				Villa model = _mapper.Map<Villa>(villaDTO);

				//Villa model = new()
				//{
				//    Amenity = villaDTO.Amenity,
				//    Details = villaDTO.Details,
				//    Id = villaDTO.Id,
				//    ImageUrl = villaDTO.ImageUrl,
				//    Occupancy = villaDTO.Occupancy,
				//    Rate = villaDTO.Rate,
				//    Sqft = villaDTO.Sqft
				//};
				await _dbVilla.UpdateAsync(model);
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}
				return NoContent();
			}
			catch (Exception ex)
			{
				_response.IsSuccess = false;
				_response.ErrorMessages = new List<string>() { ex.ToString() };
			}
			return _response;
		}

	}
}
